using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using C2DMNet.Contracts;
using C2DMNet.Contracts.DataContracts;
using C2DMNet.Contracts.Enums;
using C2DMNet.Util;

namespace C2DMNet
{
    public class C2DMConnectionService : IC2DMConnectionService
    {
        private const string ParamRegistrationId = "registration_id";

        private const string ParamCollapseKey = "collapse_key";

        public SendMessageDataContract SendMessage(string authToken, string registrationId,
                                      IDictionary<string, string> content)
        {

            var postDataBuilder = new StringBuilder();
            postDataBuilder.Append(ParamRegistrationId).Append("=")
                .Append(registrationId);
            postDataBuilder.Append("&").Append(ParamCollapseKey).Append("=")
                .Append("0");
            foreach (var kvp in content)
            {
                postDataBuilder.Append("&").Append("data.").Append(kvp.Key).Append("=")
                   .Append(HttpUtility.UrlEncodeUnicode(kvp.Value));
            }
            

            var encoding = new UTF8Encoding();
            byte[] postData = encoding.GetBytes(postDataBuilder.ToString());

            var url = new Uri("https://android.clients.google.com/c2dm/send");
            ServicePointManager.ServerCertificateValidationCallback += ValidationCallback;

            var conn = (HttpWebRequest)WebRequest.Create(url);
            conn.Proxy = null;

            conn.Method = "POST";
            conn.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            conn.ContentLength = postData.Length;
            conn.Headers.Add("Authorization", "GoogleLogin auth="
                                                     + authToken);
            var sort = conn.GetRequestStream();
            sort.Write(postData, 0, postData.Length);
            sort.Close();

            var httpWebResponse = ((HttpWebResponse)conn.GetResponse());
            var responseCode = httpWebResponse.StatusCode;
            string error;
            C2DMResponseCode c2DMResponse;
            if (responseCode.Equals(HttpStatusCode.OK))
            {
                var streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                var errorString = streamReader.ReadLines().FirstOrDefault(t => t.StartsWith("Error="));
                error = errorString!=null?errorString.Substring(6):null;
                c2DMResponse=C2DMResponseCode.Ok;
            }
            else if(responseCode.Equals(HttpStatusCode.NotImplemented))
            {
                error = "Server unavailable.";
                c2DMResponse=C2DMResponseCode.ServerUnavailable;
            }
            else if(responseCode.Equals(HttpStatusCode.Unauthorized))
            {
                error = "Invalid AUTH_TOKEN";
                c2DMResponse=C2DMResponseCode.InvalidAuthToken;
            }
            else
            {
                error = "Unspecified error";
                c2DMResponse=C2DMResponseCode.Error;
            }

            return new SendMessageDataContract
                       {
                           Error = error,
                           C2DMResponseCode = c2DMResponse,
                           ResponseCode = responseCode,
                           UpdateClient = httpWebResponse.Headers["Update-Client-Auth"]
                       };
        }

        [Obsolete("Use other SendMessage.")]
        public HttpStatusCode SendMessage(string authToken, string registrationId, IDictionary<string, string> content, out string error)
        {
            var postDataBuilder = new StringBuilder();
            postDataBuilder.Append(ParamRegistrationId).Append("=")
                .Append(registrationId);
            postDataBuilder.Append("&").Append(ParamCollapseKey).Append("=")
                .Append("0");
            foreach (var kvp in content)
            {
                postDataBuilder.Append("&").Append("data.").Append(kvp.Key).Append("=")
                   .Append(HttpUtility.UrlEncodeUnicode(kvp.Value));
            }


            var encoding = new UTF8Encoding();
            byte[] postData = encoding.GetBytes(postDataBuilder.ToString());

            var url = new Uri("https://android.clients.google.com/c2dm/send");
            ServicePointManager.ServerCertificateValidationCallback += ValidationCallback;

            var conn = (HttpWebRequest)WebRequest.Create(url);
            conn.Proxy = null;

            conn.Method = "POST";
            conn.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";
            conn.ContentLength = postData.Length;
            conn.Headers.Add("Authorization", "GoogleLogin auth="
                                                     + authToken);
            var sort = conn.GetRequestStream();
            sort.Write(postData, 0, postData.Length);
            sort.Close();

            var httpWebResponse = ((HttpWebResponse)conn.GetResponse());
            var responseCode = httpWebResponse.StatusCode;
            if (responseCode.Equals(HttpStatusCode.OK))
            {
                var streamReader = new StreamReader(httpWebResponse.GetResponseStream());
                error = streamReader.ReadLines().First(t => t.StartsWith("Error=")).Substring(6);
            }
            else if (responseCode.Equals(HttpStatusCode.NotImplemented))
            {
                error = "Server unavailable.";
            }
            else if (responseCode.Equals(HttpStatusCode.Unauthorized))
            {
                error = "Invalid AUTH_TOKEN";
            }
            else
            {
                error = "Unspecified error";
            }

            return responseCode;
        }

        public string GetToken(string email, string password, string source)
        {
            var builder = new StringBuilder();
            builder.Append("Email=").Append(email);
            builder.Append("&Passwd=").Append(password);
            builder.Append("&accountType=GOOGLE");
            builder.Append("&source=").Append(source);
            builder.Append("&service=ac2dm");

            var encoding = new UTF8Encoding();
            byte[] data = encoding.GetBytes(builder.ToString());
            var url = new Uri("https://www.google.com/accounts/ClientLogin");
            var con = WebRequest.Create(url.ToString());

            con.Method = "POST";
            con.ContentLength = data.Length;
            con.ContentType = "application/x-www-form-urlencoded";


            // Issue the HTTP POST request

            Stream output = con.GetRequestStream();
            output.Write(data, 0, data.Length);
            output.Close();

            // Read the response
            WebResponse response = con.GetResponse();
            var reader = new StreamReader(response.GetResponseStream());
            return reader.ReadLines().First(t => t.StartsWith("Auth=")).Substring(5);
        }

        private static bool ValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslpolicyerrors)
        {
            return true;
        }
    }
}