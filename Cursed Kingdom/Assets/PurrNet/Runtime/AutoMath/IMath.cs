namespace PurrNet
{
    public interface IMath<T>
    {
        public T Add(T a, T b) => throw new System.NotImplementedException();

        public T Negate(T a) => throw new System.NotImplementedException();

        public T Scale(T a, float b) => throw new System.NotImplementedException();
    }
}