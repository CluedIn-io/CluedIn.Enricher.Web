// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebExternalSearchProvider.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the WebExternalSearchProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using CluedIn.Core;
using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.ExternalSearch;
using CluedIn.Core.FileTypes;
using CluedIn.Core.Providers;
using CluedIn.Core.Utilities;
using CluedIn.Crawling.Helpers;
using CluedIn.ExternalSearch.Providers.Web.Model;
using CluedIn.ExternalSearch.Providers.Web.Vocabularies;
using CluedIn.Processing.Web.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

using RestSharp;

using CluedInVocabularies = CluedIn.Core.Data.Vocabularies.Vocabularies;
using EntityType = CluedIn.Core.Data.EntityType;

namespace CluedIn.ExternalSearch.Providers.Web
{
    /// <summary>The web external search provider.</summary>
    /// <seealso cref="CluedIn.ExternalSearch.ExternalSearchProviderBase" />
    public partial class WebExternalSearchProvider : ExternalSearchProviderBase, IExternalSearchResultLogger, IExtendedEnricherMetadata, IConfigurableExternalSearchProvider
    {
        private static readonly EntityType[] AcceptedEntityTypes = { EntityType.Organization };

        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="WebExternalSearchProvider" /> class.
        /// </summary>
        public WebExternalSearchProvider()
            : base(ExternalSearchProviderPriority.First, WebExternalSearchConstants.ProviderId, AcceptedEntityTypes)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            JsonUtility.ConfigureDefaultSerializerSettings(settings);
            settings.TypeNameHandling       = TypeNameHandling.Objects;
            settings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;

            //settings.Binder = new TypeNameSerializationBinder();

            this.jsonSerializer = JsonSerializer.Create(settings);
        }

        /**********************************************************************************************************
         * METHODS
         **********************************************************************************************************/

        /// <summary>Builds the queries.</summary>
        /// <param name="context">The context.</param>
        /// <param name="request">The request.</param>
        /// <returns>The search queries.</returns>
        public override IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request)
        {
            foreach (var externalSearchQuery in InternalBuildQueries(context, request))
            {
                yield return externalSearchQuery;
            }
        }
        private IEnumerable<IExternalSearchQuery> InternalBuildQueries(ExecutionContext context, IExternalSearchRequest request, IDictionary<string, object> config = null)
        {
            if (config.TryGetValue(WebExternalSearchConstants.KeyName.AcceptedEntityType, out var customType) && !string.IsNullOrWhiteSpace(customType.ToString()))
            {
                if (!request.EntityMetaData.EntityType.Is(customType.ToString()))
                {
                    yield break;
                }
            }
            else if (!this.Accepts(request.EntityMetaData.EntityType)) yield break;

            var existingResults = request.GetQueryResults<WebResult>(this).ToList();

            Func<string, bool> requestUriFilter = value => value == null
                                                        || existingResults.Any(r => string.Equals(r.Data.RequestUri.ToString(), value, StringComparison.InvariantCultureIgnoreCase)
                                                        || UriUtility.IsSocialProfileUri(value));

            // Query Input
            var entityType      = request.EntityMetaData.EntityType;
            var website         = new HashSet<string>();
            if (config.TryGetValue(WebExternalSearchConstants.KeyName.WebsiteKey, out var customVocabKeyWebsite) && !string.IsNullOrWhiteSpace(customVocabKeyWebsite?.ToString()))
            {
                website = request.QueryParameters.GetValue<string, HashSet<string>>(customVocabKeyWebsite.ToString(), new HashSet<string>());
            }
            else
            {
                website = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website, new HashSet<string>()).ToHashSetEx();
            }

            if (website != null)
            {
                var values = website.SelectMany(v => v.Split(new[] { ",", ";", "|", " " }, StringSplitOptions.RemoveEmptyEntries)).ToHashSetEx();

                values = values.Select(UriUtility.NormalizeHttpUri).ToHashSetEx();

                foreach (var value in values.Where(v => !requestUriFilter(v)))
                    yield return new ExternalSearchQuery(this, entityType, ExternalSearchQueryParameter.Uri, value);
            }
        }

        /// <summary>Executes the search.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <returns>The results.</returns>
        public override IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query)
        {
            var uriText = query.QueryParameters[ExternalSearchQueryParameter.Uri].FirstOrDefault();

            if (string.IsNullOrEmpty(uriText))
                yield break;

            Uri uri;

            if (Uri.IsWellFormedUriString(uriText, UriKind.Absolute))
                uri = new Uri(uriText);
            else if (!uriText.StartsWith("http://") && !uriText.StartsWith("https://"))
            {
                if (Uri.IsWellFormedUriString("http://" + uriText, UriKind.Absolute))
                    uri = new Uri("http://" + uriText);
                else
                    throw new Exception("Invalid query uri: " + uriText);
            }
            else
                throw new Exception("Invalid query uri: " + uriText);

            var client      = new RestClient(uri);
            var request     = new RestRequest("/", Method.GET);

            request.AddHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.76 Safari/537.36");

            var response    = client.Get(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                context.Log.LogDebug("HTTP Response Code: {ResponseCode}" ,response.StatusCode);
                context.Log.LogDebug("HTTP Error: {Error}", response.ErrorMessage);
                throw new Exception("Could not download page: " + response.ErrorMessage);
            }
            yield return new ExternalSearchQueryResult<WebResult>(query, new WebResult(uri, response));
        }

        /// <summary>Builds the clues.</summary>
        /// <param name="context">The context.</param>
        /// <param name="query">The query.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The clues.</returns>
        public override IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<WebResult>();

            var code = this.GetOriginEntityCode(resultItem, request);

            var clue = new Clue(code, context.Organization);
            clue.Data.OriginProviderDefinitionId = this.Id;

            this.PopulateMetadata(context, clue.Data.EntityData, resultItem, request);

            if (clue.Data.EntityData.Properties.ContainsKey(WebVocabulary.Website.Logo))
                this.DownloadPreviewImage(context, clue.Data.EntityData.Properties[WebVocabulary.Website.Logo], clue);

            var orgWebSite = resultItem.Data.GetOrganizationWebsiteMetadata(context);

            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead(orgWebSite.Logo)) //Get Full Quality Image
                {
                    var inArray = StreamUtilies.ReadFully(stream);
                    if (inArray != null)
                    {
                        var rawDataPart = new RawDataPart()
                        {
                            Type = "/RawData/PreviewImage",
                            MimeType = MimeType.Jpeg.Code,
                            FileName = "preview_{0}".FormatWith(code.Key),
                            RawDataMD5 = FileHashUtility.GetMD5Base64String(inArray),
                            RawData = Convert.ToBase64String(inArray)
                        };

                        clue.Details.RawData.Add(rawDataPart);

                        clue.Data.EntityData.PreviewImage = new ImageReferencePart(rawDataPart);
                    }
                }
            }
            catch (Exception)
            {
                //Swallow
            }

            return new[] { clue };
        }

        /// <summary>Gets the primary entity metadata.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The primary entity metadata.</returns>
        public override IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var resultItem = result.As<WebResult>();
            return this.CreateMetadata(context, resultItem, request);
        }

        /// <summary>Gets the preview image.</summary>
        /// <param name="context">The context.</param>
        /// <param name="result">The result.</param>
        /// <param name="request">The request.</param>
        /// <returns>The preview image.</returns>
        public override IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request)
        {
            var metadata = GetPrimaryEntityMetadata(context, result, request);

            if (metadata.Properties.ContainsKey(WebVocabulary.Website.Logo))
                return this.DownloadPreviewImageBlob(context, metadata.Properties[WebVocabulary.Website.Logo]);


          

            return null;
        }

        /// <summary>Creates the metadata.</summary>
        /// <param name="context">The context.</param>
        /// <param name="resultItem">The result item.</param>
        /// <param name="request"></param>
        /// <returns>The metadata.</returns>
        private IEntityMetadata CreateMetadata(ExecutionContext context, IExternalSearchQueryResult<WebResult> resultItem, IExternalSearchRequest request)
        {
            var metadata = new EntityMetadataPart();

            this.PopulateMetadata(context, metadata, resultItem, request);

            return metadata;
        }

        /// <summary>Gets the origin entity code.</summary>
        /// <param name="resultItem">The result item.</param>
        /// <returns>The origin entity code.</returns>
        private EntityCode GetOriginEntityCode(IExternalSearchQueryResult<WebResult> resultItem, IExternalSearchRequest request)
        {
            return new EntityCode(EntityType.Organization, this.GetCodeOrigin(), request.EntityMetaData.OriginEntityCode.Value);
        }

        /// <summary>Gets the code origin.</summary>
        /// <returns>The code origin</returns>
        private CodeOrigin GetCodeOrigin()
        {
            return CodeOrigin.CluedIn.CreateSpecific("website");
        }

        /// <summary>Populates the metadata.</summary>
        /// <param name="context">The context.</param>
        /// <param name="metadata">The metadata.</param>
        /// <param name="resultItem">The result item.</param>
        /// <param name="request"></param>
        private void PopulateMetadata(ExecutionContext context, IEntityMetadata metadata, IExternalSearchQueryResult<WebResult> resultItem, IExternalSearchRequest request)
        {
            var code = this.GetOriginEntityCode(resultItem, request);

            var orgWebSite  = resultItem.Data.GetOrganizationWebsiteMetadata(context);

            var name = orgWebSite.Name;

            metadata.EntityType             = request.EntityMetaData.EntityType;
            metadata.Name                   = request.EntityMetaData.Name;
            metadata.OriginEntityCode       = code;
            metadata.Uri                    = orgWebSite.RequestUri;
            metadata.Description            = orgWebSite.WebsiteDescription;

            metadata.Codes.Add(code);
            metadata.Codes.Add(request.EntityMetaData.OriginEntityCode);

            //// Aliases
            if (orgWebSite.SchemaOrgOrganization != null)
            {
                if (!string.IsNullOrEmpty(orgWebSite.SchemaOrgOrganization.LegalName))
                    metadata.Aliases.Add(orgWebSite.SchemaOrgOrganization.LegalName);

                if (!string.IsNullOrEmpty(orgWebSite.SchemaOrgOrganization.AlternateName))
                    metadata.Aliases.Add(orgWebSite.SchemaOrgOrganization.AlternateName);

                if (!string.IsNullOrEmpty(orgWebSite.CopyrightEntity))
                    metadata.Aliases.Add(orgWebSite.CopyrightEntity);
            }

            var technologiesListText = string.Join(", ", orgWebSite.Technologies.Select(t => t.Name).OrderBy(t => t));
         
            metadata.Properties[WebVocabulary.Website.URI]    = resultItem.Data.RequestUri.PrintIfAvailable();

            metadata.Properties[WebVocabulary.Website.WebsiteDescription]                  = orgWebSite.WebsiteDescription;
            metadata.Properties[WebVocabulary.Website.Title]                        = orgWebSite.WebsiteTitle;
            metadata.Properties[WebVocabulary.Website.Logo]                         = orgWebSite.Logo.PrintIfAvailable();
            metadata.Properties[WebVocabulary.Website.CopyrightEntity]              = orgWebSite.CopyrightEntity;

            metadata.Properties[WebVocabulary.Website.Name]   = orgWebSite.Name;
            metadata.Properties[WebVocabulary.Website.PhoneNumber]        = orgWebSite.PhoneNumber;
            metadata.Properties[WebVocabulary.Website.FaxNumber]                = orgWebSite.FaxNumber;
            metadata.Properties[WebVocabulary.Website.ContactEmail]       = orgWebSite.ContactEmail;
            metadata.Properties[WebVocabulary.Website.Address]            = orgWebSite.Address;
            metadata.Properties[WebVocabulary.Website.Country] = orgWebSite.Country;
            metadata.Properties[WebVocabulary.Website.TechnologiesListText]   = technologiesListText;

            if (orgWebSite.SchemaOrgOrganization != null)
            {
                var postalAddress = orgWebSite.SchemaOrgOrganization.Address as SchemaOrgPostalAddress;
                if (postalAddress != null)
                {
                    metadata.Properties[WebVocabulary.Website.AddressCountry]     = (postalAddress.AddressCountry as SchemaOrgCountry).PrintIfAvailable(v => v.Name) ?? postalAddress.AddressCountry.PrintIfAvailable();
                    metadata.Properties[WebVocabulary.Website.PostalCode]         = postalAddress.PostalCode;
                    metadata.Properties[WebVocabulary.Website.Address]                = postalAddress.StreetAddress;
                }
                else if (orgWebSite.SchemaOrgOrganization.Address is SchemaOrgText)
                {
                    metadata.Properties[WebVocabulary.Website.Address]                = orgWebSite.SchemaOrgOrganization.Address.PrintIfAvailable();
                }

                metadata.Properties[WebVocabulary.Website.FoundingDate]               = orgWebSite.SchemaOrgOrganization.FoundingDate;
                metadata.Properties[WebVocabulary.Website.Duns]            = orgWebSite.SchemaOrgOrganization.Duns;
                metadata.Properties[WebVocabulary.Website.GlobalLocationNumber]  = orgWebSite.SchemaOrgOrganization.GlobalLocationNumber;
                metadata.Properties[WebVocabulary.Website.IsicV4]                = orgWebSite.SchemaOrgOrganization.IsicV4;
                metadata.Properties[WebVocabulary.Website.LeiCode]               = orgWebSite.SchemaOrgOrganization.LeiCode;
                metadata.Properties[WebVocabulary.Website.Naics]                 = orgWebSite.SchemaOrgOrganization.Naics;
                metadata.Properties[WebVocabulary.Website.TaxId]                      = orgWebSite.SchemaOrgOrganization.TaxId;
                metadata.Properties[WebVocabulary.Website.TaxId]                  = orgWebSite.SchemaOrgOrganization.VatId;
                metadata.Properties[WebVocabulary.Website.TickerSymbol]               = (orgWebSite.SchemaOrgOrganization as SchemaOrgCorporation).PrintIfAvailable(v => v.TickerSymbol);

                //orgWebSite.SchemaOrgOrganization.Founders
            }

            foreach (var kp in orgWebSite.SocialLinks)
            {
                if (kp.Key == SocialUriType.Unknown)
                    continue;

                var uri = kp.Value;

                var key = UriUtility.GetCluedInOrganizationSocialVocabularyKey(kp.Key);

                if (key != null)
                    metadata.Properties[key] = uri.PrintIfAvailable();
                else
                    context.Log.LogError("Could not map social link type to organization vocabulary {@UriType}",new { SocialUriType = kp.Key, Uri = uri });
            }

            if (orgWebSite.Codes != null)
            {
                foreach (var c in orgWebSite.Codes)
                {
                    switch (c.Key)
                    {
                        case "cvr":
                            metadata.Properties[WebVocabulary.Website.CVR] = c.Value;
                            break;

                        case "googleAnalytics":
                            metadata.Properties[WebVocabulary.Website.GoogleAnalytics] = c.Value;
                            break;

                        case "swift":
                            // TODO
                            break;

                        default:
                            context.Log.LogDebug("Unknown code type: {Key}", c.Key);
                            break;
                    }
                }
            }
        }

        public IEnumerable<EntityType> Accepts(IDictionary<string, object> config, IProvider provider)
        {
            return AcceptedEntityTypes;
        }

        public IEnumerable<IExternalSearchQuery> BuildQueries(ExecutionContext context, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return InternalBuildQueries(context, request, config);
        }

        public IEnumerable<IExternalSearchQueryResult> ExecuteSearch(ExecutionContext context, IExternalSearchQuery query, IDictionary<string, object> config, IProvider provider)
        {
            var jobData = new WebExternalSearchJobData(config);

            foreach (var externalSearchQueryResult in ExecuteSearch(context, query)) yield return externalSearchQueryResult;
        }

        public IEnumerable<Clue> BuildClues(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return BuildClues(context, query, result, request);
        }

        public IEntityMetadata GetPrimaryEntityMetadata(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return GetPrimaryEntityMetadata(context, result, request);
        }

        public IPreviewImage GetPrimaryEntityPreviewImage(ExecutionContext context, IExternalSearchQueryResult result, IExternalSearchRequest request, IDictionary<string, object> config, IProvider provider)
        {
            return GetPrimaryEntityPreviewImage(context, result, request);
        }

        public string Icon { get; } = WebExternalSearchConstants.Icon;
        public string Domain { get; } = WebExternalSearchConstants.Domain;
        public string About { get; } = WebExternalSearchConstants.About;
        public AuthMethods AuthMethods { get; } = WebExternalSearchConstants.AuthMethods;
        public IEnumerable<Control> Properties { get; } = WebExternalSearchConstants.Properties;
        public Guide Guide { get; } = WebExternalSearchConstants.Guide;
        public IntegrationType Type { get; } = WebExternalSearchConstants.IntegrationType;
    }
}
