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
            public const string AcceptedEntityType = "acceptedEntityType";
            public const string WebsiteKey = "websiteKey";

        }

        public static string About { get; set; } = "Web enricher allows you to get information about organization through their website";
        public static string Icon { get; set; } = "Resources.web.svg";
        public static string Domain { get; set; } = "N/A";

        public static AuthMethods AuthMethods { get; set; } = new AuthMethods
        {
            Token = new List<Control>()
            {
                new Control()
                {
                    DisplayName = "Accepted Entity Type",
                    Type = "input",
                    IsRequired = true,
                    Name = KeyName.AcceptedEntityType,
                    Help = "The entity type that defines the golden records you want to enrich. (e.g., /Organization)",
                },
                new Control()
                {
                    DisplayName = "Website Vocabulary Key",
                    Type = "input",
                    IsRequired = false,
                    Name = KeyName.WebsiteKey,
                    Help = "The vocabulary key that contains the websites of companies.",
                },
            }
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
