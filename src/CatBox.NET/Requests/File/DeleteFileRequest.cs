namespace CatBox.NET.Requests.File;

/// <summary>
/// Wraps a requestBase to delete files from the API
/// </summary>
public sealed record DeleteFileRequest
{
    /// <summary>
    /// The UserHash that owns the associated files
    /// </summary>
    public required string UserHash { get; init; }

    /// <summary>
    /// The file names of the files to delete
    /// </summary>
    public required IEnumerable<string> FileNames { get; init; }
}