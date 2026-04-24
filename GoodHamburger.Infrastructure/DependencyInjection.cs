using GoodHamburger.Application.Abstractions;
using GoodHamburger.Infrastructure.Catalog;
using GoodHamburger.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace GoodHamburger.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IMenuCatalog, StaticMenuCatalog>();
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();

        return services;
    }
}
