using System;

namespace PurrNet
{
    /// <summary>
    /// Marks a method to be executed locally only.
    /// This skips the RPC call and executes the method directly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class LocalModeAttribute : Attribute { }
}
