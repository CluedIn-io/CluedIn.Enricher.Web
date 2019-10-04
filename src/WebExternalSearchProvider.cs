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
using CluedIn.Crawling.Helpers;
using CluedIn.ExternalSearch.Providers.Web.Model;
using CluedIn.ExternalSearch.Providers.Web.Vocabularies;
using CluedIn.Processing.Web.Models;

using Newtonsoft.Json;

using RestSharp;

using CluedInVocabularies = CluedIn.Core.Data.Vocabularies.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.Web
{
    /// <summary>The web external search provider.</summary>
    /// <seealso cref="CluedIn.ExternalSearch.ExternalSearchProviderBase" />
    public partial class WebExternalSearchProvider : ExternalSearchProviderBase, IExternalSearchResultLogger
    {
        /**********************************************************************************************************
         * CONSTRUCTORS
         **********************************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="WebExternalSearchProvider" /> class.
        /// </summary>
        public WebExternalSearchProvider()
            : base(ExternalSearchProviderPriority.First, Constants.ExternalSearchProviders.WebId, EntityType.Organization)
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
            if (!this.Accepts(request.EntityMetaData.EntityType))
                yield break;

            var existingResults = request.GetQueryResults<WebResult>(this).ToList();

            Func<string, bool> requestUriFilter = value => value == null
                                                        || existingResults.Any(r => string.Equals(r.Data.RequestUri.ToString(), value, StringComparison.InvariantCultureIgnoreCase)
                                                        || UriUtility.IsSocialProfileUri(value));

            // Query Input
            var entityType      = request.EntityMetaData.EntityType;
            var website         = request.QueryParameters.GetValue(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website, null);

            if (website != null)
            {
                var values = website.SelectMany(v => v.Split(new[] { ",", ";", "|", " " }, StringSplitOptions.RemoveEmptyEntries)).ToHashSet();

                values = values.Select(UriUtility.NormalizeHttpUri).ToHashSet();

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
                context.Log.Debug(() => "HTTP Response:" + response.StatusCode);
                context.Log.Debug(() => response.ErrorMessage);
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

            var code = this.GetOriginEntityCode(resultItem);

            var clue = new Clue(code, context.Organization);
            clue.Data.OriginProviderDefinitionId = this.Id;

            this.PopulateMetadata(context, clue.Data.EntityData, resultItem, request);

            if (clue.Data.EntityData.Properties.ContainsKey(WebVocabulary.Website.Logo))
                this.DownloadPreviewImage(context, clue.Data.EntityData.Properties[WebVocabulary.Website.Logo], clue);

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
        private EntityCode GetOriginEntityCode(IExternalSearchQueryResult<WebResult> resultItem)
        {
            return new EntityCode(EntityType.Organization, this.GetCodeOrigin(), resultItem.Data.RestResponse.ResponseUri.ToString().ToLowerInvariant());
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
            var code = this.GetOriginEntityCode(resultItem);

            var orgWebSite  = resultItem.Data.GetOrganizationWebsiteMetadata(context);

            var name = orgWebSite.Name;

            metadata.EntityType             = EntityType.Organization;
            metadata.Name                   = name;
            metadata.DisplayName            = orgWebSite.SchemaOrgOrganization != null ? orgWebSite.SchemaOrgOrganization.LegalName ?? orgWebSite.SchemaOrgOrganization.AlternateName : name;
            metadata.OriginEntityCode       = code;
            metadata.Uri                    = orgWebSite.RequestUri;
            metadata.Description            = orgWebSite.WebsiteDescription;

            metadata.Codes.Add(code);
            metadata.Codes.Add(new EntityCode(EntityType.Organization, this.GetCodeOrigin(), resultItem.Data.RestResponse.ResponseUri.Host.ToLowerInvariant()));
            metadata.Codes.Add(new EntityCode(EntityType.Web.Site, CodeOrigin.CluedIn, orgWebSite.ResponseUri.ToString().ToLowerInvariant())); // Force result to match back to original query

            if (request.EntityMetaData != null && request.EntityMetaData.OriginEntityCode != null)
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

            metadata.Properties[CluedInVocabularies.CluedInOrganization.Website]    = resultItem.Data.RequestUri.PrintIfAvailable();

            metadata.Properties[WebVocabulary.Website.Description]                  = orgWebSite.WebsiteDescription;
            metadata.Properties[WebVocabulary.Website.Title]                        = orgWebSite.WebsiteTitle;
            metadata.Properties[WebVocabulary.Website.Logo]                         = orgWebSite.Logo.PrintIfAvailable();
            metadata.Properties[WebVocabulary.Website.CopyrightEntity]              = orgWebSite.CopyrightEntity;

            metadata.Properties[CluedInVocabularies.CluedInOrganization.OrganizationName]   = orgWebSite.Name;
            metadata.Properties[CluedInVocabularies.CluedInOrganization.PhoneNumber]        = orgWebSite.PhoneNumber;
            metadata.Properties[CluedInVocabularies.CluedInOrganization.Fax]                = orgWebSite.FaxNumber;
            metadata.Properties[CluedInVocabularies.CluedInOrganization.ContactEmail]       = orgWebSite.ContactEmail;
            metadata.Properties[CluedInVocabularies.CluedInOrganization.Address]            = orgWebSite.Address;
            metadata.Properties[CluedInVocabularies.CluedInOrganization.AddressCountryCode] = orgWebSite.Country;
            metadata.Properties[CluedInVocabularies.CluedInOrganization.UsedTechnologies]   = technologiesListText;

            if (orgWebSite.SchemaOrgOrganization != null)
            {
                var postalAddress = orgWebSite.SchemaOrgOrganization.Address as SchemaOrgPostalAddress;
                if (postalAddress != null)
                {
                    metadata.Properties[CluedInVocabularies.CluedInOrganization.AddressCountryCode]     = (postalAddress.AddressCountry as SchemaOrgCountry).PrintIfAvailable(v => v.Name) ?? postalAddress.AddressCountry.PrintIfAvailable();
                    metadata.Properties[CluedInVocabularies.CluedInOrganization.AddressZipCode]         = postalAddress.PostalCode;
                    metadata.Properties[CluedInVocabularies.CluedInOrganization.Address]                = postalAddress.StreetAddress;
                }
                else if (orgWebSite.SchemaOrgOrganization.Address is SchemaOrgText)
                {
                    metadata.Properties[CluedInVocabularies.CluedInOrganization.Address]                = orgWebSite.SchemaOrgOrganization.Address.PrintIfAvailable();
                }

                metadata.Properties[CluedInVocabularies.CluedInOrganization.FoundingDate]               = orgWebSite.SchemaOrgOrganization.FoundingDate;
                metadata.Properties[CluedInVocabularies.CluedInOrganization.CodesDunsNumber]            = orgWebSite.SchemaOrgOrganization.Duns;
                metadata.Properties[CluedInVocabularies.CluedInOrganization.CodesGlobalLocationNumber]  = orgWebSite.SchemaOrgOrganization.GlobalLocationNumber;
                metadata.Properties[CluedInVocabularies.CluedInOrganization.CodesIsicV4]                = orgWebSite.SchemaOrgOrganization.IsicV4;
                metadata.Properties[CluedInVocabularies.CluedInOrganization.CodesLeiCode]               = orgWebSite.SchemaOrgOrganization.LeiCode;
                metadata.Properties[CluedInVocabularies.CluedInOrganization.CodesNAICS]                 = orgWebSite.SchemaOrgOrganization.Naics;
                metadata.Properties[CluedInVocabularies.CluedInOrganization.TaxId]                      = orgWebSite.SchemaOrgOrganization.TaxId;
                metadata.Properties[CluedInVocabularies.CluedInOrganization.VatNumber]                  = orgWebSite.SchemaOrgOrganization.VatId;
                metadata.Properties[CluedInVocabularies.CluedInOrganization.TickerSymbol]               = (orgWebSite.SchemaOrgOrganization as SchemaOrgCorporation).PrintIfAvailable(v => v.TickerSymbol);

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
                    context.Log.Error(new { SocialUriType = kp.Key, Uri = uri }, () => "Could not map social link type to organization vocabulary");
            }

            if (orgWebSite.Codes != null)
            {
                foreach (var c in orgWebSite.Codes)
                {
                    switch (c.Key)
                    {
                        case "cvr":
                            metadata.Properties[CluedInVocabularies.CluedInOrganization.CodesCVR] = c.Value;
                            break;

                        case "googleAnalytics":
                            metadata.Properties[CluedInVocabularies.CluedInOrganization.CodesGoogleAnalytics] = c.Value;
                            break;

                        case "swift":
                            // TODO
                            break;

                        default:
                            context.Log.Debug(() => "Unknown code type: " + c.Key);
                            break;
                    }
                }
            }
        }
    }
}
