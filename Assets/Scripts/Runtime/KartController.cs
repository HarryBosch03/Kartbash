using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

namespace Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    public class KartController : NetworkBehaviour
    {
        public float maxForwardSpeedKmpH;
        public float maxReverseSpeedKmpH;
        public float accelerationTime;
        public float steerAngleMin;
        public float steerAngleMax;

        [Space]
        public float counterFlipTorqueStart;
        public float counterFlipTorqueIncrease;
        public float currentCounterFlipTorque;
        
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

        private int wheelsOnGround;

        private WheelCollider[] wheels;

        public Vector3 lastGroundDirection = Vector3.up;
        
        public Rigidbody body { get; private set; }
        public float signedForwardSpeed { get; private set; }
        public bool activeViewer { get; set; }
        public float maxForwardSpeed => maxForwardSpeedKmpH / 3.6f;
        public float maxReverseSpeed => maxReverseSpeedKmpH / 3.6f;
        public bool onGround => wheelsOnGround >= wheels.Length - 1;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            wheels = GetComponentsInChildren<WheelCollider>();
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
        }

        [Replicate]
        private void Simulate(ReplicateData data, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            if (!IsOwner)
            {
                targetThrottle = data.targetThrottle;
                targetSteering = data.targetSteering;
                braking = data.braking;
            }
            
            var forceSlip = false;
            if (data.braking > 0.1f)
            {
                if (data.targetThrottle < 0.1f)
                {
                    data.targetThrottle = Mathf.Lerp(data.targetThrottle, -Mathf.Clamp(signedForwardSpeed, -1f, 1f), data.braking);
                }
                else
                {
                    forceSlip = true;
                }
            }
            
            currentThrottle = Mathf.MoveTowards(currentThrottle, data.targetThrottle, throttleSpeed * Time.fixedDeltaTime);
            currentSteering = Mathf.MoveTowards(currentSteering, data.targetSteering, steeringSpeed * Time.fixedDeltaTime);

            signedForwardSpeed = Vector3.Dot(transform.forward, body.linearVelocity);

            if (Mathf.Abs(signedForwardSpeed) < 0.1f)
            {
                body.linearVelocity -= Vector3.Project(body.linearVelocity, transform.forward);
            }

            LookForGround();
            ApplyThrottle();
            ApplyCounterFlipForce();

            wheelsOnGround = 0;
            foreach (var wheel in wheels)
            {
                wheel.steerAngle = currentSteering * Mathf.Lerp(steerAngleMin, steerAngleMax, forceSlip ? 0f : Mathf.Abs(signedForwardSpeed / maxForwardSpeed));
                wheel.forceSlip = forceSlip;
                wheel.Simulate();
                if (wheel.onGround) wheelsOnGround++;
            }
            
            currentSpeed = Mathf.Round(signedForwardSpeed * 3.6f);
        }

        private void ApplyCounterFlipForce()
        {
            if (wheelsOnGround < 3)
            {
                currentCounterFlipTorque += counterFlipTorqueIncrease * Time.deltaTime;
                var torque = Vector3.Cross(transform.up, lastGroundDirection) * currentCounterFlipTorque;
                body.AddTorque(torque, ForceMode.Acceleration);
            }
            else
            {
                currentCounterFlipTorque = counterFlipTorqueStart;
            }
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
            lastGroundDirection = average.normalized;
        }

        private void ApplyThrottle()
        {
            if (wheelsOnGround < 2) return;
            
            var maxSpeed = signedForwardSpeed > 0f ? maxForwardSpeed : maxReverseSpeed;
            var newVelocity = Mathf.MoveTowards(signedForwardSpeed, currentThrottle * maxSpeed, maxForwardSpeed * Mathf.Min(Time.fixedDeltaTime / accelerationTime, 1f));
            body.linearVelocity += transform.forward * (newVelocity - signedForwardSpeed) * Mathf.Abs(currentThrottle);
        }

        private ReplicateData CreateReplicationData()
        {
            if (!IsOwner) return default;
            
            return new ReplicateData
            {
                targetThrottle = targetThrottle,
                targetSteering = targetSteering,
                braking = braking,
            };
        }

        private void OnPostTick()
        {
            CreateReconcile();
        }

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
            if (data.GetTick() != TimeManager.Tick) Debug.LogWarning($"Received Funny Reconciliation Data, Data Recieved has tick of {data.GetTick()}, TimeManager is at tick {TimeManager.Tick}");
            
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
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            maxForwardSpeedKmpH = Mathf.Max(0f, maxForwardSpeedKmpH);
            steerAngleMax = Mathf.Max(0f, steerAngleMax);
            steerAngleMin = Mathf.Max(0f, steerAngleMin);
            accelerationTime = Mathf.Max(0f, accelerationTime);
        }

        public struct ReplicateData : IReplicateData
        {
            public float targetThrottle;
            public float targetSteering;
            public float braking;
            
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