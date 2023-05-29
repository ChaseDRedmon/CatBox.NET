namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// Wraps multiple URLs to upload to the API
/// </summary>
public sealed record UrlUploadRequest : UploadRequest
{
    /// <summary>
    /// A collection of URLs to upload
    /// </summary>
    public required IEnumerable<Uri> Files { get; init; }
}