using System.Net;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Microsoft.TeamFoundation.Client;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    public abstract class TfsActionBase : AgentBasedActionBase
    {
        /// <summary>
        /// Gets or sets the team project.
        /// </summary>
        [Persistent]
        public string TeamProject { get; set; }

        /// <summary>
        /// Gets or sets the name of the build definition if not empty, or includes all build definitions in the search.
        /// </summary>
        [Persistent]
        public string BuildDefinition { get; set; }

        /// <summary>
        /// Gets the extension configurer.
        /// </summary>
        public new TfsConfigurer GetExtensionConfigurer() 
        {
            return (TfsConfigurer)base.GetExtensionConfigurer();
        }

        /// <summary>
        /// Gets the appropriate version control server based by connecting to TFS using the persisted credentials
        /// </summary>
        protected TfsTeamProjectCollection GetTeamProjectCollection()
        {
            return GetTeamProjectCollection(this.GetExtensionConfigurer());
        }

        internal static TfsTeamProjectCollection GetTeamProjectCollection(TfsConfigurer configurer)
        {
            if (configurer.UseSystemCredentials)
            {
                var projectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(configurer.BaseUri);
                projectCollection.EnsureAuthenticated();
                return projectCollection;
            }
            else
            {
                var projectCollection = new TfsTeamProjectCollection(configurer.BaseUri, new TfsClientCredentials(new WindowsCredential(new NetworkCredential(configurer.UserName, configurer.Password, configurer.Domain))));
                projectCollection.EnsureAuthenticated();
                return projectCollection;
            }
        }
    }
}
