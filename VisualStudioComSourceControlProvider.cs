using System;
using System.Net;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web;
using Microsoft.TeamFoundation.Client;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    [ProviderProperties(
        "VisualStudio.com",
        "Hosted TFS on visualstudio.com; requires that Visual Studio Team System (or greater) 2012 is installed.")]
    [CustomEditor(typeof(VisualStudioComSourceControlProviderEditor))]
    public sealed class VisualStudioComSourceControlProvider : TfsSourceControlProvider
    {
        /// <summary>
        /// Initializes a new instance of the VisualStudioComSourceControlProvider class.
        /// </summary>
        public VisualStudioComSourceControlProvider()
        {
            this.UseBasicAuthentication = true;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use basic authentication or web token authentication.
        /// </summary>
        [Persistent]
        public bool UseBasicAuthentication { get; set; }

        protected override TfsTeamProjectCollection GetTeamProjectCollection()
        {
            try
            {
                var tfs = new TfsTeamProjectCollection(
                    this.BaseUri,
                    new TfsClientCredentials(
                        this.UseBasicAuthentication ? this.CreateCredentials() : new SimpleWebTokenCredential(this.UserName, this.Password)
                    )
                );
                tfs.EnsureAuthenticated();
                return tfs;
            }
            catch (TypeLoadException)
            {
                throw new NotAvailableException("Basic authentication requires Visual Studio 2012 Update 1 or newer.");
            }
        }

        private FederatedCredential CreateCredentials()
        {
            return new BasicAuthCredential(new NetworkCredential(this.UserName, this.Password));
        }
    }
}
