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

namespace MudInder.AzureFunctions.Functions
{
    public static class LoginFunctions
    {

        [FunctionName("login")]
        public static async Task<IActionResult> Login(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, [CosmosDB(Connection = "ConnectionString")] CosmosClient client)
        {
            var args = await JsonSerializer.DeserializeAsync<AzureFunctionsClient.LoginArgs>(req.Body);
            if (args?.Name == null || args.Password == null)
                return new NotFoundObjectResult(null);

            var container = client.GetDatabase("DB").GetContainer(nameof(Model.UserInfo));

            var user = await container.GetItemLinqQueryable<Model.UserInfo>().Where(x => x.UserName == args.Name).Take(1).ToFeedIterator().ToAsyncEnumerable().FirstOrDefaultAsync();
            if (user == null || !VerifyHashedPasswordV3(Convert.FromBase64String(user.Password), args.Password, out var _))
            {
                return new OkObjectResult(JsonSerializer.Serialize(new AzureFunctionsClient.LoginReturn()
                {
                    Success = false
                }));
            }

            return new OkObjectResult(JsonSerializer.Serialize(new AzureFunctionsClient.LoginReturn()
            {
                Success = true,
                Token = GenerateToken(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, args.Name)
                }, TimeSpan.FromDays(7))
            }));
        }

        [FunctionName("signup")]
        public static async Task<IActionResult> Signup(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, [CosmosDB(Connection = "ConnectionString")] CosmosClient client)
        {
            var args = await JsonSerializer.DeserializeAsync<AzureFunctionsClient.LoginArgs>(req.Body);
            if (args?.Name == null || args.Password == null)
                return new NotFoundObjectResult(null);

            var container = client.GetDatabase("DB").GetContainer(nameof(Model.UserInfo));

            var userExists = await container.GetItemLinqQueryable<Model.UserInfo>().Where(x => x.UserName == args.Name).Take(1).ToFeedIterator().ToAsyncEnumerable().AnyAsync();
            if (userExists)
            {
                return new OkObjectResult(JsonSerializer.Serialize(new AzureFunctionsClient.LoginReturn()
                {
                    Success = false
                }));
            }

            var passwordHash = Convert.ToBase64String(HashPasswordV3(args.Password));

            await container.CreateItemAsync(new Model.UserInfo()
            {
                id = Guid.NewGuid(),
                UserName = args.Name,
                Password = passwordHash
            });

            return new OkObjectResult(JsonSerializer.Serialize(new AzureFunctionsClient.LoginReturn()
            {
                Success = true,
                Token = GenerateToken(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, args.Name)
                }, TimeSpan.FromDays(7))
            }));
        }

        private static string? _Key;
        public static string Key
        {
            get
            {
                if (_Key == null)
                    _Key = Environment.GetEnvironmentVariable("TokenKeys") ?? throw new Exception("Unable to get TokenKeys");
                return _Key;
            }
        }

        private static string GenerateToken(IEnumerable<Claim> claims, TimeSpan? expiresIn)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresIn == null ? null : DateTime.UtcNow + expiresIn,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenStr = tokenHandler.WriteToken(token);

            return tokenStr;
        }

        #region Hashing, totally not stolen here https://github.com/dotnet/AspNetCore/blob/main/src/Identity/Extensions.Core/src/PasswordHasher.cs

        private static RNGCryptoServiceProvider RNG = new RNGCryptoServiceProvider();

        private static bool VerifyHashedPasswordV3(byte[] hashedPassword, string password, out int iterCount)
        {
            iterCount = default;

            try
            {
                // Read header information
                KeyDerivationPrf prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
                iterCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
                int saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

                // Read the salt: must be >= 128 bits
                if (saltLength < 128 / 8)
                {
                    return false;
                }
                byte[] salt = new byte[saltLength];
                Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);

                // Read the subkey (the rest of the payload): must be >= 128 bits
                int subkeyLength = hashedPassword.Length - 13 - salt.Length;
                if (subkeyLength < 128 / 8)
                {
                    return false;
                }
                byte[] expectedSubkey = new byte[subkeyLength];
                Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

                // Hash the incoming password and verify it
                byte[] actualSubkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, subkeyLength);
                return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
            }
            catch
            {
                // This should never occur except in the case of a malformed payload, where
                // we might go off the end of the array. Regardless, a malformed payload
                // implies verification failed.
                return false;
            }
        }

        private static byte[] HashPasswordV3(string password)
        {
            return HashPasswordV3(password, RNG,
                prf: KeyDerivationPrf.HMACSHA256,
                iterCount: 20,
                saltSize: 128 / 8,
                numBytesRequested: 256 / 8);
        }
        private static byte[] HashPasswordV3(string password, RandomNumberGenerator rng, KeyDerivationPrf prf, int iterCount, int saltSize, int numBytesRequested)
        {
            // Produce a version 3 (see comment above) text hash.
            byte[] salt = new byte[saltSize];
            rng.GetBytes(salt);
            byte[] subkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

            var outputBytes = new byte[13 + salt.Length + subkey.Length];
            outputBytes[0] = 0x01; // format marker
            WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
            WriteNetworkByteOrder(outputBytes, 5, (uint)iterCount);
            WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
            Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);
            return outputBytes;
        }
        private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return (uint)buffer[offset + 0] << 24
                | (uint)buffer[offset + 1] << 16
                | (uint)buffer[offset + 2] << 8
                | buffer[offset + 3];
        }
        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }

        #endregion
    }
}
