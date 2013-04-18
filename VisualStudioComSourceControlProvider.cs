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
        }

        protected override TfsTeamProjectCollection GetTeamProjectCollection()
        {
            var tfs = new TfsTeamProjectCollection(this.BaseUri, new TfsClientCredentials(new SimpleWebTokenCredential(this.UserName, this.Password)));
            tfs.EnsureAuthenticated();
            return tfs;
        }
    }
}
