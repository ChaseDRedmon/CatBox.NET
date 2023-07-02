using System.Text;
using BetterEnumsGen;
using CatBox.NET.Enums;
using CatBox.NET.Requests.CatBox;

namespace CatBox.NET.Client;

internal static class Common
{
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
    
    public static Task<string> ReadAsStringAsyncInternal(this HttpContent content, CancellationToken ct = default)
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
    
    // Shortening GetApiValue().ApiValue method call -> GetValue()
    public static string Value(this RequestType type) => type.GetApiValue()!.ApiValue;
    public static string Value(this ExpireAfter expireAfter) => expireAfter.GetApiValue()!.ApiValue;
}