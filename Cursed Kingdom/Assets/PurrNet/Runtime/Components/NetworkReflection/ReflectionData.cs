using System;
using System.Reflection;

namespace PurrNet
{
    [Serializable]
    public struct ReflectionData
    {
        public ReflectionType type;
        public string name;

        public FieldInfo GetField(Type reflectionType)
        {
            var fields = reflectionType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (field.Name == name)
                    return field;
            }

            return null;
        }

        public PropertyInfo GetProperty(Type reflectionType)
        {
            var properties =
                reflectionType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                if (property.Name == name)
                    return property;
            }

            return null;
        }

        public Action<object> GetSetter(UnityEngine.Object target, Type reflectionType, out Type valueType)
        {
            switch (type)
            {
                case ReflectionType.Field:
                    var field = GetField(reflectionType);

                    if (field == null)
                    {
                        valueType = null;
                        return null;
                    }

                    valueType = field.FieldType;
                    return value => field.SetValue(target, value);
                case ReflectionType.Property:
                    var property = GetProperty(reflectionType);

                    if (property == null)
                    {
                        valueType = null;
                        return null;
                    }

                    valueType = property.PropertyType;
                    return value => property.SetValue(target, value);
                default:
                    valueType = null;
                    return null;
            }
        }

        public Func<object> GetGetter(UnityEngine.Object target, Type reflectionType)
        {
            switch (type)
            {
                case ReflectionType.Field:
                    var field = GetField(reflectionType);
                    if (field == null)
                        return null;
                    return () => field.GetValue(target);
                case ReflectionType.Property:
                    var property = GetProperty(reflectionType);
                    if (property == null)
                        return null;
                    return () => property.GetValue(target);
                default:
                    return null;
            }
        }
    }
}