using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    public sealed class CreateTfsBuildOutputArtifactActionEditor : ActionEditorBase
    {
        private ValidatingTextBox txtTeamProject;
        private ValidatingTextBox txtArtifactName;
        private ValidatingTextBox txtBuildDefinition;
        private ValidatingTextBox txtBuildNumber;
        private CheckBox chkIncludeUnsuccessful;

        public override void BindToForm(ActionBase extension)
        {
            var action = (CreateTfsBuildOutputArtifactAction)extension;

            this.txtTeamProject.Text = action.TeamProject;
            this.txtArtifactName.Text = action.ArtifactName;
            this.txtBuildDefinition.Text = action.BuildDefinition;
            this.txtBuildNumber.Text = action.BuildNumber;
            this.chkIncludeUnsuccessful.Checked = action.IncludeUnsuccessful;
        }

        public override ActionBase CreateFromForm()
        {
            return new CreateTfsBuildOutputArtifactAction()
            {
                TeamProject = this.txtTeamProject.Text,
                ArtifactName = this.txtArtifactName.Text,
                BuildDefinition = this.txtBuildDefinition.Text,
                BuildNumber = this.txtBuildNumber.Text,
                IncludeUnsuccessful = this.chkIncludeUnsuccessful.Checked
            };
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            this.txtTeamProject = new ValidatingTextBox()
            {
                Required = true,
                Width = 300
            };

            this.txtArtifactName = new ValidatingTextBox()
            {
                DefaultText = "Use name of build definition",
                Width = 300
            };

            this.txtBuildNumber = new ValidatingTextBox()
            {
                DefaultText = "Last successful"
            };

            this.txtBuildDefinition = new ValidatingTextBox()
            {
                DefaultText = "Any",
                Width = 300
            };

            this.chkIncludeUnsuccessful = new CheckBox() { Text = "Include Unsuccessful Builds" };

            this.Controls.Add(
                new FormFieldGroup(
                    "Team Project", 
                    "The name of the team project to import the build output.", 
                    false, 
                    new StandardFormField("Team Project:", this.txtTeamProject)
                ),
                new FormFieldGroup(
                    "Artifact Name", 
                    "Optionally specify the name of the artifact once imported to BuildMaster.", 
                    false, 
                    new StandardFormField("Artifact Name:", this.txtArtifactName)
                ),
                new FormFieldGroup(
                    "Build Definition", 
                    "Optionally filter the import by build definition.", 
                    false, 
                    new StandardFormField("Build Definition:", this.txtBuildDefinition)
                ),
                new FormFieldGroup(
                    "Build Number", 
                    "Optionally filter the import by build number.", 
                    false, 
                    new StandardFormField("Build Number:", this.txtBuildNumber)
                ),
                new FormFieldGroup(
                    "Include Unsuccessful Builds",
                    "If checked, the latest successful, failed, or partially completed build will be imported. Otherwise, only a successful build will be imported.",
                    true,
                    new StandardFormField("", this.chkIncludeUnsuccessful)
                )
            );
        }
    }
}
