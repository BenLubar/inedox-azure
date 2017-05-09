#if BuildMaster
using Inedo.BuildMaster.Extensibility;
using Inedo.BuildMaster.Extensibility.Credentials;
using Inedo.BuildMaster.Web.Controls;
#elif Otter
using Inedo.Otter.Extensibility;
using Inedo.Otter.Extensibility.Credentials;
using Inedo.Otter.Web.Controls;
#endif
using Inedo.Extensions.Azure.Credentials;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inedo.Extensions.Azure.SuggestionProviders
{
    public sealed class ResourceGroupSuggestionProvider : ISuggestionProvider
    {
        public async Task<IEnumerable<string>> GetSuggestionsAsync(IComponentConfiguration config)
        {
            var credentials = ResourceCredentials.Create<AzureCredentials>(config[nameof(IHasCredentials<AzureCredentials>.CredentialName)]);
            if (credentials == null)
            {
                return Enumerable.Empty<string>();
            }

            var resourceGroups = await AzureHelpers.GetAllPagesAsync(credentials.Azure.ResourceGroups.List, CancellationToken.None).ConfigureAwait(false);
            return resourceGroups.Select(g => g.Name);
        }
    }
}
