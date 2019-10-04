using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using CluedIn.Core;
using CluedIn.Processing.Web.Specific;

using Newtonsoft.Json;

using RestSharp;

namespace CluedIn.ExternalSearch.Providers.Web.Model
{
    public class WebResult
    {
        private OrganizationData cachedOrganizationData;

        public WebResult()
        {
        }

        public WebResult(Uri requestUri, IRestResponse response)
        {
            this.RequestUri   = requestUri;
            this.RestResponse = new WebRestResponse(response);
        }

        public Uri RequestUri { get; set; }
        public WebRestResponse RestResponse { get; set; }

        public OrganizationData GetOrganizationWebsiteMetadata(ExecutionContext context)
        {
            if (this.cachedOrganizationData != null)
                return this.cachedOrganizationData;

            var parser = new OrganizationWebsiteParser();

            this.cachedOrganizationData = parser.Parse(context, this.RequestUri, this.RestResponse);

            return this.cachedOrganizationData;
        }
    }

    public class WebRestResponse : IRestResponse
    {
        public WebRestResponse()
        {
            this.Cookies = new List<RestResponseCookie>();
            this.Headers = new List<Parameter>();
        }

        public WebRestResponse(IRestResponse response)
        {
            this.Request           = response.Request;
            this.ContentType       = response.ContentType;
            this.ContentLength     = response.ContentLength;
            this.ContentEncoding   = response.ContentEncoding;
            this.Content           = response.Content;
            this.StatusCode        = response.StatusCode;
            this.StatusDescription = response.StatusDescription;
            this.RawBytes          = response.RawBytes;
            this.ResponseUri       = response.ResponseUri;
            this.Server            = response.Server;
            this.Cookies           = response.Cookies;
            this.Headers           = response.Headers;
            this.ResponseStatus    = response.ResponseStatus;
            this.ErrorMessage      = response.ErrorMessage;
            this.ErrorException    = response.ErrorException;
        }

        [JsonIgnore]
        public IRestRequest Request { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ContentType { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long ContentLength { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ContentEncoding { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Content { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string StatusDescription { get; set; }

        [JsonIgnore]
        public byte[] RawBytes { get; set; }

        public Uri ResponseUri { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Server { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IList<RestResponseCookie> Cookies { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public IList<Parameter> Headers { get; set; }

        public ResponseStatus ResponseStatus { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ErrorMessage { get; set; }

        [JsonIgnore]
        public Exception ErrorException { get; set; }
    }
}
