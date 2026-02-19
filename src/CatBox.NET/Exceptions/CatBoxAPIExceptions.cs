using System.Diagnostics;
using System.Net;
using System.Text.Json;
using CatBox.NET.Client;
using CatBox.NET.Logging;
using CatBox.NET.Responses;
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

// API Response Message: No userhash provided!
internal sealed class CatBoxMissingUserHashException : Exception
{
    public override string Message { get; } = "The UserHash parameter was not provided. UserHash is required for album modification and deletion operations.";
}

// API Response Message: No valid link given.
internal sealed class CatBoxMissingUrlException : Exception
{
    public override string Message { get; } = "The URL parameter was not provided or is invalid. A valid URL is required for URL upload operations.";
}

// API Response Message: Tried to delete a file that didn't belong to that userhash.
internal sealed class CatBoxFileOwnershipException : Exception
{
    public override string Message { get; } = "Attempted to delete a file that does not belong to the provided userhash. You can only delete files you own.";
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

// Album exceeds CatBox's file limit
internal sealed class CatBoxAlbumFileLimitExceededException(int fileCount) : Exception
{
    public override string Message { get; } = $"Album exceeds CatBox's {Common.MaxAlbumFiles} file limit. Attempted to add {fileCount} files.";
}

internal sealed class ExceptionHandler(ILogger<ExceptionHandler>? logger = null) : DelegatingHandler
{
    // Plain-text error messages (HTTP 412)
    private const string FileNotFound = "File doesn't exist?";
    private const string AlbumNotFound = "No album found for user specified.";
    private const string MissingRequestType = "No request type given?";
    private const string MissingFileParameter = "No files given.";
    private const string MissingUserHash = "No userhash provided!";
    private const string MissingUrl = "No valid link given.";
    private const string FileOwnershipMismatch = "Tried to delete a file that didn't belong to that userhash.";
    private const string InvalidExpiry = "No expire time specified.";

    // JSON error messages (HTTP 400)
    private const string JsonAlbumNotFound = "An album was not found. Either the album never existed, or was deleted.";

    private readonly ILogger<ExceptionHandler> _logger = logger ?? NullLogger<ExceptionHandler>.Instance;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        // Only process error status codes
        if (response.IsSuccessStatusCode)
            return response;

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        // Try JSON parsing first (HTTP 400 pattern)
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var exception = TryParseJsonError(content);
            if (exception is not null)
            {
                _logger.LogCatBoxAPIException(response.StatusCode, content);
                throw exception;
            }
        }

        // Fall back to plain-text matching (HTTP 412 pattern)
        if (response.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            _logger.LogCatBoxAPIException(response.StatusCode, content);
            throw MatchPlainTextError(content, response.StatusCode);
        }

        // Return response for unhandled status codes (let caller handle)
        return response;
    }

    private static Exception? TryParseJsonError(string content)
    {
        try
        {
            var errorResponse = JsonSerializer.Deserialize(content, CatBoxJsonContext.Default.CatBoxApiErrorResponse);
            if (errorResponse is { Success: false, Data.Error: not null })
            {
                return errorResponse.Data.Error switch
                {
                    var e when e.Equals(JsonAlbumNotFound, StringComparison.OrdinalIgnoreCase) => new CatBoxAlbumNotFoundException(),
                    _ => new HttpRequestException($"CatBox API Error: {errorResponse.Data.Error}")
                };
            }
        }
        catch (JsonException)
        {
            // Not valid JSON, return null to try other parsing
        }
        return null;
    }

    private static Exception MatchPlainTextError(string content, HttpStatusCode statusCode)
    {
        return content switch
        {
            AlbumNotFound => new CatBoxAlbumNotFoundException(),
            FileNotFound => new CatBoxFileNotFoundException(),
            InvalidExpiry => new LitterboxInvalidExpiry(),
            MissingFileParameter => new CatBoxMissingFileException(),
            MissingUserHash => new CatBoxMissingUserHashException(),
            MissingUrl => new CatBoxMissingUrlException(),
            FileOwnershipMismatch => new CatBoxFileOwnershipException(),
            MissingRequestType => new CatBoxMissingRequestTypeException(),
            _ when statusCode is >= HttpStatusCode.BadRequest and < HttpStatusCode.InternalServerError => new HttpRequestException($"Generic Request Failure: {content}"),
            _ when statusCode >= HttpStatusCode.InternalServerError => new HttpRequestException($"Generic Internal Server Error: {content}"),
            _ => new UnreachableException($"Unexpected error: {content}")
        };
    }
}