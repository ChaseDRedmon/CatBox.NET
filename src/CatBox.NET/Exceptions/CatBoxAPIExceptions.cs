using System.Diagnostics;
using System.Net;
using CatBox.NET.Client;
using CatBox.NET.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace CatBox.NET.Exceptions;

internal sealed class CatBoxFileNotFoundException : Exception
{
    public override string Message { get; } = "The CatBox File was not found";
}

internal sealed class CatBoxAlbumNotFoundException : Exception
{
    public override string Message { get; } = "The CatBox Album was not found";
}

// API Response Message: No requestBase type given.
internal sealed class CatBoxMissingRequestTypeException : Exception
{
    public override string Message { get; } = "The CatBox Request Type was not specified. Did you miss an API parameter?";
}

// API Response Message: No files given.
internal sealed class CatBoxMissingFileException : Exception
{
    public override string Message { get; } = "The FileToUpload parameter was not specified or is missing content. Did you miss an API parameter?";
}

//API Response Message: No expire time specified.
internal sealed class LitterboxInvalidExpiry : Exception
{
    public override string Message { get; } = "The Litterbox expiry requestBase parameter is invalid. Valid expiration times are: 1h, 12h, 24h, 72h";
}

// File size exceeds Litterbox's 1 GB upload limit
internal sealed class LitterboxFileSizeLimitExceededException(long fileSize) : Exception
{
    public override string Message { get; } = $"File size exceeds Litterbox's 1 GB upload limit. File size: {fileSize:N0} bytes ({fileSize / 1024.0 / 1024.0 / 1024.0:F2} GB)";
}

// File size exceeds CatBox's 200 MB upload limit
internal sealed class CatBoxFileSizeLimitExceededException(long fileSize) : Exception
{
    public override string Message { get; } = $"File size exceeds CatBox's 200 MB upload limit. File size: {fileSize:N0} bytes ({fileSize / 1024.0 / 1024.0:F2} MB)";
}

internal sealed class ExceptionHandler(ILogger<ExceptionHandler>? logger = null) : DelegatingHandler
{
    private const string FileNotFound = "File doesn't exist?";
    private const string AlbumNotFound = "No album found for user specified.";
    private const string MissingRequestType = "No requestBase type given.";
    private const string MissingFileParameter = "No files given.";
    private const string InvalidExpiry = "No expire time specified.";
    
    private readonly ILogger<ExceptionHandler> _logger = logger ?? NullLogger<ExceptionHandler>.Instance;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);
        if (response.StatusCode != HttpStatusCode.PreconditionFailed)
            return response;
            
        var content = response.Content;
        var apiErrorMessage = await content.ReadAsStringAsync(cancellationToken);
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
            _ => new UnreachableException($"I don't know how you got here, but please create an issue on our GitHub (https://github.com/ChaseDRedmon/CatBox.NET): {apiErrorMessage}")
        };
    }
}