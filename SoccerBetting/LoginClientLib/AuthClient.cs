using LoginClientLib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LoginClientLib
{
    internal class AuthClient
    {
        private readonly string appKey;

        public string AppKey => appKey;

        private WebRequestHandler getWebRequestHandlerWithCert(string certFilename)
        {
            //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3;

            var cert = new X509Certificate2(certFilename, "");
            var clientHandler = new WebRequestHandler();
            clientHandler.ClientCertificates.Add(cert);
            return clientHandler;
        }

        private const string DEFAULT_COM_BASEURL = "https://identitysso-cert.betfair.com";

        private HttpClient initHttpClientInstance(WebRequestHandler clientHandler, string appKey, string baseUrl = DEFAULT_COM_BASEURL)
        {
            //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3;
            var client = new HttpClient(clientHandler)
            {
                BaseAddress = new Uri(baseUrl)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("X-Application", appKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private FormUrlEncodedContent getLoginBodyAsContent(string username, string password)
        {
            //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3;
            var postData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            };
            return new FormUrlEncodedContent(postData);
        }

        public LoginResponse doLogin(string username, string password, string certFilename)
        {
            var handler = getWebRequestHandlerWithCert(certFilename);
            var client = initHttpClientInstance(handler, appKey);
            var content = getLoginBodyAsContent(username, password);
            var result = client.PostAsync("/api/certlogin", content).Result;
            result.EnsureSuccessStatusCode();
            var jsonSerialiser = new DataContractJsonSerializer(typeof(LoginResponse));
            var stream = new MemoryStream(result.Content.ReadAsByteArrayAsync().Result);
            return (LoginResponse)jsonSerialiser.ReadObject(stream);

            //var handler = getWebRequestHandlerWithCert(certFilename);
            //var client = initHttpClientInstance(handler, appKey);
            //var content = getLoginBodyAsContent(username, password);
            //var result = client.PostAsync("/api/certlogin", content).Result;
            //result.EnsureSuccessStatusCode();
            //var jsonSerialiser = new DataContractJsonSerializer(typeof(LoginResponse));
            //var stream = new MemoryStream(result.Content.ReadAsByteArrayAsync().Result);
            //return (LoginResponse)jsonSerialiser.ReadObject(stream);
        }

        public AuthClient(string appKey)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.DefaultConnectionLimit = 9999;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3;
            //System.Net.ServicePointManager.Expect100Continue = true;
            appKey = appKey;
        }
    }
}