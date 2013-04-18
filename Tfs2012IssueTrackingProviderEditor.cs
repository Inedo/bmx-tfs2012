using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Providers;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.TFS2012
{
    internal sealed class Tfs2012IssueTrackingProviderEditor : ProviderEditorBase
    {
        ValidatingTextBox txtBaseUrl, txtCustomReleaseNumberFieldName, txtUserName, txtDomain;
        PasswordTextBox txtPassword;
        DropDownList ddlAuthentication;

        protected override void CreateChildControls()
        {
            txtBaseUrl = new ValidatingTextBox()
            {
                Width = Unit.Pixel(300)
            };
            
            txtCustomReleaseNumberFieldName = new ValidatingTextBox()
            {
                Width = Unit.Pixel(300)
            };
            
            txtUserName = new ValidatingTextBox()
            {
                Width= Unit.Pixel(300)
            };
            
            txtDomain = new ValidatingTextBox()
            {
                Width = Unit.Pixel(300)
            };
            
            txtPassword = new PasswordTextBox()
            {
                Width = Unit.Pixel(270)
            };
            
            ddlAuthentication = new DropDownList();
            ddlAuthentication.Items.Add(new ListItem("System", "system"));
            ddlAuthentication.Items.Add(new ListItem("Specify account...", "specify"));

            // ffgAuthentication
            FormFieldGroup ffgAuthentication = new FormFieldGroup("Authentication",
                    "The method used for authenticating a connection to Team Foundation Server.",
                    false,
                    new StandardFormField("Authentication:", ddlAuthentication)
                );

            // ffgCredentials
            FormFieldGroup ffgCredentials = new FormFieldGroup("Credentials",
                    "Specify the credentials of the account you would like to use to connect to the Team Foundation Server.",
                    false,
                    new StandardFormField("Username:", txtUserName),
                    new StandardFormField("Password:", txtPassword),
                    new StandardFormField("Domain:", txtDomain)
                );

            CUtil.Add(this,
                new FormFieldGroup("Team Foundation Server URL",
                    "The base URL of the Team Foundation Server, for example: http://tfsserver:port/vdir",
                    false,
                    new StandardFormField(
                        "Base Server URL:",
                        txtBaseUrl,
                        new RenderJQueryDocReadyDelegator(w =>
                        {
                            w.WriteLine(
                                // jQuery code used to hide the Credentials section if the "System" account is to be used
@"var onAuthorizationChange = function(){
        if($('#" + ddlAuthentication.ClientID + @" option:selected').val() == 'system') {
            $('#" + ffgCredentials.ClientID + @"').hide();
        }
        else {
            $('#" + ffgCredentials.ClientID + @"').show();
        }
    };
    onAuthorizationChange();
    $('#" + ddlAuthentication.ClientID + @"').change(onAuthorizationChange);");
                        })
                    )
                ),
                ffgAuthentication,
                ffgCredentials,
                new FormFieldGroup("Custom Release Number Field",
                    "If you store your TFS work item release numbers in a custom field, enter the full field \"refname\" of the custom field here - otherwise leave this field blank and \"Iteration\" will be used to retrieve them.<br /><br />For more information on custom work item types, visit <a href=\"http://msdn.microsoft.com/en-us/library/ms400654.aspx\" target=\"_blank\">http://msdn.microsoft.com/en-us/library/ms400654.aspx</a>",
                    false,
                    new StandardFormField(
                        "Custom Field:",
                        txtCustomReleaseNumberFieldName)
                    )
              );

            base.CreateChildControls();
        }

        public override void BindToForm(ProviderBase extension)
        {
            EnsureChildControls();

            var provider = (Tfs2012IssueTrackingProvider)extension;

            txtBaseUrl.Text = provider.BaseUrl;
            txtCustomReleaseNumberFieldName.Text = provider.CustomReleaseNumberFieldName;
            txtUserName.Text = provider.UserName;
            txtPassword.Text = provider.Password;
            txtDomain.Text = provider.Domain;

            if (provider.UseSystemCredentials)
                ddlAuthentication.SelectedValue = "system";
            else
                ddlAuthentication.SelectedValue = "specify";
        }

        public override ProviderBase CreateFromForm()
        {
            EnsureChildControls();

            var provider = new Tfs2012IssueTrackingProvider();

            provider.BaseUrl = txtBaseUrl.Text;
            provider.CustomReleaseNumberFieldName = txtCustomReleaseNumberFieldName.Text;
            provider.UserName = txtUserName.Text;
            provider.Password = txtPassword.Text;
            provider.Domain = txtDomain.Text;
            provider.UseSystemCredentials = (ddlAuthentication.SelectedValue == "system");

            return provider;
        }
    }
}
