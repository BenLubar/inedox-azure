#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Web.Controls;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Web.Controls;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Inedo.Extensions.Azure.SuggestionProviders
{
    public sealed class AzureEnumSuggestionProvider<TEnum> : ISuggestionProvider
    {
        private static readonly IEnumerable<string> Names = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(TEnum)).Select(f => f.Name);
        internal static readonly IReadOnlyDictionary<string, TEnum> Lookup = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(TEnum)).ToDictionary(f => f.Name, f => (TEnum)f.GetValue(null), StringComparer.OrdinalIgnoreCase);
        internal static readonly IReadOnlyDictionary<TEnum, string> Reverse = typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(TEnum)).ToDictionary(f => (TEnum)f.GetValue(null), f => f.Name);

        public Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            return Task.FromResult(Names);
        }
    }
}
