using CluedIn.Core.Crawling;
using System.Collections.Generic;

namespace CluedIn.ExternalSearch.Providers.Web
{
    public class WebExternalSearchJobData : CrawlJobData
    {
        public WebExternalSearchJobData(IDictionary<string, object> configuration)
        {
            AcceptedEntityType = GetValue<string>(configuration, WebExternalSearchConstants.KeyName.AcceptedEntityType);
            WebsiteKey = GetValue<string>(configuration, WebExternalSearchConstants.KeyName.WebsiteKey);
        }

        public IDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>()
            {
                { WebExternalSearchConstants.KeyName.AcceptedEntityType, AcceptedEntityType },
                { WebExternalSearchConstants.KeyName.WebsiteKey, WebsiteKey }
            };
        }
        public string AcceptedEntityType { get; set; }
        public string WebsiteKey { get; set; }
    }
}
