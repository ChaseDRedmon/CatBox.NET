using Microsoft.Extensions.DependencyInjection;

namespace CatBox.NET;

public static class CatBoxServices
{
    /// <summary>
    /// Add the internal services that the library uses to the DI container
    /// </summary>
    /// <param name="services">Service Collection</param>
    /// <param name="setupAction">Configure the URL to upload files too</param>
    /// <returns>Service Collection</returns>
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