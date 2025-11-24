using CatBox.NET.Requests.File;

namespace CatBox.NET.Requests.URL;

/// <summary>
/// Wraps multiple URLs to upload to the API
/// </summary>
public sealed record UrlUploadRequest : UploadRequestBase
{
    /// <summary>
    /// A collection of URLs to upload
    /// </summary>
    public required IEnumerable<Uri> Files { get; init; }
}