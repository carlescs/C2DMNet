using System.Collections.Generic;
using System.Threading.Tasks;
using C2DMNet.Contracts.DataContracts;

namespace C2DMNet.Contracts
{
    public interface IC2DMConnectionServiceAsync
    {
        Task<string> GetTokenAsync(string email, string password, string source);
        Task<SendMessageDataContract> SendMessageAsync(string authToken, string registrationId, IDictionary<string, string> content);
    }
}