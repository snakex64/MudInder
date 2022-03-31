using System.Text.Json;

namespace MudInder.AzureFunctions.Core
{
    public class AzureFunctionsClient
    {
        private HttpClient HttpClient = new HttpClient();

        public AzureFunctionsClient(string baseUrl)
        {
            HttpClient.BaseAddress = new Uri(baseUrl);
        }

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

            var result = await HttpClient.PostAsync("/api/login", new StringContent(JsonSerializer.Serialize(loginArgs)));

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                return new LoginReturn();

            var loginReturn = await JsonSerializer.DeserializeAsync<LoginReturn>(result.Content.ReadAsStream());

            return loginReturn ?? new LoginReturn();
        }

        public async Task<LoginReturn> Signup(LoginArgs loginArgs)
        {

            var result = await HttpClient.PostAsync("/api/signup", new StringContent(JsonSerializer.Serialize(loginArgs)));

            if (result.StatusCode != System.Net.HttpStatusCode.OK)
                return new LoginReturn();

            var loginReturn = await JsonSerializer.DeserializeAsync<LoginReturn>(result.Content.ReadAsStream());

            return loginReturn ?? new LoginReturn();
        }

        #endregion
    }
}