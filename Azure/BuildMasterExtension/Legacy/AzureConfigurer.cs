using Inedo.BuildMaster.Extensibility.Configurers.Extension;
using Inedo.BuildMaster.Web;
using Inedo.Serialization;

[assembly: ExtensionConfigurer(typeof(Inedo.BuildMasterExtensions.Azure.AzureConfigurer))]

namespace Inedo.BuildMasterExtensions.Azure
{
    [CustomEditor(typeof(AzureConfigurerEditor))]
    public sealed class AzureConfigurer : ExtensionConfigurerBase
    {
        [Persistent]
        public string AzureSDKPath { get; set; }
        [Persistent]
        public int ServerID { get; set; } = 1;
        [Persistent]
        public AzureAuthentication Credentials { get; set; } = new AzureAuthentication();
    }
}
