namespace CatBox.NET;

/// <summary>
/// Wraps a request to delete files from the API
/// </summary>
public record DeleteFileRequest
{

    /// <summary>
    /// The UserHash that owns the associated files
    /// </summary>
    public required string UserHash { get; init; }

    /// <summary>
    /// The URLs of the files to delete
    /// </summary>
    public required IEnumerable<string> FileNames { get; init; }
}
