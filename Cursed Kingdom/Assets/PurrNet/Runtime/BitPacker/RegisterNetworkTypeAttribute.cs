using System;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace PurrNet
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true), Preserve]
    public class RegisterNetworkTypeAttribute : PreserveAttribute
    {
        public RegisterNetworkTypeAttribute([UsedImplicitly] Type type)
        {
        }
    }

    /*[AttributeUsage(AttributeTargets.Method), Preserve]
    public class RegisterGenericPackerAttribute : PreserveAttribute
    { }*/
}