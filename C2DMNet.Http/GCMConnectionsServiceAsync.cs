using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using C2DMNet.Contracts;
using C2DMNet.Contracts.DataContracts;
using Newtonsoft.Json;

namespace C2DMNet.Http
{
    public class GCMConnectionsServiceAsync : IGCMConnectionServiceAsync
    {
        public async Task<GCMResult> SendMessageAsync(string authKey, string[] registrationIds, IDictionary<string, string> content)
        {
            using (var client = new HttpClient())
            {
                var jsonObject = new GCMSendJson
                                     {
                                         RegistrationIds = registrationIds,
                                         Data = content
                                     };
                string json = JsonConvert.SerializeObject(jsonObject);
                var request = new HttpRequestMessage(HttpMethod.Post, "https://android.googleapis.com/gcm/send")
                                  {
                                      Content = new StringContent(json, Encoding.UTF8, "application/json"),
                                  };
                ServicePointManager.ServerCertificateValidationCallback += ValidationCallback;
                request.Headers.TryAddWithoutValidation(HttpRequestHeader.Authorization.ToString(), string.Format("key={0}", authKey));
                var res = await client.SendAsync(request);
                var responseCode = res.StatusCode;
                GCMResultJson result = null;
                if (responseCode.Equals(HttpStatusCode.OK))
                {
                    var contentResult = await res.Content.ReadAsStringAsync();
                    result = JsonConvert.DeserializeObject<GCMResultJson>(contentResult);
                }

                return new GCMResult
                           {
                               ResponseCode = responseCode,
                               ResultJson = result
                           };
            }
        }

        private bool ValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }
}