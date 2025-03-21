using System.Collections.Generic;

namespace PurrNet.Pooling
{
    public class CollectionPool<T, I> : GenericPool<T> where T : ICollection<I>, new()
    {
        private static readonly CollectionPool<T, I> _instance;

        static T Factory()
        {
            return new T();
        }

        static void Reset(T collection)
        {
            collection.Clear();
        }

        public CollectionPool() : base(Factory, Reset)
        {
        }

        public static T New()
        {
            return _instance.Allocate();
        }

        public static void Destroy(T list)
        {
            _instance.Delete(list);
        }
    }
}