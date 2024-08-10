using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Tools
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class Freecam : MonoBehaviour
    {
        public float startMoveSpeed;
        public float moveSpeedIncrease;
        public float accelerationTime;
        public float mouseSensitivity = 0.3f;
        
        private CinemachineCamera cam;

        private Vector3 velocity;
        private Vector2 rotation;
        private float moveSpeed;

        private Eddy.Message speedMessage;
        
        private void Awake()
        {
            cam = GetComponent<CinemachineCamera>();
        }

        private void OnEnable()
        {
            moveSpeed = startMoveSpeed;
            speedMessage = new Eddy.Message();

            cam.enabled = false;
        }

        private void Update()
        {
            if (Keyboard.current.f1Key.wasPressedThisFrame)
            {
                if (cam.enabled)
                {
                    cam.enabled = false;
                    Cursor.lockState = CursorLockMode.None;
                    Eddy.ShowMessage("Disabled Free Cam");
                }
                else
                {
                    cam.enabled = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    var brain = CinemachineBrain.GetActiveBrain(0);
                    cam.transform.position = brain.transform.position;
                    cam.transform.rotation = brain.transform.rotation;
                    Eddy.ShowMessage("Enabled Free Cam");
                }
            }

            if (cam.enabled)
            {
                var kb = Keyboard.current;
                var m = Mouse.current;
                var input = new Vector3
                {
                    x = kb.dKey.ReadValue() - kb.aKey.ReadValue(),
                    y = kb.eKey.ReadValue() - kb.qKey.ReadValue(),
                    z = kb.wKey.ReadValue() - kb.sKey.ReadValue(),
                };

                if (input.magnitude > 0.1f) moveSpeed += moveSpeedIncrease * Time.deltaTime;
                else moveSpeed = startMoveSpeed;
                
                var target = transform.TransformDirection(input) * moveSpeed;
                velocity = Vector3.MoveTowards(velocity, target , startMoveSpeed * Time.deltaTime / accelerationTime);

                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    rotation += m.delta.ReadValue() * mouseSensitivity;
                    rotation.x %= 360f;
                    rotation.y = Mathf.Clamp(rotation.y, -90f, 90f);
                }

                transform.rotation = Quaternion.Euler(-rotation.y, rotation.x, 0f);
            }
        }
    }
}