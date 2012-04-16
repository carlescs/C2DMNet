using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using C2DMNet.Contracts;
using System.Linq;
using C2DMNet.Contracts.DataContracts;

namespace C2DMNet.Http
{
    public class C2DMConnectionService : IC2DMConnectionService
    {
        public string GetToken(string email, string password, string source)
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
                var req = client.PostAsync("https://www.google.com/accounts/ClientLogin", content)
                    .ContinueWith(request =>
                                      {
                                          var stringResp = request.Result.Content.ReadAsStringAsync().ContinueWith(t => t.Result.Split('\n').First(s => s.StartsWith("Auth=")).Substring(5));
                                          return stringResp.Result;
                                      });
                return req.Result;
            }
        }

        public SendMessageDataContract SendMessage(string authToken, string registrationId, IDictionary<string, string> content)
        {
            using (var client=new HttpClient())
            {
                var postContent = new FormUrlEncodedContent(content.Concat(new Dictionary<string, string>
                                                                               {
                                                                                   {"registration_id", registrationId},
                                                                                   {"collapse_key", "0"}
                                                                               }));
                var request = new HttpRequestMessage(HttpMethod.Post, "https://android.clients.google.com/c2dm/send")
                                  {
                                      Content = postContent
                                  };
                ServicePointManager.ServerCertificateValidationCallback += ValidationCallback;
                request.Headers.Add(HttpRequestHeader.Authorization.ToString(), string.Format("GoogleLogin auth={0}", authToken));
                var res = client.SendAsync(request)
                    .ContinueWith(t =>
                                      {
                                          var responseCode = t.Result.StatusCode;
                                          string errorString;
                                          if (responseCode.Equals(HttpStatusCode.OK))
                                          {
                                              errorString = t.Result.Content.ReadAsStringAsync().ContinueWith(s =>
                                                                                                                  {
                                                                                                                      string error = s.Result.Split('\n').FirstOrDefault(r => r.StartsWith("Error="));
                                                                                                                      return error!=null?error.Substring(6):null;
                                                                                                                  }).Result;
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
                                                         Error=errorString,
                                                         UpdateClient = t.Result.Content.Headers.Contains("Update-Client-Auth") ? t.Result.Content.Headers.GetValues("Update-Client-Auth").First() : null
                                                     };
                                      });
                return res.Result;
            }
        }

        private bool ValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }
}
