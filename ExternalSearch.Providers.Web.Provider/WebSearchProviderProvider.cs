﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CluedIn.Core;
using CluedIn.Core.Crawling;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.ExternalSearch;
using CluedIn.Core.Providers;
using CluedIn.Core.Webhooks;
using CluedIn.ExternalSearch;
using CluedIn.Providers.Models;

namespace CluedIn.ExternalSearch.Providers.Web.Provider
{
    public class WebSearchProviderProvider : ProviderBase, IExtendedProviderMetadata, IExternalSearchProviderProvider
    {
        public IExternalSearchProvider ExternalSearchProvider { get; }

        public WebSearchProviderProvider([System.Diagnostics.CodeAnalysis.NotNull] ApplicationContext appContext) : base(appContext, GetMetaData())
        {
            ExternalSearchProvider = appContext.Container.ResolveAll<IExternalSearchProvider>().Single(n => n.Id == WebExternalSearchConstants.ProviderId);
        }

        private static IProviderMetadata GetMetaData()
        {
            return new ProviderMetadata
            {
                Id = WebExternalSearchConstants.ProviderId,
                Name = WebExternalSearchConstants.ProviderName,
                ComponentName = WebExternalSearchConstants.ComponentName,
                AuthTypes = new List<string>(),
                SupportsConfiguration = true,
                SupportsAutomaticWebhookCreation = false,
                SupportsWebHooks = false,
                Type = "Enricher"
            };
        }

        public override async Task<CrawlJobData> GetCrawlJobData(ProviderUpdateContext context, IDictionary<string, object> configuration, Guid organizationId, Guid userId, Guid providerDefinitionId)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            var result = new WebExternalSearchJobData(configuration);

            return await Task.FromResult(result);
        }

        public override Task<bool> TestAuthentication(ProviderUpdateContext context, IDictionary<string, object> configuration, Guid organizationId, Guid userId, Guid providerDefinitionId)
        {
            return Task.FromResult(true);
        }

        public override Task<ExpectedStatistics> FetchUnSyncedEntityStatistics(ExecutionContext context, IDictionary<string, object> configuration, Guid organizationId, Guid userId, Guid providerDefinitionId)
        {
            throw new NotImplementedException();
        }

        public override async Task<IDictionary<string, object>> GetHelperConfiguration(ProviderUpdateContext context, CrawlJobData jobData, Guid organizationId, Guid userId, Guid providerDefinitionId)
        {
            if (jobData is WebExternalSearchJobData result)
            {
                return await Task.FromResult(result.ToDictionary());
            }

            throw new InvalidOperationException($"Unexpected data type for AcceptanceExternalSearchJobData, {jobData.GetType()}");
        }

        public override Task<IDictionary<string, object>> GetHelperConfiguration(ProviderUpdateContext context, CrawlJobData jobData, Guid organizationId, Guid userId, Guid providerDefinitionId, string folderId)
        {
            return GetHelperConfiguration(context, jobData, organizationId, userId, providerDefinitionId);
        }

        public override Task<AccountInformation> GetAccountInformation(ExecutionContext context, CrawlJobData jobData, Guid organizationId, Guid userId, Guid providerDefinitionId)
        {
            return Task.FromResult(new AccountInformation(providerDefinitionId.ToString(), providerDefinitionId.ToString()));
        }

        public override string Schedule(DateTimeOffset relativeDateTime, bool webHooksEnabled)
        {
            return $"{relativeDateTime.Minute} 0/23 * * *";
        }

        public override Task<IEnumerable<WebHookSignature>> CreateWebHook(ExecutionContext context, CrawlJobData jobData, IWebhookDefinition webhookDefinition, IDictionary<string, object> config)
        {
            throw new NotImplementedException();
        }

        public override Task<IEnumerable<WebhookDefinition>> GetWebHooks(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteWebHook(ExecutionContext context, CrawlJobData jobData, IWebhookDefinition webhookDefinition)
        {
            throw new NotImplementedException();
        }

        public override Task<CrawlLimit> GetRemainingApiAllowance(ExecutionContext context, CrawlJobData jobData, Guid organizationId, Guid userId, Guid providerDefinitionId)
        {
            if (jobData == null) throw new ArgumentNullException(nameof(jobData));
            return Task.FromResult(new CrawlLimit(-1, TimeSpan.Zero));
        }

        public override IEnumerable<string> WebhookManagementEndpoints(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        public override bool ScheduleCrawlJobs => false;
        public string Icon { get; } = WebExternalSearchConstants.Icon;
        public string Domain { get; } = WebExternalSearchConstants.Domain;
        public string About { get; } = WebExternalSearchConstants.About;
        public AuthMethods AuthMethods { get; } = WebExternalSearchConstants.AuthMethods;
        public IEnumerable<Control> Properties { get; } = WebExternalSearchConstants.Properties;
        public Guide Guide { get; } = WebExternalSearchConstants.Guide;
        public new IntegrationType Type { get; } = WebExternalSearchConstants.IntegrationType;
    }
}
