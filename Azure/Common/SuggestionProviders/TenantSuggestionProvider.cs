#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Web.Controls;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Web.Controls;
#endif
using Inedo.Extensions.Azure.Credentials;
using Microsoft.Azure.Management.Resource.Fluent;
using Microsoft.Rest.Azure.Authentication;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inedo.Extensions.Azure.SuggestionProviders
{
    public sealed class TenantSuggestionProvider : ISuggestionProvider
    {
        public async Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            var credentials = AzureCredentials.FromComponentConfiguration(config);
            var token = await ApplicationTokenProvider.LoginSilentAsync(credentials.DomainName, credentials.ClientId, credentials.ClientSecret).ConfigureAwait(false);
            using (var client = new SubscriptionClient(token))
            {
                var tenants = await AzureHelpers.GetAllPagesAsync(client.Tenants.ListAsync, client.Tenants.ListNextAsync, CancellationToken.None).ConfigureAwait(false);
                return tenants.Select(t => t.TenantId);
            }
        }
    }
}
