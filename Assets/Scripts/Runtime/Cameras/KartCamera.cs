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
        public float cameraShakeFrequency = 10f;
        public float cameraShakeFromSpeed = 2.5f;
        public float idleFov = 90f;
        public float maxSpeedFov = 110f;

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

            var rd = Mathf.PerlinNoise1D(Time.time * cameraShakeFrequency);
            var ra = Mathf.PerlinNoise1D(Time.time * cameraShakeFrequency + 8294.0f) * 2f * Mathf.PI;
            var noiseOffset = new Vector2(Mathf.Cos(ra), Mathf.Sin(ra)) * rd;

            var noiseScale = 0f;
            noiseScale += kart.body.linearVelocity.magnitude * cameraShakeFromSpeed / (10f * kart.maxForwardSpeed);

            transform.position += (transform.right * noiseOffset.x + transform.up * noiseOffset.y) * noiseScale;

            cam.Lens.FieldOfView = Mathf.Lerp(idleFov, maxSpeedFov, Mathf.Abs(kart.signedForwardSpeed / kart.maxForwardSpeed));
        }
    }
}