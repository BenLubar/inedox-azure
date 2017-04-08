#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Configurations;
using Inedo.BuildMaster.Web.Controls;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Configurations;
using Inedo.Otter.Web.Controls;
#endif
using Inedo.Documentation;
using Inedo.Extensions.Azure.SuggestionProviders;
using Inedo.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Inedo.Extensions.Azure.Configurations
{
    public sealed class AppServiceConfiguration : PersistedConfiguration, IExistential
    {
        [Persistent]
        [Required]
        [DisplayName("Resource group")]
        [ScriptAlias("ResourceGroup")]
        public string ResourceGroup { get; set; }

        [Persistent]
        [Required]
        [DisplayName("Plan name")]
        [ScriptAlias("Name")]
        public string Name { get; set; }

        [Persistent]
        [Required]
        [DisplayName("Region")]
        [ScriptAlias("Region")]
        [SuggestibleValue(typeof(RegionSuggestionProvider))]
        public string Region { get; set; }

        [Persistent]
        [Required]
        [DisplayName("Pricing tier")]
        [ScriptAlias("PricingTier")]
        [SuggestibleValue(typeof(AppServicePricingTierSuggestionProvider))]
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
        [ScriptAlias("Exists")]
        [DefaultValue(true)]
        public bool Exists { get; set; } = true;

#if Otter
        public override string ConfigurationKey => this.ResourceGroup + " :: " + this.Name;

        public override ComparisonResult Compare(PersistedConfiguration other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            var config = other as AppServiceConfiguration;
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
