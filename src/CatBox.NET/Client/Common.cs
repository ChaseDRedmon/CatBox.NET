using System.Text;

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
}