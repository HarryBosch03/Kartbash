using UnityEngine;

namespace Runtime
{
    [RequireComponent(typeof(WheelCollider))]
    public class WheelAnimator : MonoBehaviour
    {
        public Transform visuals;
        public ParticleSystem slipSmoke;
        [Range(0f, 1f)]
        public float slipThreshold;

        private WheelCollider wheel;
        private float rotation;

        private void Awake() { wheel = GetComponent<WheelCollider>(); }

        private void LateUpdate()
        {
            rotation += wheel.rpm * 6f * Time.deltaTime;
            rotation %= 360f;

            visuals.localRotation = Quaternion.Euler(rotation, wheel.canSteer ? wheel.steerAngle : 0f, 0f);

            var slipping = wheel.slip > slipThreshold;
            if (slipping != slipSmoke.isPlaying)
            {
                if (slipping) slipSmoke.Play();
                else slipSmoke.Stop();
            }
        }
    }
}