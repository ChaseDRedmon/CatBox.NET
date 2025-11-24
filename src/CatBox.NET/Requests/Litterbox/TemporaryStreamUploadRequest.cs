namespace CatBox.NET.Requests.Litterbox;

/// <summary>
/// A temporary requestBase for an individual file upload
/// </summary>
public sealed record TemporaryStreamUploadRequest : TemporaryRequestBase
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