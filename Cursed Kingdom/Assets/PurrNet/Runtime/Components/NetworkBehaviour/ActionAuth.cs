using System;

namespace PurrNet
{
    public enum VisibilityMode
    {
        /// <summary>
        /// Fully destroy the object when it is not visible
        /// And spawn it when it is visible
        /// </summary>
        SpawnDespawn,

        /// <summary>
        /// Pool the object when it is not visible
        /// </summary>
        Pool
    }

    [Flags]
    public enum ActionAuth
    {
        /// <summary>
        /// No one can do the action
        /// </summary>
        None = 0,

        /// <summary>
        /// The server can do the action
        /// </summary>
        Server = 1,

        /// <summary>
        /// The owner of the object can do the action
        /// </summary>
        Owner = 2,

        /// <summary>
        /// Anyone can do the action
        /// </summary>
        Observer = 4
    }

    public enum ConnectionAuth
    {
        /// <summary>
        /// Only the server can do the action
        /// </summary>
        Server,

        /// <summary>
        /// Anyone can do the action
        /// </summary>
        Everyone
    }

    public enum DefaultOwner
    {
        /// <summary>
        /// No one owns the object
        /// </summary>
        None,

        /// <summary>
        /// If spawn requested by client then client owns the object
        /// </summary>
        SpawnerIfClientOnly
    }
}