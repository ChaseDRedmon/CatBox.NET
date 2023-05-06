using CatBox.NET.Requests.CatBox;

namespace CatBox.NET.Client;

[Obsolete("Do not use at this time")]
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

    public async Task<string?> CreateAlbumFromFiles(UploadAndCreateAlbumRequest request, CancellationToken ct = default)
    {
        var fileUploadRequest = request.UploadRequest;
        var catBoxFileNames = _client.UploadMultipleImages(fileUploadRequest, ct);
        var createAlbumRequest = new LocalCreateAlbumRequest
        {
            UserHash = fileUploadRequest.UserHash,
            Title = request.Title,
            Description = request.Description,
            Files = catBoxFileNames
        };

        return await _client.CreateAlbumFromUploadedFiles(createAlbumRequest, ct);
    }

    public Task CreateAlbumFromFiles(StreamUploadRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}