using Runtime.Karts;
using TMPro;
using UnityEngine;

namespace Runtime.UI
{
    public class Speedometer : MonoBehaviour
    {
        public TMP_Text text;
        public Transform rotor;
        public float minRotation;
        public float maxRotation;
        public float displaySmoothing;
        public float speed;

        private KartController kart;

        private void Awake()
        {
            kart = GetComponentInParent<KartController>();
        }

        private void Update()
        {
            speed = Mathf.Lerp(speed, Mathf.Abs(kart.signedForwardSpeed), Time.deltaTime / Mathf.Max(Time.deltaTime, displaySmoothing));
            
            if (text != null)
            {
                text.text = $"{(speed * 3.6f):0}km/h";
            }

            if (rotor != null)
            {
                var angle = Mathf.Lerp(minRotation, maxRotation, speed / kart.maxForwardSpeed);
                rotor.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }
}