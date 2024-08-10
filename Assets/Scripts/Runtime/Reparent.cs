using UnityEngine;

namespace Runtime
{
    public class Reparent : MonoBehaviour
    {
        public Transform parent;

        private void Awake()
        {
            transform.SetParent(parent);
        }
    }
}