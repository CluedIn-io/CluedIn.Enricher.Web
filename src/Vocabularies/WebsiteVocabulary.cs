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
        }

        public VocabularyKey Description { get; protected set; }
        public VocabularyKey Title { get; protected set; }
        public VocabularyKey Logo { get; protected set; }
        public VocabularyKey CopyrightEntity { get; protected set; }
    }
}
