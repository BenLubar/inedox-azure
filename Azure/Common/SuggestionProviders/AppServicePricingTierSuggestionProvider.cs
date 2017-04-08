#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Web.Controls;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Web.Controls;
#endif
using Microsoft.Azure.Management.AppService.Fluent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Inedo.Extensions.Azure.SuggestionProviders
{
    public sealed class AppServicePricingTierSuggestionProvider : ISuggestionProvider
    {
        internal static readonly IReadOnlyDictionary<string, AppServicePricingTier> Tiers = typeof(AppServicePricingTier).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(AppServicePricingTier)).ToDictionary(t => t.Name, t => (AppServicePricingTier)t.GetValue(null));
        internal static readonly IReadOnlyDictionary<AppServicePricingTier, string> Reverse = Tiers.ToDictionary(t => t.Value, t => t.Key);

        public Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            return Task.FromResult(Tiers.Keys);
        }
    }
}
