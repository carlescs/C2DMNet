using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using C2DMNet.Contracts;
using C2DMNet.Contracts.DataContracts;
using Newtonsoft.Json;

namespace C2DMNet
{
    public class GCMConnectionService : IGCMConnectionService
    {
        public GCMResult SendMessage(string authKey, string[] registrationIds, IDictionary<string, string> content)
        {
            var jsonObject = new GCMSendJson
            {
                RegistrationIds = registrationIds,
                Data = content
            };
            string json = JsonConvert.SerializeObject(jsonObject);
            var encoding = new UTF8Encoding();
            byte[] postData = encoding.GetBytes(json);

            var url = new Uri("https://android.googleapis.com/gcm/send");
            ServicePointManager.ServerCertificateValidationCallback += ValidationCallback;

            var conn = (HttpWebRequest)WebRequest.Create(url);
            conn.Proxy = null;

            conn.Method = "POST";
            conn.ContentType = "application/json;charset=UTF-8";
            conn.ContentLength = postData.Length;
            conn.Headers.Add("Authorization", string.Format("key={0}", authKey));
            var sort = conn.GetRequestStream();
            sort.Write(postData, 0, postData.Length);
            sort.Close();

            var httpWebResponse = ((HttpWebResponse)conn.GetResponse());
            var responseCode = httpWebResponse.StatusCode;
            GCMResultJson jsonResult=null;
            if (responseCode.Equals(HttpStatusCode.OK))
            {
                var streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                var errorString = streamReader.ReadToEnd();
                jsonResult = JsonConvert.DeserializeObject<GCMResultJson>(errorString);
            }

            return new GCMResult
                       {
                           ResponseCode = responseCode,
                           ResultJson = jsonResult
                       };
        }

        private bool ValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }
}