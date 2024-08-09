using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime
{
    public class PlayerKartController : MonoBehaviour
    {
        public KartController kart;

        private void Update()
        {
            if (kart != null)
            {
                var kb = Keyboard.current;

                kart.targetThrottle = kb.wKey.ReadValue() - kb.sKey.ReadValue();
                kart.braking = kb.spaceKey.ReadValue();
                kart.targetSteering = kb.dKey.ReadValue() - kb.aKey.ReadValue();
            }
        }
    }
}
