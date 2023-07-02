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

    /// <inheritdoc/>
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
    
    /// <inheritdoc/>
    public async Task<string?> UploadImagesToAlbum(UploadToAlbumRequest request, CancellationToken ct = default)
    {
        var requestType = request.Request;
        var userHash = request.UserHash;
        var albumId = request.AlbumId;
        
        if (request.UploadRequest.IsFirst) // Upload Multiple Images
        {
            var uploadedFiles = _client.UploadMultipleImages(request.UploadRequest.First, ct);
            return await _client.ModifyAlbum(new ModifyAlbumImagesRequest
            {
                Request = requestType,
                UserHash = userHash,
                AlbumId = albumId,
                Files = uploadedFiles.ToBlockingEnumerable()
            }, ct);
        }

        if (request.UploadRequest.IsSecond) // Stream one image to be uploaded
        {
            var fileName = await _client.UploadImage(request.UploadRequest.Second, ct);
            return await _client.ModifyAlbum(new ModifyAlbumImagesRequest
            {
                Request = requestType,
                UserHash = userHash,
                AlbumId = albumId,
                Files  = new [] { fileName }
            }, ct);
        }
        
        if (request.UploadRequest.IsThird) // Upload Multiple URLs
        {
            var uploadedUrls = _client.UploadMultipleUrls(request.UploadRequest.Third, ct);
            return await _client.ModifyAlbum(new ModifyAlbumImagesRequest
            {
                Request = requestType,
                UserHash = userHash,
                AlbumId = albumId,
                Files  = uploadedUrls.ToBlockingEnumerable()
            }, ct);
        }

        throw new ArgumentOutOfRangeException(nameof(request.UploadRequest), "Invalid UploadRequest Type");
    }
}