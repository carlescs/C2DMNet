using System.Net;

namespace C2DMNet.Contracts.DataContracts
{
    public class SendMessageDataContract
    {
        public HttpStatusCode ResponseCode { get; set; }
        public string Error { get; set; }
        public string UpdateClient { get; set; }
    }
}