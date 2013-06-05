using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Linq2Azure
{
    public class AzureRestException : ApplicationException
    {
        /// <summary>The response message upon which an exception was thrown.</summary>
        public HttpResponseMessage ResponseMessage { get; private set; }     
   
        public string StatusCode { get; private set; }

        /// <summary>This is populated with useful diagnostic data such as the original request XML.</summary>
        public object DebugInfo { get; set; }

        public AzureRestException(HttpResponseMessage responseMessage, string statusCode, string message, object debugInfo) : base (message)
        {
            ResponseMessage = responseMessage;
            StatusCode = statusCode;
            DebugInfo = debugInfo;
        }
    }
}
