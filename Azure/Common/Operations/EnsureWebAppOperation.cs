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
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Azure.Management.AppService.Fluent.WebAppBase.Definition;
using Microsoft.Azure.Management.AppService.Fluent.WebAppBase.Update;

namespace Inedo.Extensions.Azure.Operations.AppService
{
    [DisplayName("Ensure Web App")]
    [ScriptAlias("Ensure-Web-App")]
    [ScriptNamespace("Azure")]
    public sealed class EnsureWebAppOperation : EnsureOperation<WebAppConfiguration>
    {
#if Otter
        public override async Task<PersistedConfiguration> CollectAsync(IOperationExecutionContext context)
        {
            var manager = this.Template.Azure.AppServices;

            var webApp = await manager.WebApps.TryGetByGroupAsync(this.Template.ResourceGroup, this.Template.Name, context.CancellationToken).ConfigureAwait(false);
            if (webApp == null)
            {
                return new WebAppConfiguration
                {
                    ResourceGroup = this.Template.ResourceGroup,
                    Name = this.Template.Name,
                    Exists = false
                };
            }

            var plan = await manager.AppServicePlans.GetByIdAsync(webApp.AppServicePlanId, context.CancellationToken).ConfigureAwait(false);

            return new WebAppConfiguration
            {
                ResourceGroup = this.Template.ResourceGroup,
                PlanName = plan.Name,
                Name = this.Template.Name,
                Exists = true,
                AlwaysOn = webApp.AlwaysOn,
                WebSockets = webApp.WebSocketsEnabled,
                NetFrameworkVersion = webApp.NetFrameworkVersion.Value,
                PhpVersion = webApp.PhpVersion.Value,
                JavaVersion = webApp.JavaVersion.Value,
                WebContainer = $"{webApp.JavaContainer} {webApp.JavaContainerVersion}",
                PythonVersion = webApp.PythonVersion.Value
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

                    if (resourceGroup == null)
                    {
                        this.LogError("Resource group does not exist. Use Azure::Ensure-AppService-Plan to ensure it exists.");
                        return;
                    }
                }

                this.LogDebug($"Getting app service plan {this.Template.PlanName}...");
                IAppServicePlan plan = null;
                if (!context.Simulation)
                {
                    plan = await manager.AppServicePlans.TryGetByGroupAsync(this.Template.ResourceGroup, this.Template.PlanName, context.CancellationToken).ConfigureAwait(false);

                    if (plan == null)
                    {
                        this.LogError("AppService plan does not exist. Use Azure::Ensure-AppService-Plan to ensure it exists.");
                        return;
                    }
                }

                IWebApp webApp = null;
                if (!context.Simulation)
                {
                    webApp = await manager.WebApps.TryGetByGroupAsync(this.Template.ResourceGroup, this.Template.Name, context.CancellationToken).ConfigureAwait(false);
                }

                if (webApp == null)
                {
                    this.LogDebug("Web app does not exist. Creating...");
                }
                else
                {
                    this.LogDebug("Updating web app...");

                    IUpdate<IWebApp> updater = webApp.Update().WithExistingAppServicePlan(plan);

                    if (!string.IsNullOrEmpty(this.Template.NetFrameworkVersion))
                    {
                        updater = updater.WithNetFrameworkVersion(NetFrameworkVersion.Parse(this.Template.NetFrameworkVersion));
                    }

                    if (!string.IsNullOrEmpty(this.Template.PhpVersion))
                    {
                        updater = updater.WithPhpVersion(PhpVersion.Parse(this.Template.PhpVersion));
                    }

                    if (!string.IsNullOrEmpty(this.Template.PythonVersion))
                    {
                        updater = updater.WithPythonVersion(PythonVersion.Parse(this.Template.PythonVersion));
                    }

                    if (!string.IsNullOrEmpty(this.Template.JavaVersion))
                    {
                        updater = updater
                            .WithJavaVersion(JavaVersion.Parse(this.Template.JavaVersion))
                            .WithWebContainer(WebContainer.Parse(this.Template.WebContainer));
                    }

                    if (this.Template.AlwaysOn.HasValue)
                    {
                        updater = updater.WithWebAppAlwaysOn(this.Template.AlwaysOn.Value);
                    }

                    if (this.Template.WebSockets.HasValue)
                    {
                        updater = updater.WithWebSocketsEnabled(this.Template.WebSockets.Value);
                    }

                    if (!context.Simulation)
                    {
                        await updater.ApplyAsync(context.CancellationToken).ConfigureAwait(false);
                    }

                    return;
                }

                if (context.Simulation)
                {
                    return;
                }

                var builderDomains = manager.WebApps.Define(this.Template.Name).
                    WithExistingResourceGroup(resourceGroup).
                    WithExistingAppServicePlan(plan);

                IWithCreate<IWebApp> builder = builderDomains;

                if (!string.IsNullOrEmpty(this.Template.NetFrameworkVersion))
                {
                    builder = builder.WithNetFrameworkVersion(NetFrameworkVersion.Parse(this.Template.NetFrameworkVersion));
                }

                if (!string.IsNullOrEmpty(this.Template.PhpVersion))
                {
                    builder = builder.WithPhpVersion(PhpVersion.Parse(this.Template.PhpVersion));
                }

                if (!string.IsNullOrEmpty(this.Template.PythonVersion))
                {
                    builder = builder.WithPythonVersion(PythonVersion.Parse(this.Template.PythonVersion));
                }

                if (!string.IsNullOrEmpty(this.Template.JavaVersion))
                {
                    builder = builder
                        .WithJavaVersion(JavaVersion.Parse(this.Template.JavaVersion))
                        .WithWebContainer(WebContainer.Parse(this.Template.WebContainer));
                }

                if (this.Template.AlwaysOn.HasValue)
                {
                    builder = builder.WithWebAppAlwaysOn(this.Template.AlwaysOn.Value);
                }

                if (this.Template.WebSockets.HasValue)
                {
                    builder = builder.WithWebSocketsEnabled(this.Template.WebSockets.Value);
                }

                await builder.CreateAsync(context.CancellationToken).ConfigureAwait(false);
            }
            else
            {
                this.LogDebug($"Deleting web app plan {this.Template.ResourceGroup} :: {this.Template.Name}...");
                if (!context.Simulation)
                {
                    await manager.WebApps.DeleteByGroupAsync(this.Template.ResourceGroup, this.Template.Name, context.CancellationToken).ConfigureAwait(false);
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
