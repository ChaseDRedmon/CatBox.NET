using CatBox.NET.Client;
using CatBox.NET.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace CatBox.NET;

public static class CatBoxServices
{
    /// <param name="services">Service Collection</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Add the internal services that the library uses to the DI container
        /// </summary>
        /// <param name="setupAction">Configure the URL to upload files too</param>
        /// <returns>Service Collection</returns>
        public IServiceCollection AddCatBoxServices(Action<CatboxOptions> setupAction)
        {
            services
                .Configure(setupAction)
                .AddTransient<ExceptionHandler>()
                .AddScoped<ICatBox, Catbox>()
                .AddScoped<ICatBoxClient, CatBoxClient>()
                .AddScoped<ILitterboxClient, LitterboxClient>()
                .AddHttpClientWithMessageHandler<ICatBoxClient, CatBoxClient>()
                .AddHttpClientWithMessageHandler<ILitterboxClient, LitterboxClient>();

            return services;
        }

        private IServiceCollection AddHttpClientWithMessageHandler<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            services
                .AddHttpClient<TInterface, TImplementation>(client =>
                    client.DefaultRequestHeaders.UserAgent.ParseAdd(".NET/10 CatBox.NET/1.0 (+https://github.com/ChaseDRedmon/CatBox.NET)"))
                .AddHttpMessageHandler<ExceptionHandler>()
                .AddStandardResilienceHandler(options =>
                {
                    // Disable retries for unsafe HTTP methods to prevent duplicate uploads/album operations
                    options.Retry.DisableForUnsafeHttpMethods();

                    options.Retry = new HttpRetryStrategyOptions
                    {
                        BackoffType = DelayBackoffType.Exponential,
                        UseJitter = true,
                        Delay = TimeSpan.FromSeconds(5),
                        MaxRetryAttempts = 5
                    };
                });

            return services;
        }
    }
}