using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PurrNet.Logging;
using PurrNet.Modules;
using PurrNet.Packing;
using PurrNet.Transports;
using UnityEngine;

namespace PurrNet.Authentication
{
    public struct AuthenticationRequest : IPackedAuto
    {
        [CanBeNull] public string cookie;
    }

    public struct AuthenticationRequest<T> : IPackedAuto
    {
        /// <summary>
        /// This will be used to retrieve the same PlayerID from past sessions if present.
        /// </summary>
        [CanBeNull] public string cookie;

        /// <summary>
        /// The payload to be validated.
        /// </summary>
        public T payload;

        public AuthenticationRequest(T payload)
        {
            this.payload = payload;
            cookie = null;
        }
    }

    public struct AuthenticationResponse : IPackedAuto
    {
        /// <summary>
        /// Whether the authentication was successful.
        /// If true, the client will be authenticated.
        /// Else, the client will be disconnected.
        /// </summary>
        public bool success;

        /// <summary>
        /// Optional cookie, this overrides the cookie in the AuthenticationRequest.
        /// This will be used to retrieve the same PlayerID from past sessions if present.
        /// </summary>
        [CanBeNull] public string cookie;

        public static implicit operator bool(AuthenticationResponse response) => response.success;

        public static implicit operator AuthenticationResponse(bool success) =>
            new AuthenticationResponse { success = success };
    }

    public abstract class AuthenticationLayer : MonoBehaviour
    {
        [Tooltip("The time in seconds before the authentication times out and the client is disconnected.")]
        [SerializeField]
        private float _timeout = 5f;

        public float timeout
        {
            get => _timeout;
            set => _timeout = value;
        }

        public event Action<Connection, AuthenticationResponse> onAuthenticationComplete;

        public abstract void Subscribe(BroadcastModule broadcastModule);

        public abstract void Unsubscribe(BroadcastModule broadcastModule);

        public abstract void SendClientPayload(BroadcastModule broadcastModule, CookiesModule cookies);

        protected void TrigerAuthenticationComplete(Connection conn, AuthenticationResponse response)
        {
            try
            {
                onAuthenticationComplete?.Invoke(conn, response);
            }
            catch (Exception e)
            {
                PurrLogger.LogError($"Failed to complete authentation for {conn}: {e.Message}");
            }
        }
    }

    public abstract class AuthenticationBehaviour<T> : AuthenticationLayer
    {
        public override void Subscribe(BroadcastModule broadcastModule)
        {
            broadcastModule.Subscribe<AuthenticationRequest<T>>(OnPayload);
        }

        public override void Unsubscribe(BroadcastModule broadcastModule)
        {
            broadcastModule.Unsubscribe<AuthenticationRequest<T>>(OnPayload);
        }

        public override async void SendClientPayload(BroadcastModule broadcastModule, CookiesModule cookies)
        {
            try
            {
                var payload = await GetClientPlayload();
                payload.cookie ??= cookies.GetOrSet("client_connection_session", Guid.NewGuid().ToString());
                broadcastModule.SendToServer(payload);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async void OnPayload(Connection conn, AuthenticationRequest<T> data, bool asServer)
        {
            try
            {
                var result = await ValidateClientPayload(data.payload);
                if (result.cookie == null && data.cookie != null)
                    result.cookie = data.cookie;
                TrigerAuthenticationComplete(conn, result);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                TrigerAuthenticationComplete(conn, false);
            }
        }

        /// <summary>
        /// Get the client payload to be validated.
        /// This gets called when a new client connects and is then sent to the server for validation.
        /// </summary>
        /// <returns>The client payload to be validated.</returns>
        protected abstract Task<AuthenticationRequest<T>> GetClientPlayload();

        /// <summary>
        /// Once the client payload is received, this method is called to validate the payload.
        /// This only runs on the server.
        /// </summary>
        /// <param name="payload">The client payload to be validated.</param>
        /// <returns>The result of the validation.</returns>
        protected abstract Task<AuthenticationResponse> ValidateClientPayload(T payload);
    }
}