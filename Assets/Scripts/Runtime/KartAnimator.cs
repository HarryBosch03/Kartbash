using UnityEngine;

namespace Runtime
{
    [DefaultExecutionOrder(50)]
    public class KartAnimator : MonoBehaviour
    {
        public Transform body;
        public Vector2 bodyLeanResponse;
        public Vector2 bodyLeanMax;
        public float bodyLeanSmoothing;
        public float framerate;
        
        private KartController kart;
        private float framerateTimer;

        private Vector3 lastLinearVelocity;

        private Quaternion smoothedBodyLean;
        
        private void Awake()
        {
            kart = GetComponentInParent<KartController>();
        }

        private void Update()
        {
            if (framerateTimer > 1f / framerate)
            {
                var deltaTime = 1f / framerate;
                var forwardSpeed = Vector3.Dot(kart.transform.forward, (kart.body.linearVelocity - lastLinearVelocity) / deltaTime);
                var angularSpeed = Vector3.Dot(kart.transform.up, kart.body.angularVelocity);

                var bodyLean = Quaternion.Euler(-Mathf.Clamp(forwardSpeed * bodyLeanResponse.x, -bodyLeanMax.x, bodyLeanMax.x), 0f,  Mathf.Clamp(angularSpeed * bodyLeanResponse.y, -bodyLeanMax.y, bodyLeanMax.y));
                smoothedBodyLean = Quaternion.Slerp(smoothedBodyLean, bodyLean, deltaTime / Mathf.Max(deltaTime, bodyLeanSmoothing));
                body.localRotation = smoothedBodyLean;
            
                lastLinearVelocity = kart.body.linearVelocity;
                
                framerateTimer -= 1f / framerate;
            }
            framerateTimer += Time.deltaTime;
        }
    }
}