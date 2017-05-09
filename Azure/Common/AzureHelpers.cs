using Microsoft.Azure.Management.Resource.Fluent.Core;
using Microsoft.Azure.Management.Resource.Fluent.Core.CollectionActions;
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

        internal static async Task<IEnumerable<T>> GetAllPagesAsync<T>(Func<PagedList<T>> getList, CancellationToken cancellationToken)
        {
            return await Task.Run(() => GetAllPages(getList), cancellationToken).ConfigureAwait(false);
        }

        private static IEnumerable<T> GetAllPages<T>(Func<PagedList<T>> getList)
        {
            var list = getList();
            list.LoadAll();
            return list;
        }

        internal static async Task<T> TryGetByNameAsync<T>(this ISupportsGettingByName<T> getter, string name, CancellationToken cancellationToken)
        {
            try
            {
                return await getter.GetByNameAsync(name, cancellationToken).ConfigureAwait(false);
            }
            catch (CloudException ex) when (ex.Body.Code.EndsWith("NotFound"))
            {
                return default(T);
            }
        }

        internal static async Task<T> TryGetByGroupAsync<T>(this ISupportsGettingByGroup<T> getter, string resourceGroupName, string name, CancellationToken cancellationToken)
        {
            try
            {
                return await getter.GetByGroupAsync(resourceGroupName, name, cancellationToken).ConfigureAwait(false);
            }
            catch (CloudException ex) when (ex.Body.Code.EndsWith("NotFound"))
            {
                return default(T);
            }
        }
    }
}
