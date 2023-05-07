using CatBox.NET.Enums;

namespace CatBox.NET.Requests.Litterbox;

/// <summary>
/// A temporary request for an individual file upload
/// </summary>
public record TemporaryStreamUploadRequest : TemporaryRequest
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
