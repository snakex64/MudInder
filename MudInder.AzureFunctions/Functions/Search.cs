using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using MudInder.AzureFunctions.Core;
using Microsoft.Azure.Cosmos;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;

namespace MudInder.AzureFunctions.Functions
{
    public static class Search
    {

        //[FunctionName("login")]
        //public static async Task<IActionResult> Login(
        //    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, [CosmosDB(Connection = "ConnectionString")] CosmosClient client)
        //{
        //    var args = await JsonSerializer.DeserializeAsync<AzureFunctionsClient.LoginArgs>(req.Body);
        //    if (args?.Name == null || args.Password == null)
        //        return new NotFoundObjectResult(null);
        //
        //    var container = client.GetDatabase("DB").GetContainer(nameof(Model.UserInfo));
        //
        //    var user = await container.GetItemLinqQueryable<Model.UserInfo>().Where(x => x.UserName == args.Name).Take(1).ToFeedIterator().ToAsyncEnumerable().FirstOrDefaultAsync();
        //
        //}
    }
}
