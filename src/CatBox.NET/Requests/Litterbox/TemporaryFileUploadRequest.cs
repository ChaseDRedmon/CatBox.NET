namespace CatBox.NET.Requests.Litterbox;

/// <summary>
/// A temporary request for a collection of one or more files
/// </summary>
public sealed record TemporaryFileUploadRequest : TemporaryRequest
{
    /// <summary>
    /// A collection of files to upload
    /// </summary>
    public required IEnumerable<FileInfo> Files { get; init; }
}