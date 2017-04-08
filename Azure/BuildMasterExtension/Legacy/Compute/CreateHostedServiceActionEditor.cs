using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Azure
{
    internal sealed class CreateHostedServiceActionEditor : AzureComputeActionBaseEditor
    {
        private ValidatingTextBox txtLabel;
        private ValidatingTextBox txtDescripition;
        private ValidatingTextBox txtAffinityGroup;
        private ValidatingTextBox txtLocation;

        public CreateHostedServiceActionEditor()
        {
            this.extensionInstance = new CreateHostedServiceAction();
        }

        public override void BindToForm(ActionBase extension)
        {
            var action = (CreateHostedServiceAction)extension;
            this.EnsureChildControls();
            base.BindToForm(extension);
            this.txtLabel.Text = action.Label;
            this.txtDescripition.Text = action.Description;
            this.txtAffinityGroup.Text = action.AffinityGroup;
            this.txtLocation.Text = action.Location;
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return PopulateProperties(
                new CreateHostedServiceAction()
                {
                    Label = this.txtLabel.Text,
                    Description = this.txtDescripition.Text,
                    AffinityGroup = this.txtAffinityGroup.Text,
                    Location = this.txtLocation.Text
                }
            );
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            txtLabel = new ValidatingTextBox() { Width = 300 };
            txtDescripition = new ValidatingTextBox() { Width = 300 };
            txtLocation = new ValidatingTextBox() { Width = 300 };
            txtAffinityGroup = new ValidatingTextBox() { Width = 300 };
            this.Controls.Add(
                new SlimFormField("Label:", txtLabel),
                new SlimFormField("Description:", txtDescripition),
                new SlimFormField("Location:", txtLocation),
                new SlimFormField("Affinity group:", txtAffinityGroup)
            );
        }
    }
}
