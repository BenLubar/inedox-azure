#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Web.Controls;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Web.Controls;
#endif
using Microsoft.Azure.Management.Resource.Fluent.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Inedo.Extensions.Azure.SuggestionProviders
{
    public sealed class RuntimeVersionSuggestionProvider<TVersion> : ISuggestionProvider where TVersion : ExpandableStringEnum<TVersion>, new()
    {
        private static readonly IEnumerable<string> Versions = typeof(TVersion).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(TVersion)).Select(f => ((TVersion)f.GetValue(null)).Value);

        public Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            return Task.FromResult(Versions);
        }
    }
}
