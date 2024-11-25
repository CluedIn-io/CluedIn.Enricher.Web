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
        public const string Instruction = """
            [
              {
                "type": "bulleted-list",
                "children": [
                  {
                    "type": "list-item",
                    "children": [
                      {
                        "text": "Add the entity type to specify the golden records you want to enrich. Only golden records belonging to that entity type will be enriched."
                      }
                    ]
                  },
                  {
                    "type": "list-item",
                    "children": [
                      {
                        "text": "Add the vocabulary keys to provide the input for the enricher to search for additional information. For example, if you provide the website vocabulary key for the Web enricher, it will use specific websites to look for information about companies. In some cases, vocabulary keys are not required. If you don't add them, the enricher will use default vocabulary keys."
                      }
                    ]
                  }
                ]
              }
            ]
            """;
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
                    Help = "The entity type that defines the golden records you want to enrich (e.g., /Organization).",
                },
                new Control()
                {
                    DisplayName = "Website Vocabulary Key",
                    Type = "input",
                    IsRequired = false,
                    Name = KeyName.WebsiteKey,
                    Help = "The vocabulary key that contains the websites of companies you want to enrich (e.g., organization.website).",
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

        public static Guide Guide { get; set; } = new Guide
        {
            Instructions = Instruction
        };
        public static IntegrationType IntegrationType { get; set; } = IntegrationType.Enrichment;
    }
}
