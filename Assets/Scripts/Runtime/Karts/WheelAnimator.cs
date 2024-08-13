using Runtime.Karts;
using UnityEngine;

namespace Runtime
{
    [RequireComponent(typeof(WheelController))]
    public class WheelAnimator : MonoBehaviour
    {
        public Transform visuals;
        public ParticleSystem slipSmoke;
        [Range(0f, 1f)]
        public float slipThreshold;

        private KartController kart;
        private WheelController wheel;
        private float rotation;

        private void Awake()
        {
            kart = GetComponentInParent<KartController>();
            wheel = GetComponent<WheelController>();
        }

        private void LateUpdate()
        {
            rotation += wheel.rpm * 6f * Time.deltaTime;
            rotation %= 360f;

            visuals.localRotation = Quaternion.Euler(rotation, wheel.canSteer ? wheel.steerAngle : 0f, 0f);

            var slipping = kart.slip > slipThreshold && kart.wheelsOnGround;
            if (slipping != slipSmoke.isPlaying)
            {
                if (slipping) slipSmoke.Play();
                else slipSmoke.Stop();
            }
        }
    }
}