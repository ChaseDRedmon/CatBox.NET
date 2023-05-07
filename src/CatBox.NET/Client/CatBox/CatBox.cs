using CatBox.NET.Requests.CatBox;

namespace CatBox.NET.Client;

public sealed class Catbox : ICatBox
{
    private readonly ICatBoxClient _client;

    /// <summary>
    /// Instantiate a new catbox class 
    /// </summary>
    /// <param name="userHash"></param>
    /// <param name="host"></param>
    /// <param name="client"></param>
    public Catbox(ICatBoxClient client)
    {
        _client = client;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestFromFiles"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <inheritdoc/>
    public async Task<string?> CreateAlbumFromFiles(CreateAlbumRequestFromFiles requestFromFiles, CancellationToken ct = default)
    {
        // TODO: Not super happy with the blocking enumerable implementation

        var catBoxFileNames = _client.UploadMultipleImages(requestFromFiles.UploadRequest, ct);
        var createAlbumRequest = new RemoteCreateAlbumRequest
        {
            Title = requestFromFiles.Title,
            Description = requestFromFiles.Description,
            UserHash = requestFromFiles.UserHash,
            Files = catBoxFileNames.ToBlockingEnumerable(cancellationToken: ct)
        };

        return await _client.CreateAlbum(createAlbumRequest, ct);
    }

    public Task CreateAlbumFromFiles(StreamUploadRequest request, CancellationToken ct = default)
    {
        var x = new EditAlbumRequest
        {
            UserHash = null,
            AlbumId = null,
            Title = null,
            Description = null,
            Files = null
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestFromFiles"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <inheritdoc/>
    public async Task<string?> CreateAlbumFromUrls(CreateAlbumRequestFromFiles requestFromFiles, CancellationToken ct = default)
    {
        // TODO: Not super happy with the blocking enumerable implementation

        var catBoxFileNames = _client.UploadMultipleUrls(requestFromFiles.UploadRequest, ct);
        var createAlbumRequest = new RemoteCreateAlbumRequest
        {
            Title = requestFromFiles.Title,
            Description = requestFromFiles.Description,
            UserHash = requestFromFiles.UserHash,
            Files = catBoxFileNames.ToBlockingEnumerable(cancellationToken: ct)
        };

        return await _client.CreateAlbum(createAlbumRequest, ct);
    }

    public async Task CreateAlbumFromFiles(CreateAlbumRequestFromStream request, CancellationToken ct = default)
    {
        var catBoxFileNames = _client.UploadMultipleImages(request., ct);
        var createAlbumRequest = new RemoteCreateAlbumRequest
        {
            Title = request.Title,
            Description = request.Description,
            UserHash = request.UserHash,
            Files = catBoxFileNames.ToBlockingEnumerable(cancellationToken: ct)
        };

        await _client.CreateAlbum(createAlbumRequest, ct);
    }
}