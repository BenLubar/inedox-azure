﻿#if BuildMaster
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
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Resource.Fluent.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Inedo.Extensions.Azure.Configurations
{
    public sealed class AppServicePlanConfiguration : PersistedConfiguration, IExistential, IHasCredentials<AzureCredentials>
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
        [ConfigurationKey]
        [DisplayName("Plan name")]
        [ScriptAlias("Name")]
        public string Name { get; set; }

        [Persistent]
        [Required]
        [DisplayName("Region")]
        [ScriptAlias("Region")]
        [SuggestibleValue(typeof(AzureEnumSuggestionProvider<Region>))]
        public string Region { get; set; }

        [Persistent]
        [Required]
        [DisplayName("Pricing tier")]
        [ScriptAlias("PricingTier")]
        [SuggestibleValue(typeof(AzureEnumSuggestionProvider<AppServicePricingTier>))]
        public string PricingTier { get; set; }

        [Persistent]
        [DisplayName("Capacity")]
        [ScriptAlias("Capacity")]
        public int? Capacity { get; set; }

        [Persistent]
        [DisplayName("Per-site scaling")]
        [ScriptAlias("PerSiteScaling")]
        public bool? PerSiteScaling { get; set; }

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
            var config = other as AppServicePlanConfiguration;
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
            if (!string.Equals(this.Region, config.Region, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add(new Difference(nameof(Region), this.Region, config.Region));
            }

            if (!string.Equals(this.PricingTier, config.PricingTier, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add(new Difference(nameof(PricingTier), this.PricingTier, config.PricingTier));
            }

            if (this.Capacity.HasValue && config.Capacity.HasValue && this.Capacity.Value != config.Capacity.Value)
            {
                differences.Add(new Difference(nameof(Capacity), this.Capacity.Value, config.Capacity.Value));
            }

            if (this.PerSiteScaling.HasValue && config.PerSiteScaling.HasValue && this.PerSiteScaling.Value != config.PerSiteScaling.Value)
            {
                differences.Add(new Difference(nameof(PerSiteScaling), this.PerSiteScaling.Value, config.PerSiteScaling.Value));
            }

            return new ComparisonResult(differences);
        }
#endif
    }
}
