using System;
using System.IO;
using System.Linq;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Files;
using Inedo.BuildMaster.Web;
using Microsoft.TeamFoundation.Build.Client;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    [ActionProperties(
        "Create Artifact from TFS Build Output",
        "Gets an artifact from a TFS build server drop location.",
        DefaultToLocalServer = true)]
    [RequiresInterface(typeof(IFileOperationsExecuter))]
    [RequiresInterface(typeof(IRemoteZip))]
    [CustomEditor(typeof(CreateTfsBuildOutputArtifactActionEditor))]
    [Tag(Tags.Artifacts)]
    [Tag(Tags.Builds)]
    [Tag("tfs")]
    public sealed class CreateTfsBuildOutputArtifactAction : TfsActionBase
    {
        /// <summary>
        /// Gets or sets the build number if not empty, or includes all builds in the search.
        /// </summary>
        [Persistent]
        public string BuildNumber { get; set; }

        /// <summary>
        /// Gets or sets the name of the artifact if not empty, otherwise use the build definition name.
        /// </summary>
        [Persistent]
        public string ArtifactName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the build spec should include unsuccessful builds.
        /// </summary>
        [Persistent]
        public bool IncludeUnsuccessful { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        /// <remarks>
        /// This should return a user-friendly string describing what the Action does
        /// and the state of its important persistent properties.
        /// </remarks>
        public override string ToString()
        {
            return string.Format(
                "Create a BuildMaster artifact from the build output of the team project \"{0}\", for {1} of {2}.",
                this.TeamProject,
                string.IsNullOrEmpty(this.BuildNumber) ? "the last successful build" : string.Format("build number \"{0}\"", this.BuildNumber),
                string.IsNullOrEmpty(this.BuildDefinition) ? "any build definition" : string.Format("build definition \"{0}\"", this.BuildDefinition)
            );
        }

        protected override void Execute()
        {
            var collection = this.GetTeamProjectCollection();

            var buildService = collection.GetService<IBuildServer>();            

            var spec = buildService.CreateBuildDetailSpec(this.TeamProject, string.IsNullOrEmpty(this.BuildDefinition) ? "*" : this.BuildDefinition);
            if (!string.IsNullOrEmpty(this.BuildNumber))
                spec.BuildNumber = this.BuildNumber;
            spec.MaxBuildsPerDefinition = 1;
            spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;
            spec.Status = BuildStatus.Succeeded;

            if (this.IncludeUnsuccessful)
                spec.Status |= (BuildStatus.Failed | BuildStatus.PartiallySucceeded);

            var result = buildService.QueryBuilds(spec);
            var build = result.Builds.FirstOrDefault();
            if (build == null)
                throw new InvalidOperationException(string.Format("Build {0} for team project {1} definition {2} did not return any builds.", this.BuildNumber, this.TeamProject, this.BuildDefinition));

            this.LogDebug("Build number {0} drop location: {1}", build.BuildNumber, build.DropLocation);

            CreateArtifact(string.IsNullOrEmpty(this.ArtifactName) ? build.BuildDefinition.Name : this.ArtifactName, build.DropLocation);
        }

        private void CreateArtifact(string artifactName, string path)
        {
            if (string.IsNullOrEmpty(artifactName) || artifactName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                throw new InvalidOperationException("Artifact Name cannot contain invalid file name characters: " + new string(Path.GetInvalidFileNameChars()));

            if (StoredProcs.Releases_GetRelease(this.Context.ApplicationId, this.Context.ReleaseNumber)
                .Execute().ReleaseDeployables_Extended
                .Any(rd => rd.Deployable_Id == this.Context.DeployableId && rd.InclusionType_Code == Domains.DeployableInclusionTypes.Referenced))
            {
                this.LogError(
                    "An Artifact cannot be created for this Deployable because the Deployable is Referenced (as opposed to Included) by this Release. " +
                    "To prevent this error, either include this Deployable in the Release or use a Predicate to prevent this action group from being executed.");
                return;
            }

            var fileOps = this.Context.Agent.GetService<IFileOperationsExecuter>();
            var zipPath = fileOps.CombinePath(this.Context.TempDirectory, artifactName + ".zip");

            this.LogDebug("Preparing directories...");
            fileOps.DeleteFiles(new[] { zipPath });

            this.ThrowIfCanceledOrTimeoutExpired();

            var rootEntry = fileOps.GetDirectoryEntry(
                new GetDirectoryEntryCommand
                {
                    Path = path,
                    Recurse = false,
                    IncludeRootPath = false
                }
            ).Entry;

            if ((rootEntry.Files == null || rootEntry.Files.Length == 0) && (rootEntry.SubDirectories == null || rootEntry.SubDirectories.Length == 0))
                this.LogWarning("There are no files to capture in this artifact.");

            this.LogDebug("Zipping output...");
            this.Context.Agent.GetService<IRemoteZip>().CreateZipFile(path, zipPath);

            this.ThrowIfCanceledOrTimeoutExpired();

            this.LogDebug("Transferring file to artifact library...");

            string artifactsBasePath = StoredProcs.Configuration_GetValue("CoreEx", "ArtifactsBasePath", null).Execute();

            var artifactPath = Util.Artifacts.GetFullArtifactPath(
                artifactsBasePath,
                this.Context.ApplicationId,
                this.Context.ReleaseNumber,
                this.Context.BuildNumber,
                this.Context.DeployableId ?? 0,
                artifactName
            );

            var artifactDir = Path.GetDirectoryName(artifactPath);

            if (artifactPath.StartsWith(@"\\") && fileOps.DirectoryExists(artifactDir))
            {
                this.LogDebug("Artifacts path is a network share, transferring file to artifact library from the remote server...");
                fileOps.FileCopyBatch(
                    this.Context.TempDirectory,
                    new[] { zipPath },
                    artifactDir,
                    new[] { artifactPath },
                    true,
                    true
                );
            }
            else
            {
                this.LogDebug("Transferring file to artifact library...");
                Directory.CreateDirectory(artifactDir);
                Util.Files.TransferFile(
                    fileOps,
                    zipPath,
                    Util.Agents.CreateLocalAgent().GetService<IFileOperationsExecuter>(),
                    artifactPath
                );
            }

            StoredProcs.Artifacts_CreateOrReplaceArtifact(
                this.Context.ApplicationId,
                this.Context.ReleaseNumber,
                this.Context.BuildNumber,
                Util.NullIf(this.Context.DeployableId, 0),
                artifactName,
                this.Context.ExecutionId
            ).ExecuteNonQuery();

            this.LogDebug("Cleaning up...");
            fileOps.DeleteFiles(new[] { zipPath });

            this.LogInformation("Artifact saved");
        }
    }
}
