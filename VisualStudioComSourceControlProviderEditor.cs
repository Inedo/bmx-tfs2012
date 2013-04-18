using System;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    internal sealed class VisualStudioComSourceControlProviderEditor : ProviderEditorBase
    {
        private ValidatingTextBox txtBaseUrl;
        private ValidatingTextBox txtUserName;
        private PasswordTextBox txtPassword;
        private CheckBox chkBasicAuthentication;

        public VisualStudioComSourceControlProviderEditor()
        {
            this.ValidateBeforeSave += this.VisualStudioComSourceControlProviderEditor_ValidateBeforeSave;
        }

        public override void BindToForm(ProviderBase extension)
        {
            this.EnsureChildControls();

            var tfsProvider = (VisualStudioComSourceControlProvider)extension;
            this.txtBaseUrl.Text = tfsProvider.BaseUrl;
            this.txtUserName.Text = tfsProvider.UserName;
            this.txtPassword.Text = tfsProvider.Password;
            this.chkBasicAuthentication.Checked = tfsProvider.UseBasicAuthentication;
        }
        public override ProviderBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new VisualStudioComSourceControlProvider
            {
                BaseUrl = this.txtBaseUrl.Text,
                UserName = this.txtUserName.Text,
                Password = this.txtPassword.Text,
                UseBasicAuthentication = this.chkBasicAuthentication.Checked
            };
        }

        protected override void CreateChildControls()
        {
            this.txtBaseUrl = new ValidatingTextBox { Width = 300, Required = true };
            this.txtUserName = new ValidatingTextBox { Width = 300, Required = true };
            this.txtPassword = new PasswordTextBox { Width = 270, Required = true };
            this.chkBasicAuthentication = new CheckBox { Text = "Use Basic Authentication", Checked = true };

            this.Controls.Add(
                new FormFieldGroup(
                    "Collection URL",
                    "The URL of the collection to connect to. For example, https://contoso.visualstudio.com/defaultcollection",
                    false,
                    new StandardFormField("Collection URL:", this.txtBaseUrl)
                ),
                new FormFieldGroup(
                    "Credentials",
                    "Specify the service account credentials to use to connect to the server. See <a href=\"http://blogs.msdn.com/b/buckh/archive/2013/01/07/how-to-connect-to-tf-service-without-a-prompt-for-liveid-credentials.aspx\">this article</a> for how to configure TFS to allow basic authentication.",
                    false,
                    new StandardFormField("User Name:", this.txtUserName),
                    new StandardFormField("Password:", this.txtPassword),
                    new StandardFormField(string.Empty, this.chkBasicAuthentication)
                )
            );
        }

        private void VisualStudioComSourceControlProviderEditor_ValidateBeforeSave(object sender, ValidationEventArgs<ProviderBase> e)
        {
            Uri uri;
            if (!Uri.TryCreate(this.txtBaseUrl.Text, UriKind.Absolute, out uri))
            {
                e.ValidLevel = ValidationLevels.Error;
                e.Message = string.Format("{0} is not a valid URL.", this.txtBaseUrl.Text);
                return;
            }

            if (uri.Host.IndexOf("visualstudio.com", StringComparison.OrdinalIgnoreCase) < 0)
            {
                e.ValidLevel = ValidationLevels.Warning;
                e.Message = "This provider is only intended for use with TFS hosted at visualstudio.com";
                return;
            }
        }
    }
}
