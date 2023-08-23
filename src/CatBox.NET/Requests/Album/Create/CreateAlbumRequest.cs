using AnyOfTypes;

namespace CatBox.NET.Requests.CatBox;

public record CreateAlbumRequest : AlbumCreationRequest, IAlbumUploadRequest
{
    public required AnyOf<FileUploadRequest, IEnumerable<StreamUploadRequest>, UrlUploadRequest> UploadRequest { get; init; }
}