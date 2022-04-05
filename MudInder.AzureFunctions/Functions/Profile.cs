using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text.Json;
using MudInder.AzureFunctions.Core;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using Microsoft.Azure.Cosmos.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MudInder.AzureFunctions.Functions;

public static class ProfileFunctions
{
    [FunctionName("updateprofile")]
    public static async Task<IActionResult> UpdateProfile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, [CosmosDB(Connection = "ConnectionString")] CosmosClient client)
    {
        var claims = req.ValidateAuth();

        var name = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception();

        var args = await JsonSerializer.DeserializeAsync<AzureFunctionsClient.UpdateProfileArgs>(req.Body);
        if (args == null)
            return new NoContentResult();

        var container = client.GetDatabase("DB").GetContainer(nameof(Model.UserInfo));
        var user = await container.GetItemLinqQueryable<Model.UserInfo>().Where(x => x.UserName == name).Take(1).ToFeedIterator().ToAsyncEnumerable().FirstOrDefaultAsync();

        if (user == null)
            return new NotFoundObjectResult("");


        // just delete all the previous images, no fancy (or smart) thing going on here:
        for (int i = 0; i < (user.Profile?.NbImages ?? 0); ++i)
            await container.DeleteItemAsync<Model.UserProfileImage>(user.id.ToString() + i, new PartitionKey(user.id.ToString() + i)); // clean, I know. Lazy

        user.Profile = new Model.Profile()
        {
            Age = args.ProfileInfo.Age,
            Description = args.ProfileInfo.Description,
            DisplayedName = args.ProfileInfo.DisplayedName,
            NbImages = args.Images.Count
        };
        
        foreach ( var image in args.Images)
        {
            await container.UpsertItemAsync(new Model.UserProfileImage()
            {
                id = user.id.ToString() + image.Key,
                Data = image.Value,
                Index = image.Key,
                UserId = user.id
            });
        }

        await container.ReplaceItemAsync(user, user.id.ToString());

        return new OkObjectResult(JsonSerializer.Serialize(new AzureFunctionsClient.UpdateProfileResult()));
    }


    [FunctionName("getmyprofile")]
    public static async Task<IActionResult> GetMyProfile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, [CosmosDB(Connection = "ConnectionString")] CosmosClient client)
    {
        var claims = req.ValidateAuth();

        var name = claims.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new Exception();

        var args = await JsonSerializer.DeserializeAsync<AzureFunctionsClient.GetMyProfileArgs>(req.Body);
        if (args == null)
            return new NoContentResult();

        var container = client.GetDatabase("DB").GetContainer(nameof(Model.UserInfo));
        var user = await container.GetItemLinqQueryable<Model.UserInfo>().Where(x => x.UserName == name).Take(1).ToFeedIterator().ToAsyncEnumerable().FirstOrDefaultAsync();

        if (user?.Profile == null)
            return new NoContentResult();

        return new OkObjectResult(JsonSerializer.Serialize(new AzureFunctionsClient.ProfileInfo()
        {
            Age = user.Profile.Age,
            Description = user.Profile.Description,
            DisplayedName = user.Profile.DisplayedName
        }));
    }

}