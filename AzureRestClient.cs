using System;
using System.Collections.Generic;
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
        public static readonly string BaseUri = "https://management.core.windows.net/";

        public readonly HttpClient HttpClient;

        public AzureRestClient(Subscription subscription, string relativeUri)
        {
            Contract.Requires(subscription != null);

            Uri requestUri = new Uri(BaseUri + subscription.SubscriptionID + "/services/" + relativeUri);
            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(subscription.ManagementCertificate);
            var logger = new LoggingHandler(handler);
            HttpClient = new HttpClient(logger, true) { BaseAddress = requestUri };
            HttpClient.DefaultRequestHeaders.Add("x-ms-version", "2012-03-01");
        }

        public async Task<XElement> GetXmlAsync()
        {
            var response = await HttpClient.GetAsync("");
            if ((int)response.StatusCode >= 300) await AzureRestClient.ThrowAsync(response);
            string result = await response.Content.ReadAsStringAsync();
            return XElement.Parse(await HttpClient.GetStringAsync(""));
        }

        public async Task PostAsync (XElement xml)
        {
            Contract.Requires(xml != null);

            var payload = new StringContent(xml.ToString());
            payload.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            var response = await HttpClient.PostAsync("", payload);
            if (response.StatusCode != HttpStatusCode.Created) await ThrowAsync(response);
        }

        public async Task DeleteAsync()
        {
            var response = await HttpClient.DeleteAsync("");
            if (response.StatusCode != HttpStatusCode.OK) await ThrowAsync(response);
        }

        static async Task ThrowAsync(HttpResponseMessage response)
        {            
            string responseString = null;
            try { responseString = await response.Content.ReadAsStringAsync(); }
            catch { }

            string code = null, message = null;

            if (!string.IsNullOrWhiteSpace(responseString))
            {
                var ns = XmlNamespaces.Base;
                var errorElement = XElement.Parse(responseString);
                code = (string)errorElement.Element(ns + "Code");
                message = (string)errorElement.Element(ns + "Message");
            }

            if (string.IsNullOrWhiteSpace(code) && string.IsNullOrWhiteSpace(message))
                throw new InvalidOperationException("Error: " + response.StatusCode);
            else
                throw new InvalidOperationException(string.Join(" - ", new[] { code, message }));
        }

        // TODO - make this optional
        class LoggingHandler : DelegatingHandler
        {
            public LoggingHandler(HttpMessageHandler nextHandler)
            {
                InnerHandler = nextHandler;
            }

            protected async override Task<HttpResponseMessage> SendAsync
              (HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Console.WriteLine("Requesting: " + request.RequestUri);
                var response = await base.SendAsync(request, cancellationToken);
                Console.WriteLine("Got response: " + response.StatusCode);
                return response;
            }
        }
    }
}
