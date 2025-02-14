using System.Collections.Generic;

namespace PurrNet.Pooling
{
    public class HashSetPool<T> : GenericPool<HashSet<T>>
    {
        private static readonly HashSetPool<T> _instance;

        static HashSetPool() => _instance = new HashSetPool<T>();

        static HashSet<T> Factory() => new HashSet<T>();

        static void Reset(HashSet<T> list) => list.Clear();

        public HashSetPool() : base(Factory, Reset) { }
        
        public static int GetCount() => _instance.count;

        public static HashSet<T> Instantiate() => _instance.Allocate();

        public static void Destroy(HashSet<T> list) => _instance.Delete(list);
    }
}