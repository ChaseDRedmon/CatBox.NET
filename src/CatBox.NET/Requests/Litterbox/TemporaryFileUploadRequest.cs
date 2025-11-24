namespace CatBox.NET.Requests.Litterbox;

/// <summary>
/// A temporary requestBase for a collection of one or more files
/// </summary>
public sealed record TemporaryFileUploadRequest : TemporaryRequestBase
{
    /// <summary>
    /// A collection of files to upload
    /// </summary>
    public required IEnumerable<FileInfo> Files { get; init; }
}