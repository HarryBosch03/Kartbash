using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Karts
{
    public class KartInput : NetworkBehaviour
    {
        public KartController kart;

        private void Update()
        {
            if (kart != null && IsOwner)
            {
                var kb = Keyboard.current;

                kart.targetThrottle = kb.wKey.ReadValue() - kb.sKey.ReadValue();
                kart.braking = kb.leftShiftKey.ReadValue();
                kart.targetSteering = kb.dKey.ReadValue() - kb.aKey.ReadValue();

                kart.airControl = new Vector3
                {
                    x = kb.wKey.ReadValue() - kb.sKey.ReadValue(),
                    y = kb.eKey.ReadValue() - kb.qKey.ReadValue(),
                    z = kb.aKey.ReadValue() - kb.dKey.ReadValue(),
                };

                if (kb.spaceKey.wasPressedThisFrame) kart.jump = true;
            }
        }
    }
}
