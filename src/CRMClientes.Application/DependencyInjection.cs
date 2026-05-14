using CRMClientes.Application.Interfaces;
using CRMClientes.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CRMClientes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClienteService, ClienteService>();
        return services;
    }
}
