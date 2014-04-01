using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Extensibility.Providers.SourceControl;
using Inedo.BuildMaster.Files;
using Inedo.BuildMaster.Web;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    [ProviderProperties(
        "Team Foundation Server (2012)",
        "Supports TFS 2012 and earlier; requires that Visual Studio Team System (or greater) 2012 is installed.",
        RequiresTransparentProxy = true)]
    [CustomEditor(typeof(TfsSourceControlProviderEditor))]
    public class TfsSourceControlProvider : SourceControlProviderBase, ILabelingProvider, IRevisionProvider
    {
        private const string EmptyPathString = "$/";

        /// <summary>
        /// The base url of the TFS store, should not include collection name, e.g. "http://server:port/tfs"
        /// </summary>
        [Persistent]
        public string BaseUrl { get; set; }
        /// <summary>
        /// The username used to connect to the server
        /// </summary>
        [Persistent]
        public string UserName { get; set; }
        /// <summary>
        /// The password used to connect to the server
        /// </summary>
        [Persistent]
        public string Password { get; set; }
        /// <summary>
        /// The domain of the server
        /// </summary>
        [Persistent]
        public string Domain { get; set; }
        /// <summary>
        /// Returns true if BuildMaster should connect to TFS using its own account, false if the credentials are specified
        /// </summary>
        [Persistent]
        public bool UseSystemCredentials { get; set; }

        /// <summary>
        /// Gets the base URI of the Team Foundation Server
        /// </summary>
        protected Uri BaseUri
        {
            get { return new Uri(BaseUrl); }
        }

        /// <summary>
        /// Gets the char that's used by the provider to separate directories/files in a path string
        /// </summary>
        public override char DirectorySeparator
        {
            get { return '/'; }
        }
        /// <summary>
        /// Retrieves the latest version of the source code from the provider's sourcePath into the target path
        /// </summary>
        /// <param name="sourcePath">provider source path</param>
        /// <param name="targetPath">target file path</param>
        public override void GetLatest(string sourcePath, string targetPath)
        {
            if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException("sourcePath");
            if (string.IsNullOrEmpty(targetPath)) throw new ArgumentNullException("targetPath");
            if (!Directory.Exists(targetPath)) throw new DirectoryNotFoundException("targetPath not found: " + targetPath);

            sourcePath = BuildSourcePath(sourcePath);
            using (var tfs = this.GetTeamProjectCollection())
            {
                var versionControlServer = tfs.GetService<VersionControlServer>();

                // create workspace in the target directory
                Workspace workspace = null;
                try
                {
                    workspace = GetMappedWorkspace(versionControlServer, sourcePath, targetPath);
                    workspace.Get(VersionSpec.Latest, GetOptions.GetAll | GetOptions.Overwrite);
                }
                finally
                {
                    try { workspace.Delete(); }
                    catch { }
                }
            }
        }
        /// <summary>
        /// Returns a string representation of this provider.
        /// </summary>
        /// <returns>String representation of this provider.</returns>
        public override string ToString()
        {
            return "Provides functionality for getting files and browsing folders in TFS 2012 and earlier.";
        }
        /// <summary>
        /// Returns a loaded <see cref="DirectoryEntryInfo"/> object from the sourcePath
        /// </summary>
        /// <param name="sourcePath">provider source path</param>
        /// <param name="recurse">indicates whether to recurse</param>
        /// <returns>
        /// loaded <see cref="DirectoryEntryInfo"/> object
        /// </returns>
        public override DirectoryEntryInfo GetDirectoryEntryInfo(string sourcePath)
        {
            sourcePath = BuildSourcePath(sourcePath);

            using (var tfs = this.GetTeamProjectCollection())
            {
                // validate/clean sourcePath (should be $/SomeDir/SomePathNoTrailingSlash)
                var sourceControl = tfs.GetService<VersionControlServer>();

                sourcePath = sourceControl.GetItem(sourcePath).ServerItem; // matches the sourcePath with the base path returned by TFS

                // working lists
                var subDirs = new List<DirectoryEntryInfo>();
                var files = new List<FileEntryInfo>();

                // get the items
                ItemSet items = sourceControl.GetItems(sourcePath, RecursionType.OneLevel);
                foreach (Item item in items.Items)
                {
                    // don't add self to subdirectories
                    if (item.ServerItem == sourcePath) continue;

                    // files and directories do not have trailing slashes
                    string itemName = item.ServerItem.Substring(item.ServerItem.LastIndexOf("/") + 1);

                    switch (item.ItemType)
                    {
                        case ItemType.Any:
                            throw new ArgumentOutOfRangeException("ItemType returned was Any; expected File or Folder.");

                        case ItemType.File:
                            files.Add(new ExtendedFileEntryInfo(
                                itemName,
                                item.ServerItem,
                                item.ContentLength,
                                item.CheckinDate,
                                FileAttributes.Normal));
                            break;

                        case ItemType.Folder:
                            subDirs.Add(new DirectoryEntryInfo(
                                itemName,
                                item.ServerItem,
                                null,
                                null));
                            break;
                    }
                }

                if (sourcePath == EmptyPathString)
                    return new DirectoryEntryInfo(string.Empty, string.Empty, subDirs.ToArray(), files.ToArray());
                else
                    return new DirectoryEntryInfo(
                        sourcePath.Substring(sourcePath.LastIndexOf("/") + 1),
                        "",
                        subDirs.ToArray(),
                        files.ToArray());
            }
        }
        /// <summary>
        /// Returns the contents of the specified file
        /// </summary>
        /// <param name="filePath">provider file path</param>
        /// <returns>
        /// loaded <see cref="DirectoryEntryInfo"/> object
        /// </returns>
        public override byte[] GetFileContents(string filePath)
        {
            // validate arguments
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");

            // handle root path
            filePath = BuildSourcePath(filePath);

            // create temp file which we can overwrite with downloaded file in TFS source control
            var tempFile = Path.GetTempFileName();
            using (var tfs = this.GetTeamProjectCollection())
            {
                var versionControlServer = tfs.GetService<VersionControlServer>();
                var item = versionControlServer.GetItem(filePath);
                item.DownloadFile(tempFile);

                return File.ReadAllBytes(tempFile);
            }
        }
        /// <summary>
        /// Indicates whether the provider is installed and available for use in the current execution context
        /// </summary>
        /// <returns></returns>
        public override bool IsAvailable()
        {
            return IsAvailable2();
        }
        /// <summary>
        /// Attempts to connect with the current configuration and, if not successful, throws a <see cref="NotAvailableException"/>
        /// </summary>
        public override void ValidateConnection()
        {
            try
            {
                this.ValidateConnection2();
            }
            catch (TypeLoadException)
            {
                throw new NotAvailableException("Could not connect to TFS. Verify that Visual Studio 2012 or Team Explorer 2012 is installed on the server.");
            }
            catch (Exception ex)
            {
                throw new NotAvailableException("Could not connect to TFS: " + ex.ToString());
            }
        }
        /// <summary>
        /// Applies the specified label to the specified source path
        /// </summary>
        /// <param name="label">label to apply</param>
        /// <param name="sourcePath">path to apply label to</param>
        public void ApplyLabel(string label, string sourcePath)
        {
            // verify sourcePath
            if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException("sourcePath");
            sourcePath = BuildSourcePath(sourcePath);

            using (var tfs = this.GetTeamProjectCollection())
            {
                var versionControlService = tfs.GetService<VersionControlServer>();

                var versionControlLabel = new VersionControlLabel(versionControlService, label, versionControlService.AuthenticatedUser, sourcePath, "Label applied by BuildMaster");
                versionControlService.CreateLabel(versionControlLabel, new[] { new LabelItemSpec(new ItemSpec(sourcePath, RecursionType.Full), VersionSpec.Latest, false) }, LabelChildOption.Replace);
            }
        }
        /// <summary>
        /// Retrieves labeled the source code from the provider's sourcePath into the target path
        /// </summary>
        /// <param name="label"></param>
        /// <param name="sourcePath">provider source path</param>
        /// <param name="targetPath">target file path</param>
        public void GetLabeled(string label, string sourcePath, string targetPath)
        {
            if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException("sourcePath");
            if (string.IsNullOrEmpty(targetPath)) throw new ArgumentNullException("targetPath");
            if (!Directory.Exists(targetPath)) throw new DirectoryNotFoundException("targetPath not found: " + targetPath);

            sourcePath = BuildSourcePath(sourcePath);
            using (var tfs = this.GetTeamProjectCollection())
            {
                var versionControlServer = tfs.GetService<VersionControlServer>();

                // create workspace in the target directory
                Workspace workspace = null;

                try
                {
                    workspace = GetMappedWorkspace(versionControlServer, sourcePath, targetPath);
                    workspace.Get(VersionSpec.ParseSingleSpec("L" + label, versionControlServer.AuthenticatedUser), GetOptions.GetAll | GetOptions.Overwrite);
                }
                finally
                {
                    try { workspace.Delete(); }
                    catch { }
                }
            }
        }
        /// <summary>
        /// Returns a "fingerprint" that represents the current revision on the source control repository.
        /// </summary>
        /// <param name="path">The source control path to monitor.</param>
        /// <returns>
        /// A representation of the current revision in source control.
        /// </returns>
        public object GetCurrentRevision(string path)
        {
            var sourcePath = BuildSourcePath(path);

            // validate/clean sourcePath (should be $/SomeDir/SomePathNoTrailingSlash)
            using (var tfs = this.GetTeamProjectCollection())
            {
                var sourceControl = tfs.GetService<VersionControlServer>();

                sourcePath = sourceControl.GetItem(sourcePath).ServerItem; // matches the sourcePath with the base path returned by TFS

                // get the items
                ItemSet items = sourceControl.GetItems(sourcePath, VersionSpec.Latest, RecursionType.Full, DeletedState.Any, ItemType.Any);
                if (items == null || items.Items == null || items.Items.Length == 0)
                    return new byte[0];

                // return the highest change set id
                return items.Items.Max(i => i.ChangesetId);
            }
        }

        /// <summary>
        /// Gets the appropriate version control server based by connecting to TFS using the persisted credentials
        /// </summary>
        /// <returns></returns>
        protected virtual TfsTeamProjectCollection GetTeamProjectCollection()
        {
            if (this.UseSystemCredentials)
            {
                var projectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(this.BaseUri);
                projectCollection.EnsureAuthenticated();
                return projectCollection;
            }
            else
            {
                var projectColleciton = new TfsTeamProjectCollection(this.BaseUri, new TfsClientCredentials(new WindowsCredential(new NetworkCredential(this.UserName, this.Password, this.Domain))));
                projectColleciton.EnsureAuthenticated();
                return projectColleciton;
            }
        }

        /// <summary>
        /// Normalizes a source control path to be handled by this class.
        /// </summary>
        /// <param name="sourcePath">The source path.</param>
        private string BuildSourcePath(string sourcePath)
        {
            if (string.IsNullOrEmpty(sourcePath))
                return EmptyPathString;

            return sourcePath.TrimStart(DirectorySeparator);
        }
        /// <summary>
        /// Gets a TFS workspace mapped to the specified target path (i.e. generally the /SRC temporary directory)
        /// </summary>
        /// <param name="server">The server.</param>
        /// <param name="sourcePath">The source path.</param>
        /// <param name="targetPath">The target path.</param>
        private Workspace GetMappedWorkspace(VersionControlServer server, string sourcePath, string targetPath)
        {
            string workspaceName = "BuildMaster" + Guid.NewGuid().ToString().Replace("-", "");

            var workspaces = server.QueryWorkspaces(workspaceName, server.AuthorizedUser, Environment.MachineName);
            var workspace = workspaces.SingleOrDefault(ws => ws.Name == workspaceName);
            if (workspace != null)
            {
                workspace.Delete();
            }

            workspace = server.CreateWorkspace(workspaceName);

            workspace.CreateMapping(new WorkingFolder(sourcePath, targetPath));

            if (!workspace.HasReadPermission)
            {
                throw new SecurityException(String.Format("{0} does not have read permission for {1}", server.AuthorizedUser, targetPath));
            }

            return workspace;
        }
        private static bool IsAvailable2()
        {
            try
            {
                typeof(TfsTeamProjectCollection).GetType();
                return true;
            }
            catch
            {
                return false;
            }
        }
        private void ValidateConnection2()
        {
            using (var tfs = this.GetTeamProjectCollection())
            {
            }
        }
    }
}
