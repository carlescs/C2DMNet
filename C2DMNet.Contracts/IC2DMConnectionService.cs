using System;
using System.Collections.Generic;
using System.Net;
using C2DMNet.Contracts.DataContracts;

namespace C2DMNet.Contracts
{
    public interface IC2DMConnectionService
    {
        SendMessageDataContract SendMessage(string authToken, string registrationId, IDictionary<string,string> content);
        [Obsolete("Use other SendMessage")]
        HttpStatusCode SendMessage(string authToken, string registrationId, IDictionary<string, string> content, out string error);
    }
}