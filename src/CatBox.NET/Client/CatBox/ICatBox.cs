using CatBox.NET.Requests.CatBox;

namespace CatBox.NET.Client;


/// <summary>
/// Provides an abstraction over <see cref="CatBoxClient"/> to group multiple tasks together
/// </summary>
/// <remarks>Not currently implemented so don't use</remarks>
public interface ICatBox
{
    Task<string?> CreateAlbumFromFiles(CreateAlbumRequestFromFiles requestFromFiles, CancellationToken ct = default);
    Task<string?> CreateAlbumFromUrls(CreateAlbumRequestFromUrls requestFromUrls, CancellationToken ct = default);
    Task<string?> CreateAlbumFromFiles(CreateAlbumRequestFromStream requestFromStream, CancellationToken ct = default);
}
