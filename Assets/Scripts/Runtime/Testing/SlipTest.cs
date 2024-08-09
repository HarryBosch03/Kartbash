using System;
using FishNet;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Testing
{
    public class SlipTest : MonoBehaviour
    {
        public float height = 1f;
        public float force = 10f;
        public AnimationCurve tangentialFrictionCurve;
        public float averageSlip;
        public float averageSlipTime;
        public KartController trackedKart;

        private float slipAccumulator;
        private float slipTimer;
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
                var input = trackedKart.GetComponent<PlayerKartController>();

                foreach (var wheel in trackedKart.GetComponentsInChildren<WheelCollider>())
                {
                    wheel.tangentialFrictionCurve = tangentialFrictionCurve;
                }
                
                input.enabled = false;

                trackedKart.body.position = Vector3.up * height;
                trackedKart.body.rotation = Quaternion.identity;

                trackedKart.body.linearVelocity = Vector3.right * force;
                trackedKart.body.angularVelocity = Vector3.zero;
            }
            
            if (trackedKart)
            {
                var avgSlip = 0f;
                var wheels = trackedKart.GetComponentsInChildren<WheelCollider>();
                foreach (var wheel in wheels)
                {
                    avgSlip += wheel.slip / wheels.Length;
                }
                slipAccumulator += avgSlip * Time.deltaTime;
                if (slipTimer > averageSlipTime)
                {
                    averageSlip = slipAccumulator / averageSlipTime;
                    slipAccumulator = 0f;
                    slipTimer = 0f;
                }
                slipTimer += Time.deltaTime;
            }
        }

        private void Update()
        {
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                reset = true;
            }
        }
    }
}