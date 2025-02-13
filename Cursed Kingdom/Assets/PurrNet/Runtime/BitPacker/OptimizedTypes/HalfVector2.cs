using UnityEngine;

namespace PurrNet.Packing
{
    public struct HalfVector2
    {
        public Half x;
        public Half y;
        
        public static implicit operator Vector2(HalfVector2 value)
        {
            return new Vector2(value.x, value.y);
        }
        
        public static implicit operator HalfVector2(Vector2 value)
        {
            return new HalfVector2
            {
                x = new Half(value.x),
                y = new Half(value.y)
            };
        }
    }
}