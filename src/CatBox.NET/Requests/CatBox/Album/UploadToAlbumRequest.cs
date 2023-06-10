using AnyOfTypes;

namespace CatBox.NET.Requests.CatBox;

public record UploadToAlbumRequest : Album
{
    /// <summary>
    /// The CatBox Upload request to upload files into an existing album
    /// </summary>
    public required AnyOf<FileUploadRequest, StreamUploadRequest, UrlUploadRequest> UploadRequest { get; init; }
}