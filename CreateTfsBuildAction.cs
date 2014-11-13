using System;
using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Web;
using Microsoft.TeamFoundation.Build.Client;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    [ActionProperties(
        "Create TFS Build",
        "Creates a new build in TFS.",
        DefaultToLocalServer = true)]
    [RequiresInterface(typeof(IFileOperationsExecuter))]
    [RequiresInterface(typeof(IRemoteZip))]
    [CustomEditor(typeof(CreateTfsBuildActionEditor))]
    [Tag(Tags.Builds)]
    [Tag("tfs")]
    public sealed class CreateTfsBuildAction : TfsActionBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether the action should wait until the build completes before continuing.
        /// </summary>
        [Persistent]
        public bool WaitForCompletion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the action should fail if the build fails.
        /// </summary>
        [Persistent]
        public bool FailActionOnBuildFailure { get; set; }

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
                "Create a build of project \"{0}\" using the build definition \"{1}\" in TFS{2}.",
                this.TeamProject,
                this.BuildDefinition,
                this.WaitForCompletion ? " and wait until the build completes" : "",
                this.FailActionOnBuildFailure ? " and fail if the build fails." : ""
            );
        }

        private bool IsBuildSuccessful(IBuildDefinition buildDefinition)
        {
            return buildDefinition.LastBuildUri.Equals(buildDefinition.LastGoodBuildUri);
        }

        protected override void Execute()
        {
            var collection = this.GetTeamProjectCollection();

            var buildService = collection.GetService<IBuildServer>();
            var buildDefinition = buildService.GetBuildDefinition(this.TeamProject, this.BuildDefinition);

            if (buildDefinition == null)
                throw new InvalidOperationException(string.Format("Build definition \"{0}\" was not found.", this.BuildDefinition));

            this.LogDebug("Queueing build...");
            var queuedBuild = buildService.QueueBuild(buildDefinition);

            this.LogDebug("Build number \"{0}\" created for definition \"{1}\".", queuedBuild.Build.BuildNumber, queuedBuild.BuildDefinition.Name);

            if (this.WaitForCompletion)
            {
                this.LogDebug("Waiting for build completion...");
                queuedBuild.StatusChanged += (s, e) =>
                {
                    this.ThrowIfCanceledOrTimeoutExpired();
                    this.LogDebug("TFS Build status reported: " + ((IQueuedBuild)s).Status);
                };
                queuedBuild.WaitForBuildCompletion(TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(this.Timeout));

                if (FailActionOnBuildFailure && !IsBuildSuccessful(buildDefinition))
                {
                    throw new InvalidOperationException("Build failed");
                }
            }
        }
    }
}
