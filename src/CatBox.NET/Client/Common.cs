using System.Net;
using System.Text;
using CatBox.NET.Exceptions;

namespace CatBox.NET.Client;

internal static class Common
{
    public const string FileNotFound = "File doesn't exist?";
    public const string AlbumNotFound = "No album found for user specified.";
    
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
        if (asyncEnumerable is null)
            throw new ArgumentNullException(nameof(asyncEnumerable), "Argument cannot be null");

        var builder = new StringBuilder();
        await foreach (var s in asyncEnumerable.WithCancellation(ct))
        {
            builder.Append(s).Append(' ');
        }

        return builder.ToString();
    }
    
    /// <summary>
    /// Checks the API response message against the error messages 
    /// </summary>
    /// <param name="message">API Response</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>Message Response Body</returns>
    /// <exception cref="CatBoxAlbumNotFoundException">when the CatBox album is not found on the server</exception>
    /// <exception cref="CatBoxFileNotFoundException">when the CatBox file is not found on the server</exception>
    public static async Task<string?> ThrowIfUnsuccessfulResponse(this HttpResponseMessage message, CancellationToken ct = default)
    {
        var messageBody = await message.Content.ReadAsStringAsyncCore(ct);
        if (message.StatusCode != HttpStatusCode.PreconditionFailed) 
            return messageBody;
        
        switch (messageBody)
        {
            case AlbumNotFound:
                throw new CatBoxAlbumNotFoundException();
            case FileNotFound:
                throw new CatBoxFileNotFoundException();
        }

        return messageBody;
    }
}