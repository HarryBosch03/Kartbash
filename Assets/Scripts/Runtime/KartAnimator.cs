using UnityEngine;

namespace Runtime
{
    [DefaultExecutionOrder(50)]
    public class KartAnimator : MonoBehaviour
    {
        public Transform visuals;
        public Transform body;
        public float leanSpring;
        public float leanDamping;
        public float maxDistance;
        public float sensitivity;
        public float framerate;
        public float displaySize = 1f;
        
        private KartController kart;
        private float framerateTimer;

        private Vector3 leanPosition;
        private Vector3 leanVector;
        private Vector3 leanVelocity;
        private Quaternion bodyRotation;

        private void Awake() { kart = GetComponentInParent<KartController>(); }

        private void Update()
        {
            UpdateLeanPosition(Time.deltaTime);
            CalculateBodyRotation();
            leanVector = leanPosition - visuals.position;
            
            if (framerateTimer > 1f / framerate)
            {
                body.localRotation = bodyRotation;
                framerateTimer -= 1f / framerate;
            }

            framerateTimer += Time.deltaTime;
        }

        private void CalculateBodyRotation()
        {
            var leanVector = leanPosition - visuals.position;
            var ax = Mathf.Atan(Vector3.Dot(leanVector, visuals.forward)) * Mathf.Rad2Deg * sensitivity;
            var ay = Mathf.Atan(Vector3.Dot(leanVector, visuals.right)) * Mathf.Rad2Deg * sensitivity;

            bodyRotation = Quaternion.Euler(ax, 0f, -ay);
        }

        private void UpdateLeanPosition(float dt)
        {
            var force = (visuals.position - leanPosition) * leanSpring - leanVelocity * leanDamping;

            leanPosition += leanVelocity * dt;
            leanVelocity += force * dt;

            if ((leanPosition - visuals.position).magnitude > maxDistance)
            {
                var vector = (leanPosition - visuals.position).normalized;
                leanPosition = vector * maxDistance + visuals.position;
                leanVelocity -= vector * Mathf.Max(Vector3.Dot(vector, leanVelocity), 0f);
            }

            leanPosition += Vector3.Project(visuals.position - leanPosition, visuals.up);
            leanVelocity += Vector3.Project(-leanVelocity, visuals.up);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            var leanVector = Application.isPlaying ? this.leanVector : Vector3.zero;
            leanVector *= displaySize;

            Gizmos.DrawLine(Vector3.zero, leanVector);
            Gizmos.DrawSphere(leanVector, 0.1f);
            Gizmos.DrawSphere(leanPosition, 0.1f);

            var c = 16;
            for (var i = 0; i < c; i++)
            {
                var a0 = i / (float)c * 2f * Mathf.PI;
                var a1 = (i + 1f) / c * 2f * Mathf.PI;

                var p0 = new Vector3(Mathf.Cos(a0), 0f, Mathf.Sin(a0)) * maxDistance * displaySize;
                var p1 = new Vector3(Mathf.Cos(a1), 0f, Mathf.Sin(a1)) * maxDistance * displaySize;

                Gizmos.DrawLine(p0, p1);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, visuals.forward);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Vector3.zero, -visuals.forward);
        }

        private void OnValidate() { maxDistance = Mathf.Max(0f, maxDistance); }
    }
}