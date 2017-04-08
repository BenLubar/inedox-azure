#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Credentials;
using Inedo.BuildMaster.Web.Controls;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Credentials;
using Inedo.Otter.Web.Controls;
#endif
using Inedo.Documentation;
using Inedo.Extensions.Azure.SuggestionProviders;
using Inedo.Serialization;
using Microsoft.Azure.Management.Resource.Fluent;
using Microsoft.Azure.Management.Resource.Fluent.Authentication;
using System.ComponentModel;
using RealAzureCredentials = Microsoft.Azure.Management.Resource.Fluent.Authentication.AzureCredentials;

namespace Inedo.Extensions.Azure.Credentials
{
    [DisplayName("Microsoft Azure")]
    [Description("Credentials for Microsoft Azure.")]
    [ScriptAlias("Azure")]
    public sealed class AzureCredentials : ResourceCredentials
    {
        public enum AzureEnvironmentName
        {
            Global,
            Germany,
            China,
            USGovernment
        }

        [Required]
        [Persistent]
        public AzureEnvironmentName Environment { get; set; }

        [Required]
        [Persistent]
        public string UserName { get; set; }

        [Required]
        [Persistent]
        public string Password { get; set; }

        [Required]
        [Persistent]
        public string ClientId { get; set; }

        [Required]
        [Persistent]
        [SuggestibleValue(typeof(TenantSuggestionProvider))]
        public string TenantId { get; set; }

        [Persistent]
        [SuggestibleValue(typeof(SubscriptionSuggestionProvider))]
        public string SubscriptionId { get; set; }

        private AzureEnvironment RealEnvironment => AH.Switch<AzureEnvironmentName, AzureEnvironment>(this.Environment)
            .Case(AzureEnvironmentName.Global, AzureEnvironment.AzureGlobalCloud)
            .Case(AzureEnvironmentName.China, AzureEnvironment.AzureChinaCloud)
            .Case(AzureEnvironmentName.Germany, AzureEnvironment.AzureGermanCloud)
            .Case(AzureEnvironmentName.USGovernment, AzureEnvironment.AzureUSGovernment)
            .Default(AzureEnvironment.AzureGlobalCloud).End();

        internal RealAzureCredentials Credentials
        {
            get
            {
                var credentials = new RealAzureCredentials(new UserLoginInformation
                {
                    UserName = this.UserName,
                    Password = this.Password,
                    ClientId = this.ClientId
                }, this.TenantId, this.RealEnvironment);

                if (!string.IsNullOrWhiteSpace(this.SubscriptionId))
                {
                    credentials = credentials.WithDefaultSubscription(this.SubscriptionId);
                }

                return credentials;
            }
        }

        public override RichDescription GetDescription()
        {
            return new RichDescription("Azure ", new Hilite(this.UserName));
        }
    }
}
