using CatBox.NET.Requests.Album;
using CatBox.NET.Requests.Album.Create;
using CatBox.NET.Requests.Album.Modify;

namespace CatBox.NET.Client;

/// <summary>
/// Provides an abstraction over <see cref="CatBoxClient"/> to group multiple tasks together
/// </summary>
public interface ICatBox
{
    /// <summary>
    /// Creates an album on CatBox from files that are uploaded in the requestBase
    /// </summary>
    /// <param name="requestFromFiles">Album Creation Request</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns></returns>
    Task<string?> CreateAlbumFromFilesAsync(CreateAlbumRequest requestFromFiles, CancellationToken ct = default);

    /// <summary>
    /// Upload and add images to an existing Catbox Album
    /// </summary>
    /// <param name="request">Album Creation Request</param>
    /// <param name="ct">Cancellation Token.</param>
    /// <returns></returns>
    Task<string?> UploadImagesToAlbumAsync(UploadToAlbumRequest request, CancellationToken ct = default);
}

/// <inheritdoc/>
public sealed class Catbox : ICatBox
{
    private readonly ICatBoxClient _client;

    /// <summary>
    /// Instantiate a new catbox class 
    /// </summary>
    /// <param name="client">The CatBox Api Client (<see cref="ICatBoxClient"/>)</param>
    public Catbox(ICatBoxClient client)
    {
        _client = client;
    }
    
    /// <inheritdoc/>
    public Task<string?> CreateAlbumFromFilesAsync(CreateAlbumRequest requestFromFiles, CancellationToken ct = default)
    {
        var enumerable = Upload(requestFromFiles, ct);

        var createAlbumRequest = new RemoteCreateAlbumRequest
        {
            Title = requestFromFiles.Title,
            Description = requestFromFiles.Description,
            UserHash = requestFromFiles.UserHash,
            Files = enumerable.ToBlockingEnumerable(cancellationToken: ct)
        };

        return _client.CreateAlbumAsync(createAlbumRequest, ct);
    }
    
    /// <inheritdoc/>
    public Task<string?> UploadImagesToAlbumAsync(UploadToAlbumRequest request, CancellationToken ct = default)
    {
        var requestType = request.Request;
        var userHash = request.UserHash;
        var albumId = request.AlbumId;

        var enumerable = Upload(request, ct);

        return _client.ModifyAlbumAsync(new ModifyAlbumImagesRequest
        {
            Request = requestType,
            UserHash = userHash,
            AlbumId = albumId,
            Files = enumerable.ToBlockingEnumerable()
        }, ct);
    }
    
    /// <summary>
    /// Upload files based on the requestBase type
    /// </summary>
    /// <param name="request">Upload requestBase type</param>
    /// <param name="ct">Cancellation Token</param>
    /// <returns>API Response</returns>
    /// <exception cref="InvalidOperationException">When passing in an invalid requestBase type</exception>
    private IAsyncEnumerable<string?> Upload(IAlbumUploadRequest request, CancellationToken ct = default)
    {
        return request.UploadRequest switch
        {
            { IsFirst: true } => _client.UploadFilesAsync(request.UploadRequest, ct),
            { IsSecond: true } => _client.UploadFilesAsStreamAsync(request.UploadRequest.Second, ct),
            { IsThird: true } => _client.UploadFilesAsUrlAsync(request.UploadRequest, ct),
            _ => throw new InvalidOperationException("Invalid requestBase type")
        };
    }
}