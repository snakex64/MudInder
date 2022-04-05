using System.Text.Json;

namespace MudInder.AzureFunctions.Core
{
    public class AzureFunctionsClient
    {
        private HttpClient HttpClient = new HttpClient();

        public string? Token { get; set; }

        public AzureFunctionsClient(string baseUrl)
        {
            HttpClient.BaseAddress = new Uri(baseUrl);
        }

        #region DoRequest

        private async Task<TReturn?> DoRequest<TArgs, TReturn>(string url, TArgs args)
        {
            // add bearer authorization
            if (Token != null)
                HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

            var result = await HttpClient.PostAsync(url, new StringContent(JsonSerializer.Serialize(args)));

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                return default;

            return await JsonSerializer.DeserializeAsync<TReturn>(result.Content.ReadAsStream());
        }

        #endregion


        #region UpdateProfile / GetMyProfile 

        public class UpdateProfileResult
        {
            
        }

        public class UpdateProfileArgs
        {
            public ProfileInfo ProfileInfo { get; set; }

            public Dictionary<int, byte[]> Images { get; set; }
        }
        public class ProfileInfo
        {
            public string DisplayedName { get; set; } = null!;
            
            public int Age { get; set; }

            public string Description { get; set; } = null!;
        }

        public async Task<UpdateProfileResult?> UpdateProfile(UpdateProfileArgs args)
        {
            var result = await DoRequest<UpdateProfileArgs, UpdateProfileResult>("/api/updateprofile", args);

            return result;
        }
        
        public class GetMyProfileArgs
        {
        }
        
        public async Task<ProfileInfo?> GetMyProfile(GetMyProfileArgs args)
        {
            var result = await DoRequest<GetMyProfileArgs, ProfileInfo>("/api/getmyprofile", args);

            return result;
        }


        #endregion

        #region Login / Signup 

        public class LoginReturn
        {
            public bool Success { get; set; }

            public string? Token { get; set; }
        }
        public class LoginArgs
        {
            public string? Name { get; set; }

            public string? Password { get; set; }
        }
        public async Task<LoginReturn> Login(LoginArgs loginArgs)
        {
            var result = await DoRequest<LoginArgs, LoginReturn>("/api/login", loginArgs);

            Token = result?.Token;

            return result ?? new LoginReturn();
        }

        public async Task<LoginReturn> Signup(LoginArgs loginArgs)
        {
            var result = await DoRequest<LoginArgs, LoginReturn>("/api/signup", loginArgs);

            Token = result?.Token;
            
            return result ?? new LoginReturn();
        }

        #endregion
    }
}