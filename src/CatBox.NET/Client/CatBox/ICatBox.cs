using CatBox.NET.Requests.CatBox;

namespace CatBox.NET.Client;

/// <summary>
/// Provides an abstraction over <see cref="CatBoxClient"/> to group multiple tasks together
/// </summary>
public interface ICatBox
{
    /// <summary>
    /// Creates an album on CatBox from files that are uploaded in the request
    /// </summary>
    /// <param name="requestFromFiles">Album Creation Request</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns></returns>
    Task<string?> CreateAlbumFromFiles(CreateAlbumRequest requestFromFiles, CancellationToken ct = default);
    
    /// <summary>
    /// Upload and add images to an existing Catbox Album
    /// </summary>
    /// <param name="request">Album Creation Request</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns></returns>
    Task<string?> UploadImagesToAlbum(UploadToAlbumRequest request, CancellationToken ct = default);
}
