using System.Net;
using C2DMNet.Contracts.Enums;

namespace C2DMNet.Contracts.DataContracts
{
    public class SendMessageDataContract
    {
        public HttpStatusCode ResponseCode { get; set; }
        public C2DMResponseCode C2DMResponseCode { get; set; }
        public string Error { get; set; }
        public string UpdateClient { get; set; }
    }
}