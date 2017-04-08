using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.Web.Controls;

namespace Inedo.BuildMasterExtensions.Azure
{
    internal sealed class DeployPackageActionEditor : AzureActionWithConfigBaseEditor 
    {
        private ValidatingTextBox txtLabel;
        private CheckBox chkStartDeployment;
        private ValidatingTextBox txtStorageAccountName;
        private ValidatingTextBox txtStorageAccessKey;
        private FileBrowserTextBox ffpPackageFile;
        private ValidatingTextBox txtPackageFileStorageLocation;
        private CheckBox chkDeleteFromStorage;

        public DeployPackageActionEditor() 
        {
            this.extensionInstance = new DeployPackageAction();
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();
            base.BindToForm(extension);
            var action = (DeployPackageAction) extension;
            this.txtLabel.Text = action.Label;
            this.chkStartDeployment.Checked = action.StartDeployment;
            this.txtStorageAccountName.Text = action.StorageAccountName;
            this.txtStorageAccessKey.Text = action.StorageAccessKey;
            this.ffpPackageFile.Text = action.PackageFile;
            this.txtPackageFileStorageLocation.Text = action.PackageFileStorageLocation;
            this.chkDeleteFromStorage.Checked = action.DeletePackageFromStorage;
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return PopulateProperties(new DeployPackageAction()
                {
                    Label = this.txtLabel.Text,
                    StartDeployment = this.chkStartDeployment.Checked,
                    StorageAccountName = this.txtStorageAccountName.Text,
                    StorageAccessKey = this.txtStorageAccessKey.Text,
                    PackageFile = this.ffpPackageFile.Text,
                    PackageFileStorageLocation = this.txtPackageFileStorageLocation.Text,
                    DeletePackageFromStorage = this.chkDeleteFromStorage.Checked 
                }
            );
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            txtLabel = new ValidatingTextBox() {Width = 300};
            chkStartDeployment = new CheckBox() { Width = 300 };
            txtStorageAccountName = new ValidatingTextBox() { Width = 300, Required = true };
            txtStorageAccessKey = new ValidatingTextBox() {Width = 300, Required = true};
            ffpPackageFile = new FileBrowserTextBox() { ServerId = 1 };
            txtPackageFileStorageLocation = new ValidatingTextBox() { Width = 300 };
            chkDeleteFromStorage = new CheckBox() { Width = 300 };
            this.Controls.Add(
                new SlimFormField("Label:", txtLabel),
                new SlimFormField("Start deployment:", chkStartDeployment),
                new SlimFormField("Blob Storage account name:", txtStorageAccountName),
                new SlimFormField("Blob Storage access key:", txtStorageAccessKey),
                new SlimFormField("Package file disk location:", ffpPackageFile),
                new SlimFormField("Package file blob location:", txtPackageFileStorageLocation),
                new SlimFormField("Delete staged package from Blob Storage when complete", chkDeleteFromStorage)
            );
        }
    }
}
