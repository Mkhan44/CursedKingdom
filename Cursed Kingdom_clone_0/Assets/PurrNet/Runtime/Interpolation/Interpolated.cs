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
        private float _timer;
        private float _tickDelta;
        
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
        
        public Interpolated(LerpFunction<T> lerp, float tickDelta, T initialValue = default, int maxBufferSize = 2)
        {
            _lerp = lerp ?? throw new ArgumentNullException(nameof(lerp));
            
            if (tickDelta <= 0f)
                throw new ArgumentException("tickDelta must be greater than 0", nameof(tickDelta));
            if (maxBufferSize <= 0)
                throw new ArgumentException("maxBufferSize must be greater than 0", nameof(maxBufferSize));
                
            _tickDelta = tickDelta;
            this.maxBufferSize = maxBufferSize;
            _lastValue = initialValue;
            _buffer = new List<T>(maxBufferSize);
        }
        
        public void Add(T value)
        {
            if (_buffer.Count >= maxBufferSize)
            {
                _buffer.RemoveAt(0);
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
            if (_buffer.Count <= 0)
            {
                _timer = 0f;
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
                    return _lastValue;
                }
            }

            return _lerp(_lastValue, _buffer[0], _timer / _tickDelta);
        }
    }
}