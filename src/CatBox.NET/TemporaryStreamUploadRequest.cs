namespace CatBox.NET;

/// <summary>
/// A temporary request for an individual file upload
/// </summary>
public record TemporaryStreamUploadRequest
{
    /// <summary>
    /// When the image should be expired
    /// </summary>
    public required ExpireAfter Expiry { get; init; }

    /// <summary>
    /// The name of the file
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// The byte stream that contains the image data
    /// </summary>
    public required Stream Stream { get; init; }
}
