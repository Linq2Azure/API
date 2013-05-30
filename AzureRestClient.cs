using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure
{
    class AzureRestClient
    {
        public readonly Subscription Subscription;
        public readonly Uri Uri;
        readonly HttpClient _httpClient;

        // We use the same HttpClient for all calls to the same subscription; this allows DNS and proxy details to be
        // cached across requests. Note that HttpClient allows parallel operations.
        internal static HttpClient CreateHttpClient (Subscription subscription, string msVersion)
        {
            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(subscription.ManagementCertificate);
            var logger = new LoggingHandler(handler);
            var client = new HttpClient(logger, true);
            client.DefaultRequestHeaders.Add("x-ms-version", msVersion);
            return client;
        }

        public AzureRestClient(Subscription subscription, HttpClient httpClient, string baseUri, string servicePath)
        {
            Contract.Requires(subscription != null);
            Contract.Requires(httpClient != null);
            Subscription = subscription;
            _httpClient = httpClient;
            Uri = new Uri(baseUri + subscription.ID + "/" + servicePath);
        }

        public async Task<XElement> GetXmlAsync()
        {
            var response = await _httpClient.GetAsync(Uri);
            if ((int)response.StatusCode >= 300) await AzureRestClient.ThrowAsync(response);
            string result = await response.Content.ReadAsStringAsync();
            return XElement.Parse(await _httpClient.GetStringAsync(Uri));
        }

        public Task<HttpResponseMessage> PostAsync(XElement xml) { return SendAsync(xml, HttpMethod.Post); }
        public Task<HttpResponseMessage> PutAsync(XElement xml) { return SendAsync(xml, HttpMethod.Put); }

        async Task<HttpResponseMessage> SendAsync (XElement xml, HttpMethod method)
        {
            Contract.Requires(xml != null);

            string xmlString = xml.ToString();
            var payload = new StringContent(xmlString);
            payload.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            HttpRequestMessage request = new HttpRequestMessage(method, Uri) { Content = payload };
            var response = await _httpClient.SendAsync(request);

            if ((int)response.StatusCode >= 300) await ThrowAsync(response, xmlString);
            return response;
        }

        public async Task<HttpResponseMessage> DeleteAsync()
        {
            var response = await _httpClient.DeleteAsync(Uri);
            if ((int)response.StatusCode >= 300) await ThrowAsync(response);
            return response;
        }

        static async Task ThrowAsync(HttpResponseMessage response, object debugInfo = null)
        {            
            string responseString = null;
            try { responseString = await response.Content.ReadAsStringAsync(); }
            catch { }

            XElement errorElement = null;
            if (!string.IsNullOrWhiteSpace(responseString))
                errorElement = XElement.Parse(responseString);

            Throw(response, errorElement, debugInfo);
        }

        internal static void Throw(HttpResponseMessage responseMessage, XElement errorElement, object debugInfo = null)
        {
            string code = null, message = null;

            if (errorElement != null)
            {
                var ns = XmlNamespaces.WindowsAzure;
                code = (string)errorElement.Elements().FirstOrDefault (e => e.Name.LocalName == "Code");
                message = (string)errorElement.Elements().FirstOrDefault(e => e.Name.LocalName == "Message");
            }

            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(message))
            {
                if (errorElement != null)
                    debugInfo = debugInfo == null 
                        ? errorElement.ToString()
                        : (debugInfo + "\r\n\r\n" + errorElement);

                throw new AzureRestException(responseMessage, null, "Unknown error", debugInfo);
            }
            else
                throw new AzureRestException(responseMessage, code, message, debugInfo);
        }

        // TODO - make this optional
        class LoggingHandler : DelegatingHandler
        {
            public LoggingHandler(HttpMessageHandler nextHandler)
            {
                InnerHandler = nextHandler;
            }

            protected async override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
            {
                bool operation = request.RequestUri.AbsolutePath.Contains("/operations/");                
                if (!operation) Debug.Write(request.Method + ": " + request.RequestUri);                
                var response = await base.SendAsync(request, cancellationToken);                
                if (operation) Debug.Write('.'); else Debug.WriteLine(" " + response.StatusCode);                
                return response;
            }
        }
    }
}
