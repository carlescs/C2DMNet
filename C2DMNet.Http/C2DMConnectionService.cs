﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using C2DMNet.Contracts;
using System.Linq;
using C2DMNet.Contracts.DataContracts;
using C2DMNet.Contracts.Enums;

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
                var res = client.SendAsync(request)
                    .ContinueWith(t =>
                                      {
                                          var responseCode = t.Result.StatusCode;
                                          string errorString;
                                          C2DMResponseCode c2DMResponse;
                                          if (responseCode.Equals(HttpStatusCode.OK))
                                          {
                                              errorString = t.Result.Content.ReadAsStringAsync().ContinueWith(s =>
                                                                                                                  {
                                                                                                                      var error = s.Result.Split('\n').FirstOrDefault(r => r.StartsWith("Error="));
                                                                                                                      return error != null ? error.Substring(6) : null;
                                                                                                                  }).Result;
                                              c2DMResponse=C2DMResponseCode.Ok;
                                          }
                                          else if (responseCode.Equals(HttpStatusCode.NotImplemented))
                                          {
                                              errorString = "Server unavailable.";
                                              c2DMResponse = C2DMResponseCode.ServerUnavailable;
                                          }
                                          else if (responseCode.Equals(HttpStatusCode.Unauthorized))
                                          {
                                              errorString = "Invalid AUTH_TOKEN";
                                              c2DMResponse = C2DMResponseCode.InvalidAuthToken;
                                          }
                                          else
                                          {
                                              errorString = "Unspecified error";
                                              c2DMResponse=C2DMResponseCode.Error;
                                          }
                                          return new SendMessageDataContract
                                                     {
                                                         ResponseCode = responseCode,
                                                         C2DMResponseCode = c2DMResponse,
                                                         Error=errorString,
                                                         UpdateClient = t.Result.Content.Headers.Contains("Update-Client-Auth") ? t.Result.Content.Headers.GetValues("Update-Client-Auth").First() : null
                                                     };
                                      });
                return res.Result;
            }
        }

        [Obsolete("Use other SendMessage.")]
        public HttpStatusCode SendMessage(string authToken, string registrationId, IDictionary<string, string> content, out string error)
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
                var res = client.SendAsync(request)
                    .ContinueWith(t =>
                                      {
                                          var responseCode = t.Result.StatusCode;
                                          string errorString;
                                          if (responseCode.Equals(HttpStatusCode.OK))
                                          {
                                              errorString = t.Result.Content.ReadAsStringAsync().ContinueWith(s =>
                                                                                                                  {
                                                                                                                      var errorOut = s.Result.Split('\n').FirstOrDefault(r => r.StartsWith("Error="));
                                                                                                                      return errorOut != null ? errorOut.Substring(6) : null;
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
                error = res.Result.Error;
                return res.Result.ResponseCode;
            }
        }

        private bool ValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }
}
