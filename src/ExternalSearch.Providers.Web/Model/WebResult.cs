using System;

using CluedIn.Core;
using CluedIn.Processing.Web.Specific;
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
}
