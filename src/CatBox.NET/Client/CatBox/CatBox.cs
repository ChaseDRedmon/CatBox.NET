using CatBox.NET.Requests.CatBox;

namespace CatBox.NET.Client;

/// <inheritdoc/>
public sealed class Catbox : ICatBox
{
    private readonly ICatBoxClient _client;

    /// <summary>
    /// Instantiate a new catbox class 
    /// </summary>
    /// <param name="client">The CatBox Api Client</param>
    public Catbox(ICatBoxClient client)
    {
        _client = client;
    }
    
    /// <inheritdoc/>
    public async Task<string?> CreateAlbumFromFiles(CreateAlbumRequest requestFromFiles, CancellationToken ct = default)
    {
        var enumerable = Upload(requestFromFiles, ct);
        
        var createAlbumRequest = new RemoteCreateAlbumRequest
        {
            Title = requestFromFiles.Title,
            Description = requestFromFiles.Description,
            UserHash = requestFromFiles.UserHash,
            Files = enumerable.ToBlockingEnumerable(cancellationToken: ct)
        };

        return await _client.CreateAlbum(createAlbumRequest, ct);
    }
    
    /// <inheritdoc/>
    public async Task<string?> UploadImagesToAlbum(UploadToAlbumRequest request, CancellationToken ct = default)
    {
        var requestType = request.Request;
        var userHash = request.UserHash;
        var albumId = request.AlbumId;

        var enumerable = Upload(request, ct);

        return await _client.ModifyAlbum(new ModifyAlbumImagesRequest
        {
            Request = requestType,
            UserHash = userHash,
            AlbumId = albumId,
            Files = enumerable.ToBlockingEnumerable()
        }, ct);
    }

    private IAsyncEnumerable<string?> Upload(IAlbumUploadRequest request, CancellationToken ct = default)
    {
        return request.UploadRequest switch
        {
            { IsFirst: true } => _client.UploadFiles(request.UploadRequest, ct),
            { IsSecond: true } => _client.UploadFilesAsStream(request.UploadRequest.Second, ct),
            { IsThird: true } => _client.UploadFilesAsUrl(request.UploadRequest, ct),
            _ => throw new InvalidOperationException("Invalid request type")
        };
    }
}