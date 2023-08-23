using AnyOfTypes;

namespace CatBox.NET.Requests.CatBox;

public interface IAlbumUploadRequest
{
    AnyOf<FileUploadRequest, IEnumerable<StreamUploadRequest>, UrlUploadRequest> UploadRequest { get; init; }
}