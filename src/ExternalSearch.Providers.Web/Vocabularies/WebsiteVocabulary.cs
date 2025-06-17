// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebsiteVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the WebsiteVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using CluedIn.Core.Data;
using CluedIn.Core.Data.Vocabularies;

namespace CluedIn.ExternalSearch.Providers.Web.Vocabularies
{
    /// <summary>The website vocabulary.</summary>
    /// <seealso cref="CluedIn.Core.Data.Vocabularies.SimpleVocabulary" />
    public class WebsiteVocabulary : SimpleVocabulary
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebsiteVocabulary"/> class.
        /// </summary>
        public WebsiteVocabulary()
        {
            this.VocabularyName = "Website";
            this.KeyPrefix      = "website";
            this.KeySeparator   = ".";
            this.Grouping       = EntityType.Web.Site;

            this.Description                = this.Add(new VocabularyKey("Description"));
            this.Title                      = this.Add(new VocabularyKey("Title"));
            this.Logo                       = this.Add(new VocabularyKey("Logo", VocabularyKeyVisibility.Hidden));
            this.CopyrightEntity            = this.Add(new VocabularyKey("CopyrightEntity"));
            this.WebsiteDescription         = this.Add(new VocabularyKey("WebsiteDescription"));
            this.Name                       = this.Add(new VocabularyKey("Name"));
            this.URI                        = this.Add(new VocabularyKey("URI"));
            this.PhoneNumber                = this.Add(new VocabularyKey("PhoneNumber"));
            this.FaxNumber                  = this.Add(new VocabularyKey("FaxNumber"));
            this.ContactEmail               = this.Add(new VocabularyKey("ContactEmail"));
            this.Address                    = this.Add(new VocabularyKey("Address"));
            this.Country                    = this.Add(new VocabularyKey("Country"));
            this.TechnologiesListText       = this.Add(new VocabularyKey("TechnologiesListText"));
            this.AddressCountry             = this.Add(new VocabularyKey("AddressCountry"));
            this.PostalCode                 = this.Add(new VocabularyKey("PostalCode"));
            this.StreetAddress              = this.Add(new VocabularyKey("StreetAddress"));
            this.FoundingDate               = this.Add(new VocabularyKey("FoundingDate"));
            this.Duns                       = this.Add(new VocabularyKey("Duns"));
            this.GlobalLocationNumber       = this.Add(new VocabularyKey("GlobalLocationNumber"));
            this.IsicV4                     = this.Add(new VocabularyKey("IsicV4"));
            this.LeiCode                    = this.Add(new VocabularyKey("LeiCode"));
            this.Naics                      = this.Add(new VocabularyKey("Naics"));
            this.TaxId                      = this.Add(new VocabularyKey("TaxId"));
            this.VatId                      = this.Add(new VocabularyKey("VatId"));
            this.TickerSymbol               = this.Add(new VocabularyKey("TickerSymbol"));
            this.CVR                        = this.Add(new VocabularyKey("CVR"));
            this.GoogleAnalytics            = this.Add(new VocabularyKey("GoogleAnalytics"));
        }

        public VocabularyKey Description { get; protected set; }
        public VocabularyKey Title { get; protected set; }
        public VocabularyKey Logo { get; protected set; }
        public VocabularyKey CopyrightEntity { get; protected set; }
        public VocabularyKey WebsiteDescription { get; protected set; }
        public VocabularyKey Name { get; protected set; }
        public VocabularyKey URI { get; protected set; }
        public VocabularyKey PhoneNumber { get; protected set; }
        public VocabularyKey FaxNumber { get; protected set; }
        public VocabularyKey ContactEmail { get; protected set; }
        public VocabularyKey Address { get; protected set; }
        public VocabularyKey Country { get; protected set; }
        public VocabularyKey TechnologiesListText { get; protected set; }
        public VocabularyKey AddressCountry { get; protected set; }
        public VocabularyKey PostalCode { get; protected set; }
        public VocabularyKey StreetAddress { get; protected set; }
        public VocabularyKey FoundingDate { get; protected set; }
        public VocabularyKey Duns { get; protected set; }
        public VocabularyKey GlobalLocationNumber { get; protected set; }
        public VocabularyKey IsicV4 { get; protected set; }
        public VocabularyKey LeiCode { get; protected set; }
        public VocabularyKey Naics { get; protected set; }
        public VocabularyKey TaxId { get; protected set; }
        public VocabularyKey VatId { get; protected set; }
        public VocabularyKey TickerSymbol { get; protected set; }
        public VocabularyKey CVR { get; protected set; }
        public VocabularyKey GoogleAnalytics { get; protected set; }
    }
}
