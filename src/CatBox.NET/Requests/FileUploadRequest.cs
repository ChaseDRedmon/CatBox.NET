namespace CatBox.NET.Requests;

/// <summary>
/// Wraps multiple files to upload to the API
/// </summary>
public record FileUploadRequest : UploadRequest
{
    /// <summary>
    /// A collection of file streams to upload
    /// </summary>
    public required IEnumerable<FileInfo> Files { get; init; }
}
