#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Credentials;
using Inedo.BuildMaster.Extensibility.Operations;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Operations;
using Inedo.Otter.Extensibility.Credentials;
using Inedo.Otter.Extensibility.Configurations;
#endif
using Inedo.Diagnostics;
using Inedo.Documentation;
using Inedo.Extensions.Azure.Configurations;
using Inedo.Extensions.Azure.Credentials;
using Inedo.Extensions.Azure.SuggestionProviders;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Resource.Fluent;
using Microsoft.Azure.Management.Resource.Fluent.Core;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Inedo.Extensions.Azure.Operations.AppService
{
    [DisplayName("Ensure AppService")]
    [ScriptAlias("Ensure-App-Service")]
    public sealed class EnsureAppServiceOperation : EnsureOperation<AppServiceConfiguration>, IHasCredentials<AzureCredentials>
    {
        [DisplayName("Credentials")]
        [ScriptAlias("Credentials")]
        public string CredentialName { get; set; }

        private IAppServiceManager Manager
        {
            get
            {
                var credentials = this.TryGetCredentials();
                return AppServiceManager.Authenticate(credentials.Credentials, credentials.SubscriptionId);
            }
        }

#if Otter
        public override async Task<PersistedConfiguration> CollectAsync(IOperationExecutionContext context)
        {
            var plan = await this.Manager.AppServicePlans.GetByGroupAsync(this.Template.ResourceGroup, this.Template.Name, context.CancellationToken).ConfigureAwait(false);
            if (plan == null)
            {
                return new AppServiceConfiguration
                {
                    ResourceGroup = this.Template.ResourceGroup,
                    Name = this.Template.Name,
                    Exists = false
                };
            }

            return new AppServiceConfiguration
            {
                ResourceGroup = this.Template.ResourceGroup,
                Name = this.Template.Name,
                Exists = true,
                Region = plan.RegionName,
                PricingTier = AppServicePricingTierSuggestionProvider.Reverse[plan.PricingTier],
                Capacity = plan.Capacity,
                PerSiteScaling = plan.PerSiteScaling
            };
        }
#endif

        public override async Task ConfigureAsync(IOperationExecutionContext context)
        {
            if (this.Template.Exists)
            {
                this.LogDebug($"Getting resource group {this.Template.ResourceGroup}...");
                IResourceGroup resourceGroup = null;
                if (!context.Simulation)
                {
                    resourceGroup = await this.Manager.ResourceManager.ResourceGroups.GetByNameAsync(this.Template.ResourceGroup).ConfigureAwait(false);
                }

                if (resourceGroup == null)
                {
                    this.LogDebug("Resource group does not exist. Creating...");
                    if (!context.Simulation)
                    {
                        resourceGroup = await this.Manager.ResourceManager.ResourceGroups.Define(this.Template.ResourceGroup).WithRegion(Region.Create(this.Template.Region)).CreateAsync(context.CancellationToken).ConfigureAwait(false);
                    }
                }

                this.LogDebug($"Getting app service plan {this.Template.Name}...");
                IAppServicePlan plan = null;
                if (!context.Simulation)
                {
                    plan = await this.Manager.AppServicePlans.GetByGroupAsync(this.Template.ResourceGroup, this.Template.Name, context.CancellationToken).ConfigureAwait(false);
                }

                if (plan == null)
                {
                    this.LogDebug("Plan does not exist. Creating...");
                }
                else
                {
                    if (!string.Equals(plan.RegionName, this.Template.Region, StringComparison.OrdinalIgnoreCase))
                    {
                        this.LogDebug("Updating plan...");

                        var updater = plan.Update().WithPricingTier(AppServicePricingTierSuggestionProvider.Tiers[this.Template.PricingTier]);

                        if (this.Template.Capacity.HasValue)
                        {
                            updater = updater.WithCapacity(this.Template.Capacity.Value);
                        }

                        if (this.Template.PerSiteScaling.HasValue)
                        {
                            updater = updater.WithPerSiteScaling(this.Template.PerSiteScaling.Value);
                        }

                        if (!context.Simulation)
                        {
                            await updater.ApplyAsync(context.CancellationToken).ConfigureAwait(false);
                        }

                        return;
                    }

                    this.LogDebug("Plan region differs. Deleting plan...");
                    if (!context.Simulation)
                    {
                        await this.Manager.AppServicePlans.DeleteByGroupAsync(this.Template.ResourceGroup, this.Template.Name, context.CancellationToken).ConfigureAwait(false);
                    }

                    this.LogDebug("Re-creating...");
                }

                if (context.Simulation)
                {
                    return;
                }

                var builder = this.Manager.AppServicePlans.Define(this.Template.Name).WithRegion(Region.Create(this.Template.Region)).WithExistingResourceGroup(resourceGroup).WithPricingTier(AppServicePricingTierSuggestionProvider.Tiers[this.Template.PricingTier]);

                if (this.Template.Capacity.HasValue)
                {
                    builder = builder.WithCapacity(this.Template.Capacity.Value);
                }

                if (this.Template.PerSiteScaling.HasValue)
                {
                    builder = builder.WithPerSiteScaling(this.Template.PerSiteScaling.Value);
                }

                await builder.CreateAsync(context.CancellationToken).ConfigureAwait(false);
            }
            else
            {
                this.LogDebug($"Deleting app service plan {this.Template.ResourceGroup} :: {this.Template.Name}...");
                if (!context.Simulation)
                {
                    await this.Manager.AppServicePlans.DeleteByGroupAsync(this.Template.ResourceGroup, this.Template.Name, context.CancellationToken).ConfigureAwait(false);
                }
            }
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            var extended = new RichDescription();

            bool exists;
            if (bool.TryParse(config[nameof(AppServiceConfiguration.Exists)], out exists) && !exists)
            {
                extended.AppendContent("does not exist");
            }
            else
            {
                if (!string.IsNullOrEmpty(config[nameof(AppServiceConfiguration.Region)]))
                {
                    extended.AppendContent(" in region ", new Hilite(config[nameof(AppServiceConfiguration.Region)]));
                }

                if (!string.IsNullOrEmpty(config[nameof(AppServiceConfiguration.PricingTier)]))
                {
                    extended.AppendContent(" with pricing tier ", new Hilite(config[nameof(AppServiceConfiguration.PricingTier)]));
                }

                if (!string.IsNullOrEmpty(config[nameof(AppServiceConfiguration.Capacity)]))
                {
                    extended.AppendContent(" with capacity ", new Hilite(config[nameof(AppServiceConfiguration.Capacity)]));
                }

                bool perSiteScaling;
                if (bool.TryParse(config[nameof(AppServiceConfiguration.PerSiteScaling)], out perSiteScaling))
                {
                    extended.AppendContent(perSiteScaling ? " with per-site scaling" : " without per-site scaling");
                }
            }

            return new ExtendedRichDescription(
                new RichDescription("Ensure app service ", new Hilite(config[nameof(AppServiceConfiguration.ResourceGroup)]), " :: ", new Hilite(config[nameof(AppServiceConfiguration.Name)])),
                extended
            );
        }
    }
}
