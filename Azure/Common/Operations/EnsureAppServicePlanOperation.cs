#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Configurations;
using Inedo.BuildMaster.Extensibility.Operations;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Configurations;
using Inedo.Otter.Extensibility.Operations;
#endif
using Inedo.Diagnostics;
using Inedo.Documentation;
using Inedo.Extensions.Azure.Configurations;
using Inedo.Extensions.Azure.SuggestionProviders;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Resource.Fluent;
using Microsoft.Azure.Management.Resource.Fluent.Core;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Inedo.Extensions.Azure.Operations.AppService
{
    [DisplayName("Ensure AppService Plan")]
    [ScriptAlias("Ensure-AppService-Plan")]
    [ScriptNamespace("Azure")]
    public sealed class EnsureAppServicePlanOperation : EnsureOperation<AppServicePlanConfiguration>
    {
#if Otter
        public override async Task<PersistedConfiguration> CollectAsync(IOperationExecutionContext context)
        {
            var manager = this.Template.Azure.AppServices;

            var plan = await manager.AppServicePlans.TryGetByGroupAsync(this.Template.ResourceGroup, this.Template.Name, context.CancellationToken).ConfigureAwait(false);
            if (plan == null)
            {
                return new AppServicePlanConfiguration
                {
                    ResourceGroup = this.Template.ResourceGroup,
                    Name = this.Template.Name,
                    Exists = false
                };
            }

            return new AppServicePlanConfiguration
            {
                ResourceGroup = this.Template.ResourceGroup,
                Name = this.Template.Name,
                Exists = true,
                Region = AzureEnumSuggestionProvider<Region>.Reverse[plan.Region],
                PricingTier = AzureEnumSuggestionProvider<AppServicePricingTier>.Reverse[plan.PricingTier],
                Capacity = plan.Capacity,
                PerSiteScaling = plan.PerSiteScaling
            };
        }
#endif

        public override async Task ConfigureAsync(IOperationExecutionContext context)
        {
            var manager = this.Template.Azure.AppServices;

            if (this.Template.Exists)
            {
                this.LogDebug($"Getting resource group {this.Template.ResourceGroup}...");
                IResourceGroup resourceGroup = null;
                if (!context.Simulation)
                {
                    resourceGroup = await manager.ResourceManager.ResourceGroups.TryGetByNameAsync(this.Template.ResourceGroup, context.CancellationToken).ConfigureAwait(false);
                }

                if (resourceGroup == null)
                {
                    this.LogDebug("Resource group does not exist. Creating...");
                    if (!context.Simulation)
                    {
                        resourceGroup = await manager.ResourceManager.ResourceGroups
                            .Define(this.Template.ResourceGroup)
                            .WithRegion(AzureEnumSuggestionProvider<Region>.Lookup[this.Template.Region])
                            .CreateAsync(context.CancellationToken).ConfigureAwait(false);
                    }
                }

                this.LogDebug($"Getting app service plan {this.Template.Name}...");
                IAppServicePlan plan = null;
                if (!context.Simulation)
                {
                    plan = await manager.AppServicePlans.TryGetByGroupAsync(this.Template.ResourceGroup, this.Template.Name, context.CancellationToken).ConfigureAwait(false);
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

                        var updater = plan.Update().WithPricingTier(AzureEnumSuggestionProvider<AppServicePricingTier>.Lookup[this.Template.PricingTier]);

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
                        await manager.AppServicePlans.DeleteByGroupAsync(this.Template.ResourceGroup, this.Template.Name, context.CancellationToken).ConfigureAwait(false);
                    }

                    this.LogDebug("Re-creating...");
                }

                if (context.Simulation)
                {
                    return;
                }

                var builder = manager.AppServicePlans.Define(this.Template.Name)
                    .WithRegion(AzureEnumSuggestionProvider<Region>.Lookup[this.Template.Region])
                    .WithExistingResourceGroup(resourceGroup)
                    .WithPricingTier(AzureEnumSuggestionProvider<AppServicePricingTier>.Lookup[this.Template.PricingTier]);

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
                    await manager.AppServicePlans.DeleteByGroupAsync(this.Template.ResourceGroup, this.Template.Name, context.CancellationToken).ConfigureAwait(false);
                }
            }
        }

        protected override ExtendedRichDescription GetDescription(IOperationConfiguration config)
        {
            var extended = new RichDescription();

            bool exists;
            if (bool.TryParse(config[nameof(AppServicePlanConfiguration.Exists)], out exists) && !exists)
            {
                extended.AppendContent("does not exist");
            }
            else
            {
                if (!string.IsNullOrEmpty(config[nameof(AppServicePlanConfiguration.Region)]))
                {
                    extended.AppendContent(" in region ", new Hilite(config[nameof(AppServicePlanConfiguration.Region)]));
                }

                if (!string.IsNullOrEmpty(config[nameof(AppServicePlanConfiguration.PricingTier)]))
                {
                    extended.AppendContent(" with pricing tier ", new Hilite(config[nameof(AppServicePlanConfiguration.PricingTier)]));
                }

                if (!string.IsNullOrEmpty(config[nameof(AppServicePlanConfiguration.Capacity)]))
                {
                    extended.AppendContent(" with capacity ", new Hilite(config[nameof(AppServicePlanConfiguration.Capacity)]));
                }

                bool perSiteScaling;
                if (bool.TryParse(config[nameof(AppServicePlanConfiguration.PerSiteScaling)], out perSiteScaling))
                {
                    extended.AppendContent(perSiteScaling ? " with per-site scaling" : " without per-site scaling");
                }
            }

            return new ExtendedRichDescription(
                new RichDescription("Ensure app service ", new Hilite(config[nameof(AppServicePlanConfiguration.ResourceGroup)]), " :: ", new Hilite(config[nameof(AppServicePlanConfiguration.Name)])),
                extended
            );
        }
    }
}
