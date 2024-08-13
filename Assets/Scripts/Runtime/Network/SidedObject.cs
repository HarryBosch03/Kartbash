using System;
using FishNet.Object;

namespace Runtime.Network
{
    public class SidedObject : NetworkBehaviour
    {
        public Side visibility;

        public override void OnStartNetwork()
        {
            var isOwner = Owner.IsLocalClient;
            
            switch (visibility)
            {
                case Side.OwnerOnly:
                    gameObject.SetActive(isOwner);
                    break;
                case Side.ExceptOwner:
                    gameObject.SetActive(!isOwner);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum Side
        {
            OwnerOnly,
            ExceptOwner,
        }
    }
}