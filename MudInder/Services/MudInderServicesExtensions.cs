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
                .AddSingleton(new AzureFunctions.Core.AzureFunctionsClient(@"https://fa-mudinder.azurewebsites.net"));

            return services;
        }

    }
}
