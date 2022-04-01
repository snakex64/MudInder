using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MudInder.AzureFunctions
{
    internal static class UtilityExtensions
    {
        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this FeedIterator<T> iterator, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while(iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);

                foreach(var item in response)
                    yield return item;
            }
        }

    }
}
