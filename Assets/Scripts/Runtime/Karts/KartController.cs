using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

namespace Runtime.Karts
{
    [RequireComponent(typeof(Rigidbody))]
    public class KartController : NetworkBehaviour
    {
        public float maxForwardSpeedKmpH = 140f;
        public float maxReverseSpeedKmpH = 20f;
        public float enginePower = 50f;
        public float brakePower = 50f;
        public float steerAngleMin = 20f;
        public float steerAngleMax = 6f;
        public AnimationCurve accelerationCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Space]
        public AnimationCurve tangentFriction;

        [Space]
        public float airControlSpeed = 4f;
        public float airControlAcceleration = 2f;

        [Space]
        public float jumpForce = 14f;
        public Vector3 checkForGroundRadius;
        public Vector3 checkForGroundOffset;
        public Vector2Int checkForGroundIterations;

        [Space]
        public float currentCounterFlipTorque;

        [Space]
        public ParticleSystem jumpFx;
        public ParticleSystem sparks;
        public float sparksThreshold;
        public float lastImpactValue;

        [Space]
        public float currentSpeed;
        [Range(-1f, 1f)]
        public float currentThrottle;
        [Range(-1f, 1f)]
        public float targetThrottle;
        public float throttleSpeed;
        [Range(-1f, 1f)]
        public float currentSteering;
        [Range(-1f, 1f)]
        public float targetSteering;
        public float steeringSpeed;
        [Range(0f, 1f)]
        public float braking;
        public Vector3 airControl;
        public bool jump;
        [Range(0f, 1f)]
        public float slip;

        public bool brakeLights { get; private set; }

        private int groundedWheelsCount;

        private WheelController[] wheels;

        public Vector3 lastWheelGroundDirection = Vector3.up;
        public Vector3 lastGroundDirection = Vector3.up;

        public Rigidbody body { get; private set; }
        public float signedForwardSpeed { get; private set; }
        public bool activeViewer { get; set; }
        public float maxForwardSpeed => maxForwardSpeedKmpH / 3.6f;
        public float maxReverseSpeed => maxReverseSpeedKmpH / 3.6f;
        public bool wheelsOnGround => groundedWheelsCount >= wheels.Length - 1;
        public bool onGround { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            wheels = GetComponentsInChildren<WheelController>();
        }

        public override void OnStartNetwork()
        {
            TimeManager.OnTick += OnTick;
            TimeManager.OnPostTick += OnPostTick;

            activeViewer = Owner.IsLocalClient;
        }

        public override void OnStopNetwork()
        {
            TimeManager.OnTick += OnTick;
            TimeManager.OnPostTick += OnPostTick;
        }

        private void OnTick()
        {
            Simulate(CreateReplicationData());
            if (IsServerStarted)
            {
                if (body.position.y < -50f)
                {
                    body.position = new Vector3(0f, 1f, 0f);
                    body.rotation = Quaternion.identity;

                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }
        }

        [Replicate]
        private void Simulate(ReplicateData data, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            currentThrottle = Mathf.MoveTowards(currentThrottle, data.targetThrottle, throttleSpeed * Time.fixedDeltaTime);
            currentSteering = Mathf.MoveTowards(currentSteering, data.targetSteering, steeringSpeed * Time.fixedDeltaTime);

            signedForwardSpeed = Vector3.Dot(transform.forward, body.linearVelocity);

            if (Mathf.Abs(signedForwardSpeed) < 0.1f)
            {
                body.linearVelocity -= Vector3.Project(body.linearVelocity, transform.forward);
            }

            CheckIfGrounded(data);
            LookForGround();
            ApplyThrottle(ref data);
            ApplyTangentFriction();
            ApplyAirControl(data);
            Jump(data, state == ReplicateState.CurrentCreated);

            groundedWheelsCount = 0;
            foreach (var wheel in wheels)
            {
                wheel.steerAngle = currentSteering * Mathf.Lerp(steerAngleMin, steerAngleMax, Mathf.Abs(signedForwardSpeed / maxForwardSpeed));
                wheel.Simulate();
                if (wheel.onGround) groundedWheelsCount++;
            }

            currentSpeed = Mathf.Round(signedForwardSpeed * 3.6f);

            if (state == ReplicateState.CurrentCreated)
            {
                brakeLights = data.braking > 0.1f;
            }
        }

        private void CheckIfGrounded(ReplicateData data)
        {
            var ix = checkForGroundIterations.x;
            var iy = checkForGroundIterations.y;
            var hits = new RaycastHit?[ix * iy];
            var center = transform.TransformPoint(checkForGroundOffset);
            for (var x = 0; x < ix; x++)
            {
                for (var y = 0; y < iy; y++)
                {
                    var ax = x / (float)ix * 360f;
                    var ay = y / (iy - 1f) * 180f;
                    var vector = Vector3.Scale(Quaternion.Euler(ax, ay, 0f) * Vector3.forward, checkForGroundRadius);
                    var length = vector.magnitude;
                    vector = vector.normalized;
                    if (Physics.Raycast(new Ray(center, transform.rotation * vector), out var hit, length))
                    {
                        hits[x * iy + y] = hit;
                    }
                }
            }

            var bestHit = (RaycastHit?)null;
            for (var i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.HasValue)
                {
                    if (!bestHit.HasValue) bestHit = hit;
                    else if ((hit.Value.point - center).sqrMagnitude < (bestHit.Value.point - center).sqrMagnitude) bestHit = hit;
                }
            }

            if (bestHit.HasValue)
            {
                onGround = true;
                lastGroundDirection = bestHit.Value.normal;
            }
            else
            {
                onGround = false;
            }
        }

        private void Jump(ReplicateData data, bool current)
        {
            if (onGround && data.jump)
            {
                body.linearVelocity += (wheelsOnGround ? lastWheelGroundDirection : lastGroundDirection) * jumpForce;
                if (current) jumpFx.Play(true);
            }
        }

        private void ApplyAirControl(ReplicateData data)
        {
            if (wheelsOnGround) return;

            var input = body.rotation * Vector3.ClampMagnitude(data.airControl, 1f);
            var torque = (input * airControlSpeed - body.angularVelocity) * airControlAcceleration;
            body.angularVelocity += torque * Time.fixedDeltaTime;
        }

        private void ApplyTangentFriction()
        {
            if (!wheelsOnGround) return;

            var min = float.MaxValue;
            var max = float.MinValue;
            foreach (var wheel in wheels)
            {
                var point = wheel.transform.position - body.worldCenterOfMass;
                if (point.z < min) min = point.z;
                if (point.z > max) max = point.z;
            }

            var sideSpeed = Mathf.Abs(Vector3.Dot(body.linearVelocity, transform.right));
            var forwardSpeed = Mathf.Abs(signedForwardSpeed);
            slip = sideSpeed + forwardSpeed > 0.1f ? sideSpeed / (sideSpeed + forwardSpeed) : 0f;

            var distance = Mathf.Max(0f, max - min);
            ApplyTangentFriction(slip, distance * -0.5f, 0f, 0.5f);

            var steerAngle = Mathf.Lerp(steerAngleMin, steerAngleMax, Mathf.Abs(signedForwardSpeed) / maxForwardSpeed) * currentSteering;
            ApplyTangentFriction(slip, distance * 0.5f, steerAngle, 0.5f);
        }

        private void ApplyTangentFriction(float slip, float distance, float angleDeg, float scaleFactor)
        {
            var angleRad = angleDeg * Mathf.Deg2Rad;

            var point = body.worldCenterOfMass + transform.forward * distance;
            var tangent = transform.right * Mathf.Cos(angleRad) + transform.forward * -Mathf.Sin(angleRad);
            var velocity = body.GetPointVelocity(point);

            var deltaV = Vector3.Project(-velocity, tangent) * tangentFriction.Evaluate(slip) * scaleFactor;

            Debug.DrawRay(point, tangent * 2f + deltaV, Color.red);
            Debug.DrawRay(point, tangent * 2f, Color.blue);

            body.linearVelocity += deltaV;
            body.angularVelocity += Vector3.Cross(point - body.worldCenterOfMass, deltaV);
        }

        private void LookForGround()
        {
            var average = Vector3.zero;
            var wheelCount = 0;
            for (var i = 0; i < wheels.Length; i++)
            {
                var wheel = wheels[i];
                if (wheel.onGround)
                {
                    average += wheel.groundHit.normal;
                    wheelCount++;
                }
            }

            if (wheelCount == 0) return;
            lastWheelGroundDirection = average.normalized;
        }

        private void ApplyThrottle(ref ReplicateData data)
        {
            if (groundedWheelsCount < 2) return;

            if (currentThrottle * signedForwardSpeed < 0f && Mathf.Abs(signedForwardSpeed) > 5f)
            {
                data.braking = 1f;
            }

            var maxSpeed = signedForwardSpeed > 0f ? maxForwardSpeed : maxReverseSpeed;
            var throttleForce = currentThrottle * accelerationCurve.Evaluate(Mathf.Abs(signedForwardSpeed) / maxSpeed) * enginePower;
            var brakeForce = MathHelper.ClosestToZero(-signedForwardSpeed / Time.fixedDeltaTime, -Mathf.Sign(signedForwardSpeed) * brakePower);
            var force = Mathf.Lerp(throttleForce, brakeForce, data.braking);
            body.linearVelocity += transform.forward * force * Time.fixedDeltaTime;
        }

        private ReplicateData CreateReplicationData()
        {
            if (!IsOwner) return default;

            var data = new ReplicateData
            {
                targetThrottle = targetThrottle,
                targetSteering = targetSteering,
                braking = braking,
                airControl = airControl,
                jump = jump,
            };

            jump = false;
            return data;
        }

        private void OnPostTick() { CreateReconcile(); }

        public override void CreateReconcile()
        {
            var data = new ReconcileData
            {
                position = body.position,
                linearVelocity = body.linearVelocity,
                rotation = body.rotation,
                angularVelocity = body.angularVelocity,
                currentThrottle = currentThrottle,
                currentSteering = currentSteering,
                currentCounterFlipTorque = currentCounterFlipTorque,
            };
            ReconcileState(data);
        }

        [Reconcile]
        private void ReconcileState(ReconcileData data, Channel channel = Channel.Unreliable)
        {
            body.transform.position = data.position;
            body.transform.rotation = data.rotation;
            body.linearVelocity = data.linearVelocity;
            body.angularVelocity = data.angularVelocity;

            currentThrottle = data.currentThrottle;
            currentSteering = data.currentSteering;

            currentCounterFlipTorque = data.currentCounterFlipTorque;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0.0f, 1.0f);

            var body = GetComponent<Rigidbody>();
            Gizmos.DrawSphere(body.worldCenterOfMass, 0.1f);

            Gizmos.color = Color.green;
            var ix = checkForGroundIterations.x;
            var iy = checkForGroundIterations.y;
            var center = transform.TransformPoint(checkForGroundOffset);
            for (var x = 0; x < ix; x++)
            {
                for (var y = 0; y < iy; y++)
                {
                    var ax = x / (float)ix * 360f;
                    var ay = y / (iy - 1f) * 180f;
                    var vector = Vector3.Scale(Quaternion.Euler(ax, ay, 0f) * Vector3.forward, checkForGroundRadius);
                    var length = vector.magnitude;
                    vector = vector.normalized;
                    Gizmos.DrawSphere(center + (transform.rotation * vector) * length, 0.02f);
                }
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            for (var i = 0; i < other.contactCount; i++)
            {
                var contact = other.GetContact(i);
                var impulse = other.impulse.magnitude / body.mass;
                lastImpactValue = impulse;
                if (impulse > sparksThreshold)
                {
                    sparks.transform.position = contact.point;
                    sparks.Play();
                }
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            maxForwardSpeedKmpH = Mathf.Max(0f, maxForwardSpeedKmpH);
            steerAngleMax = Mathf.Max(0f, steerAngleMax);
            steerAngleMin = Mathf.Max(0f, steerAngleMin);
            enginePower = Mathf.Max(0f, enginePower);
        }

        public struct ReplicateData : IReplicateData
        {
            public float targetThrottle;
            public float targetSteering;
            public float braking;
            public Vector3 airControl;
            public bool jump;

            private uint tick;
            public void Dispose() { }
            public uint GetTick() => tick;
            public void SetTick(uint value) => tick = value;
        }

        public struct ReconcileData : IReconcileData
        {
            public Vector3 position;
            public Vector3 linearVelocity;
            public Quaternion rotation;
            public Vector3 angularVelocity;

            public float currentThrottle;
            public float currentSteering;

            public float currentCounterFlipTorque;

            private uint tick;
            public void Dispose() { }
            public uint GetTick() => tick;
            public void SetTick(uint value) => tick = value;
        }
    }
}