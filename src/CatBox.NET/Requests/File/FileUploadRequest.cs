namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// Wraps multiple files to upload to the API
/// </summary>
public sealed record FileUploadRequest : UploadRequest
{
    /// <summary>
    /// A collection of files paths to upload
    /// </summary>
    public required IEnumerable<FileInfo> Files { get; init; }
}