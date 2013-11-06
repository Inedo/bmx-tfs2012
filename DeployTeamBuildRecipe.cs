using Inedo.BuildMaster;
using Inedo.BuildMaster.Data;
using Inedo.BuildMaster.Extensibility.Recipes;
using Inedo.BuildMaster.Web;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    [RecipeProperties(
       "Deploy TFS Team Build",
       "An application that imports a build artifact from a TFS build's drop location and deploys through multiple environments.",
       RecipeScopes.NewApplication)]
    [CustomEditor(typeof(DeployTeamBuildRecipeEditor))]
    public sealed class DeployTeamBuildRecipe : RecipeBase, IApplicationCreatingRecipe, IWorkflowCreatingRecipe
    {
        public string ApplicationGroup { get; set; }
        public string ApplicationName { get; set; }
        public int ApplicationId { get; set; }

        public string WorkflowName { get; set; }
        public int[] WorkflowSteps { get; set; }
        public int WorkflowId { get; set; }

        public string TargetDeploymentPath { get; set; }
        public string TeamProject { get; set; }
        public string BuildDefinition { get; set; }

        public override void Execute()
        {
            int deployableId = Util.Recipes.CreateDeployable(this.ApplicationId, this.ApplicationName);
            string deployableName = this.ApplicationName;
            int firstEnvironmentId = this.WorkflowSteps[0];

            int planId = Util.Recipes.CreatePlan(this.ApplicationId, deployableId, firstEnvironmentId,
                "Get Artifact from Team Build Output",
                "Actions in this group will start a build in TFS and import it into BuildMaster as an artifact."
            );

            Util.Recipes.AddAction(planId, new CreateTfsBuildAction()
                {
                    TeamProject = this.TeamProject,
                    BuildDefinition = this.BuildDefinition,
                    WaitForCompletion = true
                }
            );

            Util.Recipes.AddAction(planId, new CreateTfsBuildOutputArtifactAction()
                {
                    TeamProject = this.TeamProject,
                    BuildDefinition = this.BuildDefinition,
                    ArtifactName = deployableName
                }
            );

            foreach (int environmentId in this.WorkflowSteps)
            {
                Util.Recipes.CreatePlan(this.ApplicationId, null, environmentId,
                    "Stop Application",
                    "Stop/shutdown/disable the application or application servers prior to deployment."
                );

                planId = Util.Recipes.CreatePlan(this.ApplicationId, deployableId, environmentId,
                    "Deploy " + deployableName,
                    "Deploy the artifacts created in the build actions, and then any configuration files needed."
                );

                Util.Recipes.AddAction(planId, 1, Util.Recipes.Munging.MungeCoreExAction(
                    "Inedo.BuildMaster.Extensibility.Actions.Artifacts.DeployArtifactAction", new
                    {
                        ArtifactName = deployableName,
                        OverriddenTargetDirectory = this.TargetDeploymentPath,
                        DoNotClearTargetDirectory = false
                    })
                );

                Util.Recipes.CreatePlan(this.ApplicationId, null, environmentId,
                    "Start Application",
                    "Start the application or application servers after deployment, and possibly run some post-startup automated testing."
                );
            }

            Util.Recipes.CreateSetupRelease(this.ApplicationId, Domains.ReleaseNumberSchemes.MajorMinor, this.WorkflowId, new[] { deployableId });
        }
    }
}
