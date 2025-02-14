using System.Collections.Generic;

namespace PurrNet.Pooling
{
    public class ListPool<T> : GenericPool<List<T>>
    {
        private static readonly ListPool<T> _instance;

        static ListPool() => _instance = new ListPool<T>();

        static List<T> Factory() => new List<T>();

        static void Reset(List<T> list) => list.Clear();

        public ListPool() : base(Factory, Reset) { }
        
        public static int GetCount() => _instance.count;

        public static List<T> Instantiate() => _instance.Allocate();

        public static void Destroy(List<T> list) => _instance.Delete(list);
    }
}
