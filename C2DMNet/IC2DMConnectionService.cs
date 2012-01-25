using System.Net;

namespace C2DMNet
{
    public interface IC2DMConnectionService
    {
        string GetToken(string email, string password, string source);
        HttpStatusCode SendMessage(string authToken, string registrationId, string message, out string error);
    }
}