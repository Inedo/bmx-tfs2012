//using System;
//using Inedo.BuildMaster.Extensibility.Providers;
//using Inedo.BuildMaster.Web.Controls;
//using Inedo.BuildMaster.Web.Controls.Extensions;
//using Inedo.Web.Controls;

//namespace Inedo.BuildMasterExtensions.TFS2012
//{
//    internal sealed class VisualStudioComIssueTrackingProviderEditor : ProviderEditorBase
//    {
//        private ValidatingTextBox txtBaseUrl;
//        private ValidatingTextBox txtUserName;
//        private PasswordTextBox txtPassword;

//        public VisualStudioComIssueTrackingProviderEditor()
//        {
//            this.ValidateBeforeSave += this.VisualStudioComIssueTrackingProviderEditor_ValidateBeforeSave;
//        }

//        public override void BindToForm(ProviderBase extension)
//        {
//            this.EnsureChildControls();

//            var tfsProvider = (VisualStudioComIssueTrackingProvider)extension;
//            this.txtBaseUrl.Text = tfsProvider.BaseUrl;
//            this.txtUserName.Text = tfsProvider.UserName;
//            this.txtPassword.Text = tfsProvider.Password;
//        }
//        public override ProviderBase CreateFromForm()
//        {
//            this.EnsureChildControls();

//            return new VisualStudioComIssueTrackingProvider
//            {
//                BaseUrl = this.txtBaseUrl.Text,
//                UserName = this.txtUserName.Text,
//                Password = this.txtPassword.Text
//            };
//        }

//        protected override void CreateChildControls()
//        {
//            this.txtBaseUrl = new ValidatingTextBox { Width = 300, Required = true };
//            this.txtUserName = new ValidatingTextBox { Width = 300, Required = true };
//            this.txtPassword = new PasswordTextBox { Width = 270, Required = true };

//            this.Controls.Add(
//                new FormFieldGroup(
//                    "Collection URL",
//                    "The URL of the collection to connect to. For example, https://contoso.visualstudio.com/defaultcollection",
//                    false,
//                    new StandardFormField("Collection URL:", this.txtBaseUrl)
//                ),
//                new FormFieldGroup(
//                    "Credentials",
//                    "Specify the service account credentials to use to connect to the server.",
//                    true,
//                    new StandardFormField("User Name:", this.txtUserName),
//                    new StandardFormField("Password:", this.txtPassword)
//                )
//            );
//        }

//        private void VisualStudioComIssueTrackingProviderEditor_ValidateBeforeSave(object sender, ValidationEventArgs<ProviderBase> e)
//        {
//            Uri uri;
//            if (!Uri.TryCreate(this.txtBaseUrl.Text, UriKind.Absolute, out uri))
//            {
//                e.ValidLevel = ValidationLevels.Error;
//                e.Message = string.Format("{0} is not a valid URL.", this.txtBaseUrl.Text);
//                return;
//            }

//            if (uri.Host.IndexOf("visualstudio.com", StringComparison.OrdinalIgnoreCase) < 0)
//            {
//                e.ValidLevel = ValidationLevels.Warning;
//                e.Message = "This provider is only intended for use with TFS hosted at visualstudio.com";
//                return;
//            }
//        }
//    }
//}
