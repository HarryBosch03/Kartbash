using UnityEngine;

namespace Runtime
{
    public static class MathHelper
    {
        public static float ClosestToZero(float a, float b)
        {
            return Mathf.Abs(a) < Mathf.Abs(b) ? a : b;
        }
    }
}