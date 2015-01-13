using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Linq2Azure
{
    /// <summary>
    /// Performs REST-like options on the Azure HTTP management endpoint, taking care of authentication, error
    /// handling and other functions common to all requests. This also handles optional request logging.
    /// </summary>
    class AzureRestClient
    {
        public readonly Subscription Subscription;
        public readonly Uri Uri;
        readonly HttpClient _httpClient;

        private const string TimeoutErrorMessage = "HttpClient threw a TaskCanceledException, as we didn't provide a cancellation token, we're assuming this is a timeout.";

        // We use the same HttpClient for all calls to the same subscription; this allows DNS and proxy details to be
        // cached across requests. Note that HttpClient allows parallel operations.
        internal static HttpClient CreateHttpClient (Subscription subscription, string msVersion, Func<IEnumerable<TraceListener>> listenerFunc)
        {
            var handler = new WebRequestHandler();
            handler.ClientCertificates.Add(subscription.ManagementCertificate);
            var logger = new LoggingHandler(handler, listenerFunc);
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
            var response = await WrapAsync(() => _httpClient.GetAsync(Uri));
            return await AsXmlResponse(response);
        }

        public async Task<XElement> PostWithXmlResponseAsync(XElement xml)
        {
            var response = await SendAsync(xml, HttpMethod.Post);
            return await AsXmlResponse(response);
        }

        public Task<HttpResponseMessage> PostAsync(XElement xml) { return SendAsync(xml, HttpMethod.Post); }
        public Task<HttpResponseMessage> PostAsync() { return SendAsync(null, HttpMethod.Post); }
        public Task<HttpResponseMessage> PutAsync(XElement xml) { return SendAsync(xml, HttpMethod.Put); }

        async Task<HttpResponseMessage> SendAsync (XElement xml, HttpMethod method)
        {
            string xmlString = xml == null ? "" : xml.ToString(SaveOptions.DisableFormatting);
            var payload = new StringContent(xmlString);
            payload.Headers.ContentType = new MediaTypeHeaderValue("application/xml");

            var request = new HttpRequestMessage(method, Uri) { Content = payload };
            var response = await WrapAsync(() => _httpClient.SendAsync(request));

            if ((int)response.StatusCode >= 300) await ThrowAsync(response, xmlString);
            return response;
        }

        public async Task<HttpResponseMessage> DeleteAsync()
        {
            var response = await WrapAsync(() => _httpClient.DeleteAsync(Uri));
            if ((int)response.StatusCode >= 300) await ThrowAsync(response);
            return response;
        }

        static async Task<HttpResponseMessage> WrapAsync(Func<Task<HttpResponseMessage>> f)
        {
            try
            {
                return await f();
            }
            catch (TaskCanceledException tex)
            {
                throw new TimeoutException(TimeoutErrorMessage, tex);
            }
            catch (AggregateException aex)
            {
                var b = aex.GetBaseException();
                if (b is TaskCanceledException)
                {
                    throw new TimeoutException(TimeoutErrorMessage, aex);
                }
                throw;
            }
        }

        static async Task<XElement> AsXmlResponse(HttpResponseMessage response)
        {
            if ((int)response.StatusCode >= 300) await ThrowAsync(response);
            var result = await response.Content.ReadAsStringAsync();
            return XElement.Parse(result);
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
                code = (string)errorElement.Elements().FirstOrDefault(e => e.Name.LocalName == "Code");
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

        internal class LoggingHandler : DelegatingHandler
        {
            Func<IEnumerable<TraceListener>> _listenerFunc;

            public LoggingHandler(HttpMessageHandler nextHandler, Func<IEnumerable<TraceListener>> listenerFunc)
            {
                InnerHandler = nextHandler;
                _listenerFunc = listenerFunc;
            }

            IEnumerable<TraceListener> Listeners 
            {
                get { return _listenerFunc() ?? new TraceListener[0]; }
            }

            void Write(string msg) { foreach (var l in Listeners) l.Write(msg); }
            void WriteLine(string msg) { foreach (var l in Listeners) l.WriteLine(msg); }

            protected async override Task<HttpResponseMessage> SendAsync (HttpRequestMessage request, CancellationToken cancellationToken)
            {
                bool operation = request.RequestUri.AbsolutePath.Contains("/operations/");
                if (!operation) Write(request.Method + ": " + request.RequestUri);                
                var response = await base.SendAsync(request, cancellationToken);
                if (operation) Write("."); else WriteLine(" " + response.StatusCode);                
                return response;
            }
        }
    }
}
