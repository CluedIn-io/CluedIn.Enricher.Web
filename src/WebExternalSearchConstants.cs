using CluedIn.Core.Data.Relational;
using CluedIn.Core.Providers;
using System;
using System.Collections.Generic;
using System.Text;

namespace CluedIn.ExternalSearch.Providers.Web
{
    public static class WebExternalSearchConstants
    {
        public const string ComponentName = "WebEnricher";
        public const string ProviderName = "Web";
        public static readonly Guid ProviderId = Guid.Parse("41FD7617-F9AD-45AA-A20A-04F2C26BDDC6");

        public struct KeyName
        {
            public const string ApiToken = "apiToken";

        }

        public static string About { get; set; } = "Web enricher allows you to get information about organization through their website";
        public static string Icon { get; set; } = "Resources.web.svg";
        public static string Domain { get; set; } = "N/A";

        public static AuthMethods AuthMethods { get; set; } = new AuthMethods
        {
            token = new List<Control>()
        };

        public static IEnumerable<Control> Properties { get; set; } = new List<Control>()
        {
            // NOTE: Leaving this commented as an example - BF
            //new()
            //{
            //    displayName = "Some Data",
            //    type = "input",
            //    isRequired = true,
            //    name = "someData"
            //}
        };

        public static Guide Guide { get; set; } = null;
        public static IntegrationType IntegrationType { get; set; } = IntegrationType.Enrichment;
    }
}
