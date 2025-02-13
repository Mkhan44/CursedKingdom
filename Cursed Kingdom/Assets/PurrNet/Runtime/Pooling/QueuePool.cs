using System.Collections.Generic;

namespace PurrNet.Pooling
{
    public class QueuePool<T> : GenericPool<Queue<T>>
    {
        private static readonly QueuePool<T> _instance;

        static QueuePool() => _instance = new QueuePool<T>();

        static Queue<T> Factory() => new();

        static void Reset(Queue<T> list) => list.Clear();

        public QueuePool() : base(Factory, Reset) { }
        
        public static int GetCount() => _instance.count;

        public static Queue<T> Instantiate() => _instance.Allocate();

        public static void Destroy(Queue<T> list) => _instance.Delete(list);
    }
}