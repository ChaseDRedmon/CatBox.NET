namespace CatBox.NET.Requests;

/// <summary>
/// Wraps multiple URLs to upload to the API
/// </summary>
public record UrlUploadRequest : UploadRequest
{
    /// <summary>
    /// A collection of URLs to upload
    /// </summary>
    public required IEnumerable<Uri> Files { get; init; }
}
