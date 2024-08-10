using UnityEngine;

namespace Runtime
{
    [RequireComponent(typeof(WheelCollider))]
    public class WheelAnimator : MonoBehaviour
    {
        public Transform visuals;
        
        private WheelCollider vehicleWheel;
        private float rotation;

        private void Awake()
        {
            vehicleWheel = GetComponent<WheelCollider>();
        }

        private void Update()
        {
            rotation += vehicleWheel.rpm * 6f * Time.deltaTime;
            rotation %= 360f;
            
            visuals.localRotation = Quaternion.Euler(rotation, vehicleWheel.canSteer ? vehicleWheel.steerAngle : 0f, 0f);
        }
    }
}