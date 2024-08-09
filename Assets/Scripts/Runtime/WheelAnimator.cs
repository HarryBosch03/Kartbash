using UnityEngine;

namespace Runtime
{
    [RequireComponent(typeof(WheelCollider))]
    public class WheelAnimator : MonoBehaviour
    {
        public Transform visuals;
        
        private WheelCollider wheel;
        private float rotation;

        private void Awake()
        {
            wheel = GetComponent<WheelCollider>();
        }

        private void Update()
        {
            rotation += wheel.rpm * 6f * Time.deltaTime;
            rotation %= 360f;
            
            visuals.localRotation = Quaternion.Euler(rotation, wheel.canSteer ? wheel.steerAngle : 0f, 0f);
        }
    }
}