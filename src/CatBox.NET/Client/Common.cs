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
    
    public static Task<string> ReadAsStringAsyncCore(this HttpContent content, CancellationToken ct = default)
    {
#if NET5_0_OR_GREATER
        return content.ReadAsStringAsync(ct);
#else
        return content.ReadAsStringAsync();
#endif
    }
}
