#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Credentials;
using Inedo.BuildMaster.Web;
using Inedo.BuildMaster.Web.Controls;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Credentials;
using Inedo.Otter.Extensions;
using Inedo.Otter.Web.Controls;
#endif
using Inedo.Documentation;
using Inedo.Extensions.Azure.SuggestionProviders;
using Inedo.Serialization;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Resource.Fluent;
using Microsoft.Azure.Management.Resource.Fluent.Authentication;
using System;
using System.ComponentModel;
using System.Reflection;
using FluentAzureCredentials = Microsoft.Azure.Management.Resource.Fluent.Authentication.AzureCredentials;

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
        public string ClientId { get; set; }

        [Required]
        [Persistent(Encrypted = true)]
        [FieldEditMode(FieldEditMode.Password)]
        public string ClientSecret { get; set; }

        [Required]
        [Persistent]
        public string UserName { get; set; }

        [Required]
        [Persistent(Encrypted = true)]
        [FieldEditMode(FieldEditMode.Password)]
        public string Password { get; set; }

        [Required]
        [Persistent]
        public string DomainName { get; set; }

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

        public FluentAzureCredentials FluentCredentials
        {
            get
            {
                var credentials = new AzureCredentialsFactory().FromUser(this.UserName, this.Password, this.ClientId, this.TenantId, this.RealEnvironment);
                if (!string.IsNullOrEmpty(this.SubscriptionId))
                {
                    credentials = credentials.WithDefaultSubscription(this.SubscriptionId);
                }
                return credentials;
            }
        }

        public IAzure Azure
        {
            get
            {
                return Microsoft.Azure.Management.Fluent.Azure.Configure()
                    .WithUserAgent(typeof(AzureCredentials).Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product, typeof(ResourceCredentials).Assembly.GetName().Version.ToString())
                    .Authenticate(this.FluentCredentials)
                    .WithDefaultSubscription();
            }
        }

        public override RichDescription GetDescription()
        {
            return new RichDescription(new Hilite(this.ClientId), " @ ", new Hilite(this.DomainName));
        }

        internal static AzureCredentials FromComponentConfiguration(IComponentConfiguration config)
        {
            AzureEnvironmentName environment;
            if (!Enum.TryParse(config[nameof(Environment)], true, out environment))
            {
                environment = default(AzureEnvironmentName);
            }
            return new AzureCredentials
            {
                Environment = environment,
                DomainName = config[nameof(DomainName)],
                ClientId = config[nameof(ClientId)],
                ClientSecret = config[nameof(ClientSecret)],
                UserName = config[nameof(UserName)],
                Password = config[nameof(Password)],
                TenantId = config[nameof(TenantId)],
                SubscriptionId = config[nameof(SubscriptionId)]
            };
        }
    }
}
