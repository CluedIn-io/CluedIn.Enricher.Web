using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CluedIn.Core;
using CluedIn.Core.Data.Relational;
using CluedIn.Crawling.Helpers;
using CluedIn.ExternalSearch.DataStore;
using CluedIn.ExternalSearch.Providers.Web.Model;

using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.Web
{
    public partial class WebExternalSearchProvider
    {
        /// <summary>The json serializer</summary>
        private readonly JsonSerializer jsonSerializer;

        public void LogResult(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result)
        {
            using (var systemContext = context.ApplicationContext.System.CreateExecutionContext())
            {
                var dataStore   = systemContext.Organization.DataStores.GetDataStore<ExternalSearchWebLogRecord>();
                var resultItem  = result.As<WebResult>();
                var record      = this.CreateRecord(context, query, result, resultItem.Data);

                dataStore.InsertOrUpdate(systemContext, record);
            }
        }

        private ExternalSearchWebLogRecord CreateRecord(ExecutionContext context, IExternalSearchQuery query, IExternalSearchQueryResult result, WebResult resultItem)
        {
            var orgData = resultItem.GetOrganizationWebsiteMetadata(context);

            var record = new ExternalSearchWebLogRecord();
            record.Id               = ExternalSearchLogIdGenerator.GenerateId(query.ProviderId, query.EntityType, orgData.RequestUri, orgData.ResponseUri);
            record.ProviderId       = query.ProviderId;
            record.EntityType       = query.EntityType;

            record.RequestUri                       = orgData.RequestUri.PrintIfAvailable();
            record.ResponseUri                      = orgData.ResponseUri.PrintIfAvailable();
            record.Name                             = orgData.Name;
            record.WebsiteTitle                     = orgData.WebsiteTitle;
            record.WebsiteDescription               = orgData.WebsiteDescription;
            record.WebsiteKeywords                  = orgData.WebsiteKeywords.PrintIfAvailable(v => string.Join("; ", v));
            record.PhoneNumber                      = orgData.PhoneNumber;
            record.FaxNumber                        = orgData.FaxNumber;
            record.Country                          = orgData.Country;
            record.Address                          = orgData.Address;
            record.ContactEmail                     = orgData.ContactEmail;
            record.CopyrightEntity                  = orgData.CopyrightEntity;
            record.Logo                             = orgData.Logo.PrintIfAvailable();
            record.SchemaOrgOrganization            = orgData.SchemaOrgOrganization.PrintIfAvailable(v => JsonUtility.Serialize(v, this.jsonSerializer));
            record.SocialLinks                      = orgData.SocialLinks.PrintIfAvailable(v => string.Join("; ", v.Select(t => t.Key + ":" + t.Value)));
            record.Technologies                     = orgData.Technologies.PrintIfAvailable(v => string.Join("; ", v.Select(t => t.Name)));
            record.Codes                            = orgData.Codes.PrintIfAvailable(v => string.Join("; ", v.Select(t => t.Key + ":" + t.Value)));

            return record;
        }
    }
}
