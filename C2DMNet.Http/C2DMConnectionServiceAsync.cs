using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using C2DMNet.Contracts;
using C2DMNet.Contracts.DataContracts;

namespace C2DMNet.Http
{
    public class C2DMConnectionServiceAsync : IC2DMConnectionServiceAsync
    {
        public async Task<string> GetTokenAsync(string email, string password, string source)
        {
            using (var client = new HttpClient())
            {
                HttpContent content = new FormUrlEncodedContent(new Dictionary<string, string>
                                                                    {
                                                                        {"Email", email},
                                                                        {"Passwd", password},
                                                                        {"accountType", "GOOGLE"},
                                                                        {"source", source},
                                                                        {"service", "ac2dm"}
                                                                    });
                var req = await client.PostAsync("https://www.google.com/accounts/ClientLogin", content);
                var res = await req.Content.ReadAsStringAsync();
                return res.Split('\n').First(s => s.StartsWith("Auth=")).Substring(5);
            }
        }

        public async Task<SendMessageDataContract> SendMessageAsync(string authToken, string registrationId, IDictionary<string, string> content)
        {
            using (var client=new HttpClient())
            {
                var nameValueCollection = new Dictionary<string, string>
                                              {
                                                  {"registration_id", registrationId}, {"collapse_key", "0"}
                                              };
                foreach (var kvp in content)
                {
                    nameValueCollection.Add("data." + kvp.Key, kvp.Value);
                }
                var postContent = new FormUrlEncodedContent(nameValueCollection);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://android.clients.google.com/c2dm/send")
                                  {
                                      Content = postContent
                                  };
                ServicePointManager.ServerCertificateValidationCallback += ValidationCallback;
                request.Headers.Add(HttpRequestHeader.Authorization.ToString(), string.Format("GoogleLogin auth={0}", authToken));
                var res = await client.SendAsync(request);

                var responseCode = res.StatusCode;
                string errorString;
                if (responseCode.Equals(HttpStatusCode.OK))
                {
                    var errorLine=(await res.Content.ReadAsStringAsync()).Split('\n').FirstOrDefault(r => r.StartsWith("Error="));
                    errorString = errorLine != null ? errorLine.Substring(6) : null;
                }
                else if (responseCode.Equals(HttpStatusCode.NotImplemented))
                {
                    errorString = "Server unavailable.";
                }
                else if (responseCode.Equals(HttpStatusCode.Unauthorized))
                {
                    errorString = "Invalid AUTH_TOKEN";
                }
                else
                {
                    errorString = "Unspecified error";
                }
                return new SendMessageDataContract
                           {
                               ResponseCode = responseCode,
                               Error = errorString,
                               UpdateClient = res.Content.Headers.Contains("Update-Client-Auth") ? res.Content.Headers.GetValues("Update-Client-Auth").First() : null
                           };
            }
        }

        private bool ValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }
}