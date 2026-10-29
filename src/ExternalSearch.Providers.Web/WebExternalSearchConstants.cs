using CluedIn.Core.Data.Relational;
using CluedIn.Core.Providers;
using System;
using System.Collections.Generic;

namespace CluedIn.ExternalSearch.Providers.Web
{
    public static class WebExternalSearchConstants
    {
        public const string ComponentName = "WebEnricher";
        public const string ProviderName = "Web";
        public static readonly Guid ProviderId = Guid.Parse("41FD7617-F9AD-45AA-A20A-04F2C26BDDC6");
        public static readonly string Instruction = $$"""
                                                      [
                                                        {
                                                          "type": "bulleted-list",
                                                          "children": [
                                                            {
                                                              "type": "list-item",
                                                              "children": [
                                                                {
                                                                  "text": "Add the {{EntityTypeLabel.ToLower()}} to specify the golden records you want to enrich. Only golden records belonging to that {{EntityTypeLabel.ToLower()}} will be enriched."
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

        private static Version _cluedInVersion;
        public static Version CluedInVersion => _cluedInVersion ??= typeof(Core.Constants).Assembly.GetName().Version;
        public static string EntityTypeLabel => CluedInVersion < new Version(4, 5, 0) ? "Entity Type" : "Business Domain";

        public static AuthMethods AuthMethods { get; set; } = new AuthMethods
        {
            Token = new List<Control>()
            {
                new Control()
                {
                    DisplayName = $"Accepted {EntityTypeLabel}",
                    Type = "entityTypeSelector",
                    IsRequired = true,
                    Name = KeyName.AcceptedEntityType,
                    Help = $"The {EntityTypeLabel.ToLower()} that defines the golden records you want to enrich (e.g., /Organization).",
                },
                new Control()
                {
                    DisplayName = "Website Vocabulary Key",
                    Type = "vocabularyKeySelector",
                    IsRequired = true,
                    Name = KeyName.WebsiteKey,
                    Help = "The vocabulary key that contains the websites of companies you want to enrich (e.g., organization.website).",
                },
            }
        };

        private static Dictionary<string, object> CreateFieldGroup(string id, string label, params string[] fields) => new()
        {
            ["id"] = id,
            ["label"] = label,
            ["fields"] = fields,
        };

        /// <summary>
        /// The sections the UI groups the configuration fields into, in order. Fields no group lists
        /// (such as the filter and rate limit settings added by CluedIn) go to the last group.
        /// </summary>
        public static readonly Dictionary<string, object>[] FieldGroups =
        [
            CreateFieldGroup("whatToEnrich", "What to enrich", KeyName.AcceptedEntityType, "enricherRuleCondition"),
            CreateFieldGroup("fieldMapping", "Field mapping", KeyName.WebsiteKey),
            CreateFieldGroup("advanced", "Advanced"),
        ];

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
