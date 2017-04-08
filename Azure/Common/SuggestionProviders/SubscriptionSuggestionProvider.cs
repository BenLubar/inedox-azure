#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Web.Controls;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Web.Controls;
#endif
using Inedo.Extensions.Azure.Credentials;
using Microsoft.Azure.Management.Resource.Fluent;
using Microsoft.Rest;
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
            using (var client = new SubscriptionClient(new BasicAuthenticationCredentials
            {
                UserName = config[nameof(AzureCredentials.UserName)],
                Password = config[nameof(AzureCredentials.Password)]
            }))
            {
                var subscriptions = await AzureHelpers.GetAllPagesAsync(client.Subscriptions.ListAsync, client.Subscriptions.ListNextAsync, CancellationToken.None).ConfigureAwait(false);
                return subscriptions.Select(s => s.SubscriptionId);
            }
        }
    }
}
