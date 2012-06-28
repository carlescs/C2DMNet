using System.Net;

namespace C2DMNet.Contracts.DataContracts
{
    public class GCMResult
    {
        public HttpStatusCode ResponseCode { get; set; }

        public GCMResultJson ResultJson { get; set; }
    }
}