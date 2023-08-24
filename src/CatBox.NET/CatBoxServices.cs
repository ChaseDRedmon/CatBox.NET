using CatBox.NET.Client;
using CatBox.NET.Exceptions;
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
    public static IServiceCollection AddCatBoxServices(this IServiceCollection services, Action<CatboxOptions> setupAction)
    {
        services
            .Configure(setupAction)
            .AddScoped<ExceptionHandler>()
            .AddScoped<ICatBox, Catbox>()
            .AddScoped<ICatBoxClient, CatBoxClient>()
            .AddScoped<ILitterboxClient, LitterboxClient>()
            .AddHttpClientWithMessageHandler<ICatBoxClient, CatBoxClient>(static _ => new ExceptionHandler())
            .AddHttpClientWithMessageHandler<ILitterboxClient, LitterboxClient>(static _ => new ExceptionHandler());

        return services;
    }

    private static IServiceCollection AddHttpClientWithMessageHandler<TInterface, TImplementation>(this IServiceCollection services, Func<IServiceProvider, DelegatingHandler> configureClient)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddHttpClient<TInterface, TImplementation>().AddHttpMessageHandler(configureClient);
        return services;
    }
}