using System;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;
using Inedo.Web.Controls.SimpleHtml;

namespace Inedo.BuildMasterExtensions.Azure.Storage
{
    internal sealed class UploadFilesToBlobStorageActionEditor : ActionEditorBase
    {
        private ValidatingTextBox txtAccountName;
        private ValidatingTextBox txtAccessKey;
        private ValidatingTextBox txtContainerName;
        private ValidatingTextBox txtFileMasks;
        private CheckBox chkRecursive;
        private ValidatingTextBox txtTargetPath;

        public override bool DisplaySourceDirectory => true;

        public override void BindToForm(ActionBase extension)
        {
            var action = (UploadFilesToBlobStorageAction)extension;
            this.txtAccountName.Text = action.AccountName;
            this.txtAccessKey.Text = action.AccessKey;
            this.txtContainerName.Text = action.Container;
            this.txtFileMasks.Text = string.Join(Environment.NewLine, action.FileMasks ?? new string[0]);
            this.chkRecursive.Checked = action.Recursive;
            this.txtTargetPath.Text = action.TargetFolder;
        }
        public override ActionBase CreateFromForm()
        {
            return new UploadFilesToBlobStorageAction
            {
                AccountName = this.txtAccountName.Text,
                AccessKey = this.txtAccessKey.Text,
                Container = this.txtContainerName.Text,
                FileMasks = this.txtFileMasks.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries),
                Recursive = this.chkRecursive.Checked,
                TargetFolder = this.txtTargetPath.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.txtAccountName = new ValidatingTextBox
            {
                ID = "txtAccountName",
                Required = true
            };

            this.txtAccessKey = new ValidatingTextBox
            {
                ID = "txtAccessKey",
                Required = true
            };

            this.txtContainerName = new ValidatingTextBox
            {
                ID = "txtContainerName",
                Required = true
            };

            this.txtFileMasks = new ValidatingTextBox
            {
                Required = true,
                Rows = 3,
                TextMode = TextBoxMode.MultiLine
            };

            this.chkRecursive = new CheckBox
            {
                Text = "Recursively upload files from subdirectories"
            };

            this.txtTargetPath = new ValidatingTextBox
            {
                DefaultText = "(container root)"
            };

            this.Controls.Add(
                new SlimFormField("Account:", this.txtAccountName),
                new SlimFormField("Access key:", this.txtAccessKey),
                new SlimFormField("File masks:", new Div(this.txtFileMasks), new Div(this.chkRecursive)),
                new SlimFormField("Container:", this.txtContainerName),
                new SlimFormField("Target path:", this.txtTargetPath)
            );
        }
    }
}
