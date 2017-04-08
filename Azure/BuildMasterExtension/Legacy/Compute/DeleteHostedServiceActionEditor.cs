using Inedo.BuildMaster.Extensibility.Actions;

namespace Inedo.BuildMasterExtensions.Azure
{
    internal sealed class DeleteHostedServiceActionEditor : AzureComputeActionBaseEditor 
    {
        public DeleteHostedServiceActionEditor() 
        {
            this.extensionInstance = new DeleteHostedServiceAction();
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();
            base.BindToForm(extension);
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return PopulateProperties(new DeleteHostedServiceAction());
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
        }
    }
}
