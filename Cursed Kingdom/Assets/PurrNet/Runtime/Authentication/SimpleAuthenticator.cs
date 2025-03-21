using System.Threading.Tasks;
using UnityEngine;

namespace PurrNet.Authentication
{
    [RegisterNetworkType(typeof(AuthenticationRequest<string>))]
    public class SimpleAuthenticator : AuthenticationBehaviour<string>
    {
        [Tooltip("The password required to authenticate the client.")] [SerializeField]
        private string _password = "PurrNet";

        protected override Task<AuthenticationRequest<string>> GetClientPlayload()
        {
            return Task.FromResult(new AuthenticationRequest<string>(_password));
        }

        protected override Task<AuthenticationResponse> ValidateClientPayload(string payload)
        {
            return Task.FromResult<AuthenticationResponse>(_password == payload);
        }
    }
}