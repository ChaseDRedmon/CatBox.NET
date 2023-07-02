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
    public static IServiceCollection AddCatBoxServices(this IServiceCollection services, Action<CatboxOptions> setupAction)
    {
        services
            .Configure(setupAction)
            .AddScoped<ExceptionHandler>()
            .AddScoped<ICatBox, Catbox>()
            .AddScoped<ICatBoxClient, CatBoxClient>()
            .AddScoped<ILitterboxClient, LitterboxClient>()
            .AddHttpClientWithMessageHandler<ICatBoxClient, CatBoxClient, ExceptionHandler>()
            .AddHttpClientWithMessageHandler<ILitterboxClient, LitterboxClient, ExceptionHandler>();

        return services;
    }

    private static IServiceCollection AddHttpClientWithMessageHandler<TInterface, TImplementation, THandler>(this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
        where THandler : DelegatingHandler
    {
        services.AddHttpClient<TInterface, TImplementation>().AddHttpMessageHandler<THandler>();
        return services;
    }
}

internal sealed class ExceptionHandler : DelegatingHandler
{
    private const string FileNotFound = "File doesn't exist?";
    private const string AlbumNotFound = "No album found for user specified.";
    private const string MissingRequestType = "No request type given.";
    private const string MissingFileParameter = "No files given.";
    private const string InvalidExpiry = "No expire time specified.";
    
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(ILogger<ExceptionHandler>? logger = null) : base(new HttpClientHandler())
    {
        _logger = logger ?? NullLogger<ExceptionHandler>.Instance;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.PreconditionFailed)
            return response;
            
        var content = response.Content;
        var apiErrorMessage = await content.ReadAsStringAsyncInternal(ct: cancellationToken);
        _logger.LogCatBoxAPIException(response.StatusCode, apiErrorMessage);
            
        throw apiErrorMessage switch
        {
            AlbumNotFound => new CatBoxAlbumNotFoundException(),
            FileNotFound => new CatBoxFileNotFoundException(),
            InvalidExpiry => new LitterboxInvalidExpiry(),
            MissingFileParameter => new CatBoxMissingFileException(),
            MissingRequestType => new CatBoxMissingRequestTypeException(),
            _ when response.StatusCode is >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError => new HttpRequestException($"Generic Request Failure: {apiErrorMessage}"),
            _ when response.StatusCode >= HttpStatusCode.InternalServerError => new HttpRequestException($"Generic Internal Server Error: {apiErrorMessage}"),
            _ => new InvalidOperationException($"I don't know how you got here, but please create an issue on our GitHub (https://github.com/ChaseDRedmon/CatBox.NET): {apiErrorMessage}")
        };
    }
}