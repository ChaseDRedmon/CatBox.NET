namespace CatBox.NET;

/// <summary>
/// A temporary request for a collection of one or more files
/// </summary>
public record TemporaryFileUploadRequest
{

    /// <summary>
    /// When the image, or images, should be expired
    /// </summary>
    public required ExpireAfter ExpireAfter { get; init; }

    /// <summary>
    /// A collection of files to upload
    /// </summary>
    public required IEnumerable<FileInfo> Files { get; init; }

}
