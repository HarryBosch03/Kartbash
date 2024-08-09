using Unity.Cinemachine;
using UnityEngine;

namespace Runtime.Cameras
{
    [DefaultExecutionOrder(50)]
    public class KartCamera : MonoBehaviour
    {
        public KartController kart;
        public CinemachineMixingCamera mixingCam;
        public float blendTime;

        private float blendPercent;
        
        private void Awake()
        {
            transform.SetParent(null);
            name = $"[{kart.name}] {name}";
        }

        private void FixedUpdate()
        {
            blendPercent = Mathf.MoveTowards(blendPercent, kart.onGround ? 0f : 1f,  Time.deltaTime / Mathf.Max(blendTime, Time.deltaTime));
            var t = Smootherstep(blendPercent);
            mixingCam.SetWeight(0, 1f - t);
            mixingCam.SetWeight(1, t);
        }
        
        private static float Smootherstep(float x) => x * x * x * (x * (6.0f * x - 15.0f) + 10.0f);
    }
}