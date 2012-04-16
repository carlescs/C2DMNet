using System.Collections.Generic;
using System.Net;

namespace C2DMNet.Contracts
{
    public interface IC2DMConnectionService
    {
        string GetToken(string email, string password, string source);
        HttpStatusCode SendMessage(string authToken, string registrationId, IDictionary<string,string> content, out string error);
    }
}