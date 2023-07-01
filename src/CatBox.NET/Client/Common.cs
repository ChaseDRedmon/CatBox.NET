using System.Net;
using System.Text;
using CatBox.NET.Enums;
using CatBox.NET.Exceptions;
using CatBox.NET.Requests.CatBox;

namespace CatBox.NET.Client;

internal static class Common
{
    public const string FileNotFound = "File doesn't exist?";
    public const string AlbumNotFound = "No album found for user specified.";
    public const string MissingRequestType = "No request type given.";
    public const string MissingFileParameter = "No files given.";
    public const string InvalidExpiry = "No expire time specified.";
    
    /// <summary>
    /// These file extensions are not allowed by the API, so filter them out
    /// </summary>
    /// <param name="extension"></param>
    /// <returns><see cref="bool"/></returns>
    public static bool IsFileExtensionValid(string extension)
    {
        switch (extension)
        {
            case ".exe":
            case ".scr":
            case ".cpl":
            case var _ when extension.Contains(".doc"):
            case ".jar":
                return false;
            default:
                return true;
        }
    }
    
    public static Task<string> ReadAsStringAsyncCore(this HttpContent content, CancellationToken ct = default)
    {
#if NET5_0_OR_GREATER
        return content.ReadAsStringAsync(ct);
#else
        return content.ReadAsStringAsync();
#endif
    }
    
    public static async Task<string> ToStringAsync(this IAsyncEnumerable<string?> asyncEnumerable, CancellationToken ct = default)
    {
        Throw.IfNull(asyncEnumerable);

        var builder = new StringBuilder();
        await foreach (var s in asyncEnumerable.WithCancellation(ct))
        {
            builder.Append(s).Append(' ');
        }

        return builder.ToString();
    }
    
    /// <summary>
    /// Validates an Album Creation Request
    /// </summary>
    /// <param name="request">The album creation request to validate</param>
    /// <exception cref="ArgumentNullException">when the request is null</exception>
    /// <exception cref="ArgumentNullException">when the description is null</exception>
    /// <exception cref="ArgumentNullException">when the title is null</exception>
    public static void ThrowIfAlbumCreationRequestIsInvalid(AlbumCreationRequest request)
    {
        Throw.IfNull(request);
        Throw.IfStringIsNullOrWhitespace(request.Description, "Album description cannot be null, empty, or whitespace");
        Throw.IfStringIsNullOrWhitespace(request.Title, "Album title cannot be null, empty, or whitespace");
    }
    
    /// <summary>
    /// 1. Filter Invalid Request Types on the Album Endpoint <br/>
    /// 2. Check that the user hash is not null, empty, or whitespace when attempting to modify or delete an album. User hash is required for those operations
    /// </summary>
    /// <param name="imagesRequest"></param>
    /// <returns></returns>
    public static bool IsAlbumRequestTypeValid(ModifyAlbumImagesRequest imagesRequest)
    {
        switch (imagesRequest.Request)
        {
            case RequestType.CreateAlbum:
            case RequestType.EditAlbum when !string.IsNullOrWhiteSpace(imagesRequest.UserHash):
            case RequestType.AddToAlbum when !string.IsNullOrWhiteSpace(imagesRequest.UserHash):
            case RequestType.RemoveFromAlbum when !string.IsNullOrWhiteSpace(imagesRequest.UserHash):
            case RequestType.DeleteAlbum when !string.IsNullOrWhiteSpace(imagesRequest.UserHash):
                return true;

            case RequestType.UploadFile:
            case RequestType.UrlUpload:
            case RequestType.DeleteFile:
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Converts a <see cref="CatBoxRequestTypes"/> to the CatBox.moe equivalent API parameter string
    /// </summary>
    /// <param name="requestTypes">A request type</param>
    /// <returns>CatBox API Request String</returns>
    /// <exception cref="ArgumentOutOfRangeException"> when an invalid request type is chosen</exception>
    public static string ToRequest(this RequestType requestTypes) =>
        requestTypes switch
        {
            RequestType.UploadFile => ApiAction.UploadFile,
            RequestType.UrlUpload => ApiAction.UrlUpload,
            RequestType.DeleteFile => ApiAction.DeleteFile,
            RequestType.CreateAlbum => ApiAction.CreateAlbum,
            RequestType.EditAlbum => ApiAction.EditAlbum,
            RequestType.AddToAlbum => ApiAction.AddToAlbum,
            RequestType.RemoveFromAlbum => ApiAction.RemoveFromAlbum,
            RequestType.DeleteAlbum => ApiAction.DeleteFromAlbum,
            _ => throw new ArgumentOutOfRangeException(nameof(requestTypes), requestTypes, null)
        };

    

    /// <summary>
    /// Converts a <see cref="ExpireAfter"/> value to the Litterbox.moe API equivalent time string
    /// </summary>
    /// <param name="expiry">Amount of time before an image expires and is deleted</param>
    /// <returns>Litterbox API Time Equivalent parameter value</returns>
    /// <exception cref="ArgumentOutOfRangeException"> when an invalid expiry value is chosen</exception>
    public static string ToRequest(this ExpireAfter expiry) =>
        expiry switch
        {
            ExpireAfter.OneHour => "1h",
            ExpireAfter.TwelveHours => "12h",
            ExpireAfter.OneDay => "24h",
            ExpireAfter.ThreeDays => "72h",
            _ => throw new ArgumentOutOfRangeException(nameof(expiry), expiry, null)
        };
}