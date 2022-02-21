using CluedIn.Core.Crawling;
using System.Collections.Generic;

namespace CluedIn.ExternalSearch.Providers.Web
{
    public class WebExternalSearchJobData : CrawlJobData
    {
        public WebExternalSearchJobData(IDictionary<string, object> configuration)
        {
            // ApiToken = GetValue<string>(configuration, WebConstants.KeyName.ApiToken);
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>();
        }
    }
}
