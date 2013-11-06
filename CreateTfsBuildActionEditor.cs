using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    public sealed class CreateTfsBuildActionEditor : ActionEditorBase
    {
        private ValidatingTextBox txtTeamProject;
        private ValidatingTextBox txtBuildDefinition;
        private CheckBox chkWaitForCompletion;

        public override void BindToForm(ActionBase extension)
        {
            var action = (CreateTfsBuildAction)extension;

            this.txtTeamProject.Text = action.TeamProject;
            this.txtBuildDefinition.Text = action.BuildDefinition;
            this.chkWaitForCompletion.Checked = action.WaitForCompletion;
        }

        public override ActionBase CreateFromForm()
        {
            return new CreateTfsBuildAction()
            {
                TeamProject = this.txtTeamProject.Text,
                BuildDefinition = this.txtBuildDefinition.Text,
                WaitForCompletion = this.chkWaitForCompletion.Checked
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

            this.txtBuildDefinition = new ValidatingTextBox()
            {
                Required = true,
                Width = 300
            };

            this.chkWaitForCompletion = new CheckBox() { Text = "Wait For Completion" };

            this.Controls.Add(
                new FormFieldGroup(
                    "Team Project",
                    "The name of the team project for which to create the build.",
                    false,
                    new StandardFormField("Team Project:", this.txtTeamProject)
                ),
                new FormFieldGroup(
                    "Build Definition",
                    "The name of the build definition used to create the build.",
                    false,
                    new StandardFormField("Build Definition:", this.txtBuildDefinition)
                ),
                new FormFieldGroup(
                    "Wait for Completion",
                    "If checked, the BuildMaster execution will wait until the build is completed before continuing to the next action.",
                    true,
                    new StandardFormField("", this.chkWaitForCompletion)
                )
            );
        }
    }
}
