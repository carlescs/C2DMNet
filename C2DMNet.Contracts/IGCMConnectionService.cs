using System.Collections.Generic;
using C2DMNet.Contracts.DataContracts;

namespace C2DMNet.Contracts
{
    public interface IGCMConnectionService
    {
        GCMResult SendMessage(string authKey, string[] registrationIds, IDictionary<string, string> content);
    }
}
