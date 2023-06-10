using System.Net;
using CatBox.NET.Client;
using CatBox.NET.Exceptions;
using CatBox.NET.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
            .AddScoped<ExceptionHandler>()
            .AddScoped<ICatBox, Catbox>()
            .AddScoped<ILitterboxClient, LitterboxClient>()
            .AddScoped<ICatBoxClient, CatBoxClient>()
            .AddScoped<ILitterboxClient, LitterboxClient>()
            .AddHttpClient<ICatBoxClient, CatBoxClient>()
            .AddHttpMessageHandler<ExceptionHandler>();

        services
            .AddHttpClient<ILitterboxClient, LitterboxClient>()
            .AddHttpMessageHandler<ExceptionHandler>();

        return services;
    }
}

internal sealed class ExceptionHandler : DelegatingHandler
{
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler>? logger = null)
    {
        _logger = logger ?? NullLogger<ExceptionHandler>.Instance;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.PreconditionFailed)
            return await Task.FromResult(response); // TODO: this is stupid
        
        var content = response.Content;
        var apiErrorMessage = await content.ReadAsStringAsyncCore(ct: cancellationToken);
        _logger.LogCatBoxAPIException(response.StatusCode, apiErrorMessage);

        throw apiErrorMessage switch
        {
            Common.AlbumNotFound => new CatBoxAlbumNotFoundException(),
            Common.FileNotFound => new CatBoxFileNotFoundException(),
            Common.InvalidExpiry => new LitterboxInvalidExpiry(),
            Common.MissingFileParameter => new CatBoxMissingFileException(),
            Common.MissingRequestType => new CatBoxMissingRequestTypeException(),
            _ when response.StatusCode is >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError => new HttpRequestException($"Generic Request Failure: {apiErrorMessage}"),
            _ when response.StatusCode >= HttpStatusCode.InternalServerError => new HttpRequestException($"Generic Internal Server Error: {apiErrorMessage}"),
            _ => new InvalidOperationException($"I don't know how you got here, but please create an issue on our GitHub (https://github.com/ChaseDRedmon/CatBox.NET): {apiErrorMessage}")
        };
    }
}