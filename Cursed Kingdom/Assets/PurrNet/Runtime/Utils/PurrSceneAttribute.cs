using System;
using UnityEngine;

namespace PurrNet
{
    /// <summary>
    /// Attribute that restricts a field to only accept Scene assets
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PurrSceneAttribute : PropertyAttribute
    {
        public PurrSceneAttribute()
        {
        }
    }
}