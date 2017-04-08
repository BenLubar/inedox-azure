using Microsoft.Rest.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Inedo.Extensions.Azure
{
    internal static class AzureHelpers
    {
        internal static async Task<IEnumerable<T>> GetAllPagesAsync<T>(Func<CancellationToken, Task<IPage<T>>> first, Func<string, CancellationToken, Task<IPage<T>>> next, CancellationToken cancellationToken)
        {
            var all = Enumerable.Empty<T>();
            var page = await first(cancellationToken).ConfigureAwait(false);
            while (true)
            {
                all = all.Concat(page);
                if (page.NextPageLink == null)
                {
                    return all;
                }
                page = await next(page.NextPageLink, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
