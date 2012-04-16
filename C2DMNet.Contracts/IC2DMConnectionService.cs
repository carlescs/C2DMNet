using System.Collections.Generic;
using System.Net;
using C2DMNet.Contracts.DataContracts;

namespace C2DMNet.Contracts
{
    public interface IC2DMConnectionService
    {
        string GetToken(string email, string password, string source);
        SendMessageDataContract SendMessage(string authToken, string registrationId, IDictionary<string,string> content);
    }
}