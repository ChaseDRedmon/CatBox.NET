using CatBox.NET.Enums;

namespace CatBox.NET.Requests.Litterbox;

public abstract record TemporaryRequestBase
{
    /// <summary>
    /// When the image, or images, should be expired
    /// </summary>
    public required ExpireAfter Expiry { get; init; }
}