namespace CatBox.NET;

/// <summary>
/// Wraps a network stream to stream content to the API
/// </summary>
public record StreamUploadRequest : UploadRequest
{
    /// <summary>
    /// The name of the file
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// The byte stream that contains the image data
    /// </summary>
    public required Stream Stream { get; init; }

}
