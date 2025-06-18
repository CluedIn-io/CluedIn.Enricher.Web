// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebVocabulary.cs" company="Clued In">
//   Copyright Clued In
// </copyright>
// <summary>
//   Defines the WebVocabulary type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace CluedIn.ExternalSearch.Providers.Web.Vocabularies
{
    /// <summary>The web vocabulary.</summary>
    public static class WebVocabulary
    {
        /// <summary>
        /// Initializes static members of the <see cref="WebVocabulary" /> class.
        /// </summary>
        static WebVocabulary()
        {
            Website = new WebsiteVocabulary();
        }

        /// <summary>Gets the website.</summary>
        /// <value>The website.</value>
        public static WebsiteVocabulary Website { get; private set; }
    }
}