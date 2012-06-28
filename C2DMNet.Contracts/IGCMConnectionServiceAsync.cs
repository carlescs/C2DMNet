using System.Collections.Generic;
using System.Threading.Tasks;
using C2DMNet.Contracts.DataContracts;

namespace C2DMNet.Contracts
{
    public interface IGCMConnectionServiceAsync
    {
        Task<GCMResult> SendMessageAsync(string authKey, string[] registrationIds, IDictionary<string, string> content);
    }
}