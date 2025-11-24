using AnyOfTypes;
using CatBox.NET.Requests.File;
using CatBox.NET.Requests.URL;

namespace CatBox.NET.Requests.Album;

/// <summary>
/// Represents an upload requestBase
/// </summary>
public interface IAlbumUploadRequest
{
    /// <summary>
    /// The upload requestBase
    /// </summary>
    AnyOf<FileUploadRequest, IEnumerable<StreamUploadRequest>, UrlUploadRequest> UploadRequest { get; init; }
}