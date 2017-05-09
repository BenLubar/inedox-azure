#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Web.Controls;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Web.Controls;
#endif
using Inedo.Extensions.Azure.Credentials;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inedo.Extensions.Azure.SuggestionProviders
{
    public sealed class SubscriptionSuggestionProvider : ISuggestionProvider
    {
        public async Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            var credentials = AzureCredentials.FromComponentConfiguration(config);
            var subscriptions = await AzureHelpers.GetAllPagesAsync(credentials.Azure.Subscriptions.List, CancellationToken.None).ConfigureAwait(false);
            return subscriptions.Select(s => s.SubscriptionId);
        }
    }
}
