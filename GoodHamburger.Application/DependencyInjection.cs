using GoodHamburger.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GoodHamburger.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<IMenuService, MenuService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
