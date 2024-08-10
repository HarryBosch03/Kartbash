using Unity.Cinemachine;
using UnityEngine;

namespace Runtime.Cameras
{
    [DefaultExecutionOrder(50)]
    [RequireComponent(typeof(CinemachineCamera))]
    public class KartCamera : MonoBehaviour
    {
        public KartController kart;
        public Transform visuals;
        public Vector3 offset;
        public float damping = 1f;

        private CinemachineCamera cam;
        private Vector3 dampedPosition;
        private Vector3 localOffset;

        private void Awake()
        {
            cam = GetComponent<CinemachineCamera>();
        }

        private void LateUpdate()
        {
            dampedPosition = Vector3.Lerp(dampedPosition, visuals.TransformPoint(offset), Time.deltaTime / Mathf.Max(Time.deltaTime, damping));
            transform.position = dampedPosition;
            transform.rotation = visuals.rotation;
            cam.enabled = kart.activeViewer;
        }
    }
}