using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;
using Inedo.Web.Controls.SimpleHtml;

namespace Inedo.BuildMasterExtensions.Azure
{
    public abstract class AzureComputeActionBaseEditor : ActionEditorBase
    {
        protected ValidatingTextBox txtServiceName;
        protected ValidatingTextBox txtDeploymentName;
        protected ValidatingTextBox txtSlotName;
        protected TextBox txtExtendedProperties;
        protected TextBox txtExtensionConfiguration;
        protected CheckBox chkWarningsAsError;
        protected CheckBox chkWaitForCompletion;
        protected ValidatingTextBox txtSubscriptionID;
        protected ValidatingTextBox txtCertificateName;

        protected AzureComputeActionBase extensionInstance;

        public AzureComputeActionBaseEditor()
        {
            this.txtServiceName = new ValidatingTextBox() { Width = 300, Required = true };
            this.txtDeploymentName = new ValidatingTextBox() { Width = 300 };
            this.txtSlotName = new ValidatingTextBox() { Width = 300 };
            this.txtExtendedProperties = new TextBox() { TextMode = TextBoxMode.MultiLine, Width = 300, Rows = 4 };
            this.txtExtensionConfiguration = new TextBox() { TextMode = TextBoxMode.MultiLine, Width = 300, Rows = 4 };
            this.chkWarningsAsError = new CheckBox() { Width = 300, TextAlign = TextAlign.Right };
            this.chkWaitForCompletion = new CheckBox() { Width = 300, TextAlign = TextAlign.Right };
            this.txtSubscriptionID = new ValidatingTextBox() { Width = 300 };
            this.txtCertificateName = new ValidatingTextBox() { Width = 300 };
        }

        protected virtual AzureComputeActionBase PopulateProperties(AzureComputeActionBase Value)
        {
            if (Value.UsesServiceName)
                Value.ServiceName = txtServiceName.Text;
            if (Value.UsesDeploymentName)
                Value.DeploymentName = txtDeploymentName.Text;
            if (Value.UsesSlotName)
                Value.SlotName = txtSlotName.Text;
            if (Value.UsesExtendedProperties)
                Value.ExtendedProperties = txtExtendedProperties.Text;
            if (Value.UsesExtensionConfiguration)
                Value.ExtensionConfiguration = txtExtensionConfiguration.Text;
            if (Value.UsesTreatWarningsAsError)
                Value.TreatWarningsAsError = chkWarningsAsError.Checked;
            if (Value.UsesWaitForCompletion)
                Value.WaitForCompletion = chkWaitForCompletion.Checked;
            if (!string.IsNullOrEmpty(this.txtSubscriptionID.Text))
                Value.ActionCredentials = new AzureAuthentication() { SubscriptionID = this.txtSubscriptionID.Text, CertificateName = this.txtCertificateName.Text };
            else
                Value.ActionCredentials = null;
            return Value;
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var action = (AzureComputeActionBase)extension;
            this.txtServiceName.Text = action.ServiceName;
            this.txtDeploymentName.Text = action.DeploymentName;
            this.txtSlotName.Text = action.SlotName;
            this.txtExtendedProperties.Text = action.ExtendedProperties;
            this.txtExtensionConfiguration.Text = action.ExtensionConfiguration;
            this.chkWarningsAsError.Checked = action.TreatWarningsAsError;
            this.chkWaitForCompletion.Checked = action.WaitForCompletion;
            if (null != action.ActionCredentials)
            {
                this.txtSubscriptionID.Text = action.ActionCredentials.SubscriptionID;
                this.txtCertificateName.Text = action.ActionCredentials.CertificateName;
            }
        }

        protected override void CreateChildControls()
        {
            AddActionAuthentication();
            AddServiceInformation();
            AddDeploymentInformation();
            AddExtendedInformation();
            AddActionOptions();
        }

        private void AddActionAuthentication()
        {
            this.Controls.Add(
                new SlimFormField("Subscription ID:", txtSubscriptionID),
                new SlimFormField("Certificate name:", txtCertificateName)
            );
        }

        private void AddServiceInformation()
        {

            if (extensionInstance.UsesServiceName)
            {
                this.Controls.Add(new SlimFormField("Service name:", this.txtServiceName));
            }
        }

        private void AddDeploymentInformation()
        {
            if (extensionInstance.UsesDeploymentName)
                this.Controls.Add(new SlimFormField("Deployment name:", txtDeploymentName));
            if (extensionInstance.UsesSlotName)
                this.Controls.Add(new SlimFormField("Deployment slot:", txtSlotName));
        }

        private void AddExtendedInformation()
        {
            this.txtExtendedProperties.Rows = 4;
            this.txtExtensionConfiguration.Rows = 4;
            if (extensionInstance.UsesExtendedProperties)
                this.Controls.Add(new SlimFormField("Extended propeties (name=value):", txtExtendedProperties));
            if (extensionInstance.UsesExtensionConfiguration)
                this.Controls.Add(new SlimFormField("Extension configuration (XML fragment):", txtExtensionConfiguration));
        }

        private void AddActionOptions()
        {
            var ff = new SlimFormField("Action options:");
            if (extensionInstance.UsesTreatWarningsAsError)
                ff.Controls.Add(new Div(chkWarningsAsError));
            if (extensionInstance.UsesWaitForCompletion)
                ff.Controls.Add(new Div(chkWaitForCompletion));
            if (extensionInstance.UsesTreatWarningsAsError || extensionInstance.UsesWaitForCompletion)
                this.Controls.Add(ff);
        }
    }
}
