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
    /// Creates an album on CatBox from files that are uploaded in the request
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

    /// <summary>
    /// Creates an album on CatBox from URLs that are specified in the request
    /// </summary>
    /// <param name="requestFromUrls"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <inheritdoc/>
    public async Task<string?> CreateAlbumFromUrls(CreateAlbumRequestFromUrls requestFromUrls, CancellationToken ct = default)
    {
        // TODO: Not super happy with the blocking enumerable implementation

        var catBoxFileNames = _client.UploadMultipleUrls(requestFromUrls.UrlUploadRequest, ct);
        var createAlbumRequest = new RemoteCreateAlbumRequest
        {
            Title = requestFromUrls.Title,
            Description = requestFromUrls.Description,
            UserHash = requestFromUrls.UserHash,
            Files = catBoxFileNames.ToBlockingEnumerable(cancellationToken: ct)
        };

        return await _client.CreateAlbum(createAlbumRequest, ct);
    }
    
    /// <summary>
    /// Creates an album on CatBox from files that are streamed to the API in the request
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ct"></param>
    public async Task<string?> CreateAlbumFromFiles(CreateAlbumRequestFromStream requestFromStream, CancellationToken ct = default)
    {
        var catBoxFileNames = new List<string>();

        foreach (var request in requestFromStream.Request)
        {
            var fileName = await _client.UploadImage(request, ct);
            catBoxFileNames.Add(fileName);
        }
        
        var createAlbumRequest = new RemoteCreateAlbumRequest
        {
            Title = requestFromStream.Title,
            Description = requestFromStream.Description,
            UserHash = requestFromStream.UserHash,
            Files = catBoxFileNames
        };

        return await _client.CreateAlbum(createAlbumRequest, ct);
    }
}