using CatBox.NET.Requests.CatBox;

namespace CatBox.NET.Client;


/// <summary>
/// Provides an abstraction over <see cref="CatBoxClient"/> to group multiple tasks together
/// </summary>
/// <remarks>Not currently implemented so don't use</remarks>
public interface ICatBox
{
    Task<string?> CreateAlbumFromFiles(UploadAndCreateAlbumRequest request, CancellationToken ct = default);
    Task CreateAlbumFromFiles(StreamUploadRequest request, CancellationToken ct = default);
}
