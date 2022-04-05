using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using MudInder.AzureFunctions.Functions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
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

        public static ClaimsPrincipal ValidateAuth(this HttpRequest request)
        {
            try
            {
                var auth = request.Headers["Authorization"].FirstOrDefault()?["Bearer ".Length..];

                var tokenHandler = new JwtSecurityTokenHandler();
                return tokenHandler.ValidateToken(auth, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(LoginFunctions.Key))
                }, out SecurityToken validatedToken);
            }
            catch
            {
                throw new NotSupportedException();
            }
        }

    }
}
