#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Configurations;
using Inedo.BuildMaster.Extensibility.Credentials;
using Inedo.BuildMaster.Web.Controls;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Configurations;
using Inedo.Otter.Extensibility.Credentials;
using Inedo.Otter.Web.Controls;
#endif
using Inedo.Documentation;
using Inedo.Extensions.Azure.Credentials;
using Inedo.Extensions.Azure.SuggestionProviders;
using Inedo.Serialization;
using Microsoft.Azure.Management.Fluent;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Azure.Management.AppService.Fluent;

namespace Inedo.Extensions.Azure.Configurations
{
    public sealed class WebAppConfiguration : PersistedConfiguration, IExistential, IHasCredentials<AzureCredentials>
    {
        private string credentialName = null;

        [Required]
        [ConfigurationKey]
        [DisplayName("Credentials")]
        [Description(CommonDescriptions.Credentials)]
        [ScriptAlias("Credentials")]
        public string CredentialName
        {
            get
            {
                return this.credentialName;
            }

            set
            {
                if (value != null)
                {
                    this.credentialName = value;
                }
            }
        }

        public IAzure Azure => this.TryGetCredentials()?.Azure;

        [Persistent]
        [Required]
        [ConfigurationKey]
        [DisplayName("Resource group")]
        [ScriptAlias("ResourceGroup")]
        [SuggestibleValue(typeof(ResourceGroupSuggestionProvider))]
        public string ResourceGroup { get; set; }

        [Persistent]
        [Required]
        [DisplayName("Plan name")]
        [ScriptAlias("PlanName")]
        public string PlanName { get; set; }

        [Persistent]
        [Required]
        [ConfigurationKey]
        [DisplayName("App name")]
        [Description("Your app will be accessible from <code>[app name].azurewebsites.net</code>.")]
        [ScriptAlias("Name")]
        public string Name { get; set; }

        [Persistent]
        [DisplayName("Always on")]
        [ScriptAlias("AlwaysOn")]
        public bool? AlwaysOn { get; set; }

        [Persistent]
        [DisplayName("Websockets enabled")]
        [ScriptAlias("WebSockets")]
        public bool? WebSockets { get; set; }

        [Persistent]
        [DisplayName(".NET Framework version")]
        [ScriptAlias("NetFrameworkVersion")]
        [Category("Runtimes")]
        [SuggestibleValue(typeof(RuntimeVersionSuggestionProvider<NetFrameworkVersion>))]
        public string NetFrameworkVersion { get; set; }

        [Persistent]
        [DisplayName("PHP version")]
        [ScriptAlias("PhpVersion")]
        [Category("Runtimes")]
        [SuggestibleValue(typeof(RuntimeVersionSuggestionProvider<PhpVersion>))]
        public string PhpVersion { get; set; }

        [Persistent]
        [DisplayName("Python version")]
        [ScriptAlias("PythonVersion")]
        [Category("Runtimes")]
        [SuggestibleValue(typeof(RuntimeVersionSuggestionProvider<PythonVersion>))]
        public string PythonVersion { get; set; }

        [Persistent]
        [DisplayName("Java version")]
        [Description("If this is not null, Java web container must be set and other runtimes are disabled.")]
        [ScriptAlias("JavaVersion")]
        [Category("Runtimes")]
        [SuggestibleValue(typeof(RuntimeVersionSuggestionProvider<JavaVersion>))]
        public string JavaVersion { get; set; }

        [Persistent]
        [DisplayName("Java web container")]
        [ScriptAlias("WebContainer")]
        [Category("Runtimes")]
        [SuggestibleValue(typeof(RuntimeVersionSuggestionProvider<WebContainer>))]
        public string WebContainer { get; set; }

        [Persistent]
        [DisplayName("Exists")]
        [Description(CommonDescriptions.Exists)]
        [ScriptAlias("Exists")]
        [DefaultValue(true)]
        public bool Exists { get; set; } = true;

#if Otter
        public override ComparisonResult Compare(PersistedConfiguration other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            var config = other as WebAppConfiguration;
            if (config == null)
            {
                throw new InvalidOperationException("Cannot compare configurations of different types");
            }

            if (!this.Exists && !config.Exists)
            {
                return ComparisonResult.Identical;
            }

            if (this.Exists || config.Exists)
            {
                return new ComparisonResult(new[] { new Difference(nameof(Exists), this.Exists, config.Exists) });
            }

            var differences = new List<Difference>();

            if (!string.Equals(this.PlanName, config.PlanName, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add(new Difference(nameof(PlanName), this.PlanName, config.PlanName));
            }

            if (this.AlwaysOn.HasValue && this.AlwaysOn != config.AlwaysOn)
            {
                differences.Add(new Difference(nameof(AlwaysOn), this.AlwaysOn, config.AlwaysOn));
            }

            if (this.WebSockets.HasValue && this.WebSockets != config.WebSockets)
            {
                differences.Add(new Difference(nameof(WebSockets), this.WebSockets, config.WebSockets));
            }

            if (!string.IsNullOrEmpty(this.NetFrameworkVersion) && !string.Equals(this.NetFrameworkVersion, config.NetFrameworkVersion, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add(new Difference(nameof(NetFrameworkVersion), this.NetFrameworkVersion, config.NetFrameworkVersion));
            }

            if (!string.IsNullOrEmpty(this.PhpVersion) && !string.Equals(this.PhpVersion, config.PhpVersion, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add(new Difference(nameof(PhpVersion), this.PhpVersion, config.PhpVersion));
            }

            if (!string.IsNullOrEmpty(this.JavaVersion) && !string.Equals(this.JavaVersion, config.JavaVersion, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add(new Difference(nameof(JavaVersion), this.JavaVersion, config.JavaVersion));
            }

            if (!string.IsNullOrEmpty(this.JavaVersion) && !Microsoft.Azure.Management.AppService.Fluent.JavaVersion.Off.Equals(this.JavaVersion) && !string.IsNullOrEmpty(this.WebContainer) && !string.Equals(this.WebContainer, config.WebContainer, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add(new Difference(nameof(WebContainer), this.WebContainer, config.WebContainer));
            }

            if (!string.IsNullOrEmpty(this.PythonVersion) && !string.Equals(this.PythonVersion, config.PythonVersion, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add(new Difference(nameof(PythonVersion), this.PythonVersion, config.PythonVersion));
            }

            return new ComparisonResult(differences);
        }
#endif
    }
}
