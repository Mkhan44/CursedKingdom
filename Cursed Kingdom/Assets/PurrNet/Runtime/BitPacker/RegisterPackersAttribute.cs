using System;
using UnityEngine.Scripting;

namespace PurrNet.Packing
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RegisterPackersAttribute : PreserveAttribute
    {
    }
}
