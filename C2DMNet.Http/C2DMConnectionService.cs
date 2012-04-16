using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using C2DMNet.Contracts;
using System.Linq;

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

        public HttpStatusCode SendMessage(string authToken, string registrationId, IDictionary<string, string> content, out string error)
        {
            using (var client=new HttpClient())
            {
                var postContent = new FormUrlEncodedContent(content.Concat(new Dictionary<string, string>
                                                                               {
                                                                                   {"registration_id", registrationId},
                                                                                   {"collapse_key", "0"}
                                                                               }));
                postContent.Headers.Add("Authorization", string.Format("GoogleLogin auth={0}", authToken));
                var res = client.PostAsync("https://android.clients.google.com/c2dm/send", postContent)
                    .ContinueWith(t =>
                                      {
                                          var responseCode=t.Result.StatusCode;
                                          string errorString;
                                          if (responseCode.Equals(200))
                                          {

                                              errorString=t.Result.Content.ReadAsStringAsync().ContinueWith(s => s.Result.Split('\n').First(r=>r.StartsWith("Error=")).Substring(6)).Result;
                                          }
                                          else if (responseCode.Equals(501))
                                          {
                                              errorString = "Server unavailable.";
                                          }
                                          else if (responseCode.Equals(401))
                                          {
                                              errorString = "Invalid AUTH_TOKEN";
                                          }
                                          else
                                          {
                                              errorString = "Unspecified error";
                                          }

                                          return new
                                                     {
                                                         responseCode,
                                                         errorString
                                                     };
                                      });
                var result = res.Result;
                error = result.errorString;
                return result.responseCode;
            }
        }
    }
}
