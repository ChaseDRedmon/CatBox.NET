using AnyOfTypes;

namespace CatBox.NET.Requests.CatBox;

/// <summary>
/// A request for uploading files into an existing CatBox Album
/// </summary>
public record UploadToAlbumRequest : Album, IAlbumUploadRequest
{
    /// <summary>
    /// The CatBox Upload request to upload files into an existing album
    /// </summary>
    public required AnyOf<FileUploadRequest, IEnumerable<StreamUploadRequest>, UrlUploadRequest> UploadRequest { get; init; }
}