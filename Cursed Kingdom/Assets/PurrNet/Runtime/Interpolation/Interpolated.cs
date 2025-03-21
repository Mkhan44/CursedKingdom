using System;
using System.Collections.Generic;

namespace PurrNet
{
    public delegate T LerpFunction<T>(T from, T to, float t);

    public class Interpolated<T>
    {
        private readonly LerpFunction<T> _lerp;
        private readonly List<T> _buffer;
        private T _lastValue;
        private T _currentState;
        private float _timer;
        private float _tickDelta;
        protected bool _waitForMinBufferSize;

        public int bufferSize => _buffer.Count;

        public float tickDelta
        {
            get => _tickDelta;
            set
            {
                if (value <= 0f)
                    throw new ArgumentException("TickDelta must be greater than 0", nameof(value));
                _tickDelta = value;
            }
        }

        public int maxBufferSize { get; set; }

        public int minBufferSize { get; set; }

        public Interpolated(LerpFunction<T> lerp, float tickDelta, T initialValue = default, int maxBufferSize = 2, int minBufferSize = 1)
        {
            _lerp = lerp ?? throw new ArgumentNullException(nameof(lerp));

            if (tickDelta <= 0f)
                throw new ArgumentException("tickDelta must be greater than 0", nameof(tickDelta));

            _buffer = new List<T>(maxBufferSize);

            this.maxBufferSize = maxBufferSize;
            this.minBufferSize = minBufferSize;

            _tickDelta = tickDelta;
            _lastValue = initialValue;
            _currentState = initialValue;
            _waitForMinBufferSize = true;
        }

        public void Add(T value)
        {
            if (_buffer.Count >= maxBufferSize)
            {
                // remove up to minBufferSize
                var removeCount = _buffer.Count - minBufferSize;
                _buffer.RemoveRange(0, removeCount);
                _lastValue = _currentState;
                _timer = 0f;
            }
            _buffer.Add(value);
        }

        public void Teleport(T value)
        {
            _lastValue = value;
            _buffer.Clear();
            _timer = 0f;
        }

        public T Advance(float deltaTime)
        {
            if (_waitForMinBufferSize)
            {
                if (_buffer.Count < minBufferSize)
                {
                    _timer = 0f;
                    return _lastValue;
                }

                _waitForMinBufferSize = false;
            }

            if (_buffer.Count <= 0)
            {
                _timer = 0f;
                _waitForMinBufferSize = true;
                return _lastValue;
            }

            _timer += deltaTime;

            while (_timer >= _tickDelta)
            {
                var lerped = _lerp(_lastValue, _buffer[0], 1f);
                _buffer.RemoveAt(0);
                _lastValue = lerped;
                _timer -= _tickDelta;

                if (_buffer.Count <= 0)
                {
                    _timer = 0f;
                    _waitForMinBufferSize = true;
                    _currentState = _lastValue;
                    return _lastValue;
                }
            }

            _currentState = _lerp(_lastValue, _buffer[0], _timer / _tickDelta);
            return _currentState;
        }
    }
}
