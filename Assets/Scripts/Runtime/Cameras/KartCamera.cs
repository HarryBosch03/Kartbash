using Runtime.Karts;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

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
        public float landTransitionDuration = 1f;
        public float cameraShakeFrequency = 10f;
        public float cameraShakeFromSpeed = 2.5f;
        public float idleFov = 90f;
        public float maxSpeedFov = 110f;
        public float mouseSensitivity = 0.3f;

        private CinemachineCamera cam;
        private Vector3 dampedPosition;
        private Vector3 localOffset;
        private Vector2 rotation;
        private bool freeCamEnabled;
        
        private Quaternion orientation = Quaternion.identity;
        private Quaternion lastOrientation = Quaternion.identity;
        private bool wasOnGround;
        private float orientationBlend;

        private void Awake()
        {
            cam = GetComponent<CinemachineCamera>();
        }

        private void LateUpdate()
        {
            var kb = Keyboard.current;
            var m = Mouse.current;
            if (kb.vKey.wasPressedThisFrame)
            {
                if (freeCamEnabled)
                {
                    freeCamEnabled = false;
                    rotation = new Vector2(transform.eulerAngles.y, -transform.eulerAngles.x);
                }
                else freeCamEnabled = true;
            }
            if (freeCamEnabled)
            {
                rotation += m.delta.ReadValue() * mouseSensitivity;
                rotation.x %= 360f;
                rotation.y = Mathf.Clamp(rotation.y, -45f, 80f);
            }
            
            TranslateCamera();
            OrientCamera();

            cam.enabled = kart.activeViewer;

            AddNoise();

            cam.Lens.FieldOfView = Mathf.Lerp(idleFov, maxSpeedFov, Mathf.Abs(kart.signedForwardSpeed / kart.maxForwardSpeed));
        }

        private void AddNoise()
        {
            var rd = Mathf.PerlinNoise1D(Time.time * cameraShakeFrequency);
            var ra = Mathf.PerlinNoise1D(Time.time * cameraShakeFrequency + 8294.0f) * 2f * Mathf.PI;
            var noiseOffset = new Vector2(Mathf.Cos(ra), Mathf.Sin(ra)) * rd;

            var noiseScale = 0f;
            noiseScale += kart.body.linearVelocity.magnitude * cameraShakeFromSpeed / (10f * kart.maxForwardSpeed);

            transform.position += (transform.right * noiseOffset.x + transform.up * noiseOffset.y) * noiseScale;
        }

        private void TranslateCamera()
        {
            var t = Time.deltaTime / Mathf.Max(Time.deltaTime, damping);
            dampedPosition = Vector3.Lerp(dampedPosition, kart.body.worldCenterOfMass + transform.rotation * offset, t);
            transform.position = dampedPosition;
        }

        private void OrientCamera()
        {
            if (freeCamEnabled)
            {
                transform.rotation = Quaternion.Euler(-rotation.y, rotation.x, 0f);
                return;
            }
            
            var onGround = kart.wheelsOnGround;
            if (onGround)
            {   
                if (!wasOnGround && orientationBlend > 1f)
                {
                    lastOrientation = orientation;
                    orientationBlend = 0f;
                }
                orientation = visuals.rotation;
            }
            wasOnGround = onGround;
            
            transform.rotation = Quaternion.Slerp(lastOrientation, orientation, 1f - sqr(1f - Mathf.Clamp01(orientationBlend)));
            orientationBlend += Time.deltaTime / landTransitionDuration;

            float sqr(float x) => x * x;
        }
    }
}