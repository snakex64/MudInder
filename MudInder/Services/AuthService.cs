using MudInder.AzureFunctions.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudInder.Services
{
    internal class AuthService
    {
        public AuthService(AzureFunctionsClient azureFunctionsClient)
        {
            AzureFunctionsClient = azureFunctionsClient;
        }

        public AzureFunctionsClient AzureFunctionsClient { get; set; }

        public AzureFunctionsClient.ProfileInfo? ProfileInfo { get; set; }

        public bool IsAuthenticated => Token != null;

        public string? Token { get; private set; }

        public async Task SetToken(string token)
        {
            Token = token;

            ProfileInfo = await AzureFunctionsClient.GetMyProfile(new AzureFunctionsClient.GetMyProfileArgs());
        }
    }
}
