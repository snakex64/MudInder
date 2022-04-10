using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudInder.Services
{
    public static class MudInderServicesExtensions
    {
        public static IServiceCollection AddMudInderServices(this IServiceCollection services)
        {
            services.AddMudServices();

            services
                .AddSingleton<AuthService>()
#if DEBUG
                .AddSingleton(new AzureFunctions.Core.AzureFunctionsClient(@"http://localhost:7071"));
#else
                .AddSingleton(new AzureFunctions.Core.AzureFunctionsClient(@"https://fa-mudinder.azurewebsites.net"));
#endif

            return services;
        }

    }
}
