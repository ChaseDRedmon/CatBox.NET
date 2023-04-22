using Microsoft.Extensions.DependencyInjection;

namespace CatBox.NET;

public static class CatBoxServices
{
    public static IServiceCollection AddCatBoxServices(this IServiceCollection services, Action<CatBoxConfig> setupAction)
    {
        services
            .Configure(setupAction)
            .AddScoped<ICatBox, Catbox>()
            .AddScoped<ILitterboxClient, LitterboxClient>()
            .AddScoped<ICatBoxClient, CatBoxClient>()
            .AddHttpClient<ICatBoxClient, CatBoxClient>();

        return services;
    }
}