using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Azure
{
    internal sealed class SwapDeploymentActionEditor : AzureComputeActionBaseEditor 
    {
        private ValidatingTextBox txtProductionDeploymentName;
        private ValidatingTextBox txtSourceDeploymentName;

        public SwapDeploymentActionEditor() 
        {
            this.extensionInstance = new SwapDeploymentAction();
        }

        public override void BindToForm(ActionBase extension)
        {
            var action = (SwapDeploymentAction)extension;
            this.EnsureChildControls();
            base.BindToForm(extension);
            this.txtProductionDeploymentName.Text = action.ProductionDeploymentName;
            this.txtSourceDeploymentName.Text = action.SourceDeploymentName;
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();
            return PopulateProperties(new SwapDeploymentAction() 
                {
                    ProductionDeploymentName = this.txtProductionDeploymentName.Text,
                    SourceDeploymentName = this.txtSourceDeploymentName.Text,
                }
            );

        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            txtProductionDeploymentName = new ValidatingTextBox() { Width = 300 };
            txtSourceDeploymentName = new ValidatingTextBox() { Width = 300 };
            this.Controls.Add(
                new SlimFormField("Production Deployment Name:", txtProductionDeploymentName),
                new SlimFormField("Source Deployment Name:", txtSourceDeploymentName)
            );
        }
    }
}
