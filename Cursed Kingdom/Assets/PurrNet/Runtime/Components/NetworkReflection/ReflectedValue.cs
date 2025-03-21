using System;
using PurrNet.Logging;
using UnityEngine;

namespace PurrNet
{
    internal class ReflectedValue
    {
        readonly Action<object> _setter;
        readonly Func<object> _getter;
        public readonly Type valueType;
        public readonly string name;

        public object lastValue { get; private set; }

        public ReflectedValue(UnityEngine.Object target, ReflectionData data)
        {
            var type = target.GetType();

            name = data.name;
            _setter = data.GetSetter(target, type, out valueType);
            _getter = data.GetGetter(target, type);
            lastValue = _getter?.Invoke();
        }

        bool IsDifferent(object value)
        {
            return (value == null && lastValue != null) || (value != null && !value.Equals(lastValue));
        }

        public bool Update()
        {
            var value = _getter?.Invoke();

            if (IsDifferent(value))
            {
                lastValue = value;
                return true;
            }

            return false;
        }

        public void SetValue(object reflectedData)
        {
            if (_setter == null)
            {
                PurrLogger.LogError($"Setter for {name} is null, aborting");
                return;
            }

            if (IsDifferent(reflectedData))
            {
                _setter.Invoke(reflectedData);
                lastValue = reflectedData;
            }
        }
    }
}