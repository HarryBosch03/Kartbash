using System;
using UnityEngine;

namespace Runtime.Cameras
{
    [DefaultExecutionOrder(500)]
    public class KartCamera : MonoBehaviour
    {
        public KartController kart;
        public Transform visuals;
        public Vector3 offset;
        public float damping = 1f;

        private Vector3 dampedPosition;
        private Camera mainCamera;
        
        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            dampedPosition = Vector3.Lerp(dampedPosition, visuals.TransformPoint(offset), Time.deltaTime / Mathf.Max(Time.deltaTime, damping));
            transform.position = dampedPosition;
            transform.rotation = visuals.rotation;
        }

        private void LateUpdate()
        {
            if (kart.activeViewer)
            {
                mainCamera.transform.position = transform.position;
                mainCamera.transform.rotation = transform.rotation;
            }
        }

        private static float Smootherstep(float x) => x * x * x * (x * (6.0f * x - 15.0f) + 10.0f);
    }
}