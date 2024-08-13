using System;
using FishNet;
using Runtime.Karts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Testing
{
    public class SlipTest : MonoBehaviour
    {
        public float height = 1f;
        public float force = 10f;
        public AnimationCurve tangentialFrictionCurve;
        [Range(0, 100)]
        public int slip;
        public KartController trackedKart;

        private bool reset;
        
        private void Start()
        {
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection("127.0.0.1");
        }

        private void FixedUpdate()
        {
            if (reset)
            {
                reset = false;
                
                trackedKart = FindFirstObjectByType<KartController>();
                var input = trackedKart.GetComponent<KartInput>();

                trackedKart.tangentFriction = tangentialFrictionCurve;
                
                input.enabled = false;

                trackedKart.body.position = Vector3.up * height;
                trackedKart.body.rotation = Quaternion.identity;

                trackedKart.body.linearVelocity = Vector3.right * force;
                trackedKart.body.angularVelocity = Vector3.zero;
            }
            
            if (trackedKart)
            {
                slip = Mathf.RoundToInt(trackedKart.slip * 100);
            }
        }

        private void Update()
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                reset = true;
            }
            if (trackedKart != null && Keyboard.current.yKey.wasPressedThisFrame)
            {
                trackedKart.GetComponent<KartInput>().enabled = true;
            }
        }
    }
}