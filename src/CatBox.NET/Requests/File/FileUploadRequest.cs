namespace CatBox.NET.Requests.File;

/// <summary>
/// Wraps multiple files to upload to the API
/// </summary>
public sealed record FileUploadRequest : UploadRequestBase
{
    /// <summary>
    /// A collection of files paths to upload
    /// </summary>
    public required IEnumerable<FileInfo> Files { get; init; }
}