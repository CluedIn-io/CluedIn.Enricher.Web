// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebTests.cs" company="Clued In">
//   Copyright (c) 2019 Clued In. All rights reserved.
// </copyright>
// <summary>
//   Implements the web tests class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Parts;
using CluedIn.Core.Messages.Processing;
using CluedIn.ExternalSearch.Providers.Web;
using CluedIn.Testing.Base.ExternalSearch;
using Moq;
using Xunit;

namespace ExternalSearch.Web.Integration.Tests
{
    public class WebTests : BaseExternalSearchTest<WebExternalSearchProvider>
    {
        [Fact(Skip = "Failed Mock exception. GitHub Issue 829 - ref https://github.com/CluedIn-io/CluedIn/issues/829")]
        public void Test()
        {
            var properties = new EntityMetadataPart();
            properties.Properties.Add(CluedIn.Core.Data.Vocabularies.Vocabularies.CluedInOrganization.Website, "www.sitecore.com");
            IEntityMetadata entityMetadata = new EntityMetadataPart()
            {
                Name = "Sitecore",
                EntityType = EntityType.Organization,
                Properties = properties.Properties
            };

            this.Setup(null, entityMetadata);

            // Assert
            this.testContext.ProcessingHub.Verify(h => h.SendCommand(It.IsAny<ProcessClueCommand>()), Times.AtLeastOnce);

            Assert.True(this.clues.Count > 0);
        }
    }
}
