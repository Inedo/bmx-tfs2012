using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    internal sealed class TfsSourceControlProviderEditor : ProviderEditorBase
    {
        private ValidatingTextBox txtBaseUrl, txtUserName, txtDomain;
        private PasswordTextBox txtPassword;
        private DropDownList ddlAuthentication;

        protected override void CreateChildControls()
        {
            this.txtBaseUrl = new ValidatingTextBox { Width = 300 };

            this.txtUserName = new ValidatingTextBox { Width = 300 };

            this.txtDomain = new ValidatingTextBox { Width = 300 };

            this.txtPassword = new PasswordTextBox { Width = 270 };

            ddlAuthentication = new DropDownList();
            ddlAuthentication.Items.Add(new ListItem("System", "system"));
            ddlAuthentication.Items.Add(new ListItem("Specify account...", "specify"));

            var ffgAuthentication = new FormFieldGroup("Authentication",
                    "The method used for authenticating a connection to Team Foundation Server",
                    false,
                    new StandardFormField("Authentication:", ddlAuthentication)
                );

            var ffgCredentials = new FormFieldGroup("Credentials",
                    "Specify the credentials of the account you would like to use to connect to Team Foundation Server",
                    false,
                    new StandardFormField("Username:", txtUserName),
                    new StandardFormField("Password:", txtPassword),
                    new StandardFormField("Domain:", txtDomain)
                );

            CUtil.Add(this,
                new FormFieldGroup("TFS Server Name",
                    "The name of the Team Foundation Server to connect to, e.g. http://tfsserver:8080/tfs",
                    false,
                    new StandardFormField(
                        "Server Name:",
                        txtBaseUrl,
                        new RenderClientScriptDelegator(w =>
                        {
                            w.WriteLine(
                                // jQuery code used to hide the Credentials section if the "System" account is to be used
@"$().ready(function(){
    var onAuthorizationChange = function(){
        if($('#" + ddlAuthentication.ClientID + @" option:selected').val() == 'system') {
            $('#" + ffgCredentials.ClientID + @"').hide();
        }
        else {
            $('#" + ffgCredentials.ClientID + @"').show();
        }
    };
    onAuthorizationChange();
    $('#" + ddlAuthentication.ClientID + @"').change(onAuthorizationChange);
});");
                        })
                    )
                ),
                ffgAuthentication,
                ffgCredentials
              );

            base.CreateChildControls();
        }

        public override void BindToForm(ProviderBase extension)
        {
            EnsureChildControls();

            var tfsProvider = (TfsSourceControlProvider)extension;
            this.txtBaseUrl.Text = tfsProvider.BaseUrl;
            this.txtUserName.Text = tfsProvider.UserName;
            this.txtPassword.Text = tfsProvider.Password;
            this.txtDomain.Text = tfsProvider.Domain;

            if (tfsProvider.UseSystemCredentials)
                ddlAuthentication.SelectedValue = "system";
            else
                ddlAuthentication.SelectedValue = "specify";
        }

        public override ProviderBase CreateFromForm()
        {
            EnsureChildControls();

            return new TfsSourceControlProvider
            {
                BaseUrl = this.txtBaseUrl.Text,
                UserName = this.txtUserName.Text,
                Password = this.txtPassword.Text,
                Domain = this.txtDomain.Text,
                UseSystemCredentials = (this.ddlAuthentication.SelectedValue == "system")
            };
        }
    }
}
