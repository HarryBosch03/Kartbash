using UnityEngine;

namespace Runtime
{
    public class WheelController : MonoBehaviour
    {
        public bool canSteer;

        [Space]
        public float radius;
        public float suspensionSpring;
        public float suspensionDamping;

        [Space]
        public bool onGround;

        private Rigidbody body;
        public RaycastHit groundHit { get; private set; }
        public float rpm { get; private set; }
        public bool forceSlip { get; set; }
        
        public float steerAngle { get; set; }

        private void Awake()
        {
            body = GetComponentInParent<Rigidbody>(); 
            transform.SetParent(body.transform);
        }

        public void Simulate()
        {
            var ray = new Ray(transform.position, -transform.up);
            onGround = Physics.Raycast(ray, out var hit, radius);
            if (onGround)
            {
                groundHit = hit;
                
                var position = ray.GetPoint(radius);
                var velocity = body.GetPointVelocity(position);

                var circumference = radius * Mathf.PI * 2f;
                rpm = Vector3.Dot(velocity, transform.forward) / circumference * 60f;
                
                ApplySteering();
                ApplySuspensionForce(position, velocity);
            }
        }

        private void ApplySteering() { transform.localRotation = Quaternion.Euler(0f, canSteer ? steerAngle : 0f, 0f); }

        private void ApplySuspensionForce(Vector3 position, Vector3 velocity)
        {
            var force = (groundHit.point - position) * suspensionSpring + ((groundHit.rigidbody != null ? groundHit.rigidbody.GetPointVelocity(groundHit.point) : Vector3.zero) - velocity) * suspensionDamping;
            Accelerate(Vector3.Project(force, groundHit.normal), groundHit.point);
        }

        private void Accelerate(Vector3 acceleration, Vector3 point)
        {
            if (!onGround) return;

            body.linearVelocity += acceleration * Time.fixedDeltaTime;
            body.angularVelocity += Vector3.Cross(point - body.worldCenterOfMass, acceleration) * Time.fixedDeltaTime;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.DrawLine(Vector3.zero, Vector3.down * radius);
            var c = 32;
            for (var i = 0; i < c; i++)
            {
                var a0 = i / (c * 0.5f) * Mathf.PI;
                var a1 = (i + 1) / (c * 0.5f) * Mathf.PI;
                Gizmos.DrawLine(new Vector3(0f, Mathf.Cos(a0), Mathf.Sin(a0)) * radius, new Vector3(0f, Mathf.Cos(a1), Mathf.Sin(a1)) * radius);
            }
        }
    }
}