using FishNet.Object;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime
{
    public class PlayerKartController : NetworkBehaviour
    {
        public KartController kart;

        private void Update()
        {
            if (kart != null && IsOwner)
            {
                var kb = Keyboard.current;

                kart.targetThrottle = kb.wKey.ReadValue() - kb.sKey.ReadValue();
                kart.braking = kb.spaceKey.ReadValue();
                kart.targetSteering = kb.dKey.ReadValue() - kb.aKey.ReadValue();
            }
        }
    }
}
