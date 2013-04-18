//using Inedo.BuildMaster.Extensibility.Providers;
//using Inedo.BuildMaster.Web;
//using Microsoft.TeamFoundation.Client;

//namespace Inedo.BuildMasterExtensions.TFS2012
//{
//    [ProviderProperties(
//        "VisualStudio.com",
//        "Hosted TFS on visualstudio.com; requires that Visual Studio Team System (or greater) 2012 is installed.")]
//    [CustomEditor(typeof(VisualStudioComIssueTrackingProviderEditor))]
//    public sealed class VisualStudioComIssueTrackingProvider : Tfs2012IssueTrackingProvider
//    {
//        /// <summary>
//        /// Initializes a new instance of the VisualStudioComIssueTrackingProvider class.
//        /// </summary>
//        public VisualStudioComIssueTrackingProvider()
//        {
//        }

//        protected override Microsoft.TeamFoundation.Client.TfsTeamProjectCollection GetTeamProjectCollection()
//        {
//            var tfs = new TfsTeamProjectCollection(this.BaseUri, new TfsClientCredentials(new SimpleWebTokenCredential(this.UserName, this.Password)));
//            tfs.EnsureAuthenticated();
//            return tfs;
//        }
//    }
//}
