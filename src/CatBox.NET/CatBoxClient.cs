using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using static CatBox.NET.Common;

namespace CatBox.NET;

public class CatBoxClient : ICatBoxClient
{
    private readonly HttpClient _client;
    private readonly CatBoxConfig _config;

    /// <summary>
    /// Creates a new <see cref="CatBoxClient"/>
    /// </summary>
    /// <param name="client"><see cref="HttpClient"/></param>
    /// <param name="config"><see cref="IOptions{TOptions}"/></param>
    /// <exception cref="ArgumentNullException"></exception>
    public CatBoxClient(HttpClient client, IOptions<CatBoxConfig> config)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client), "HttpClient cannot be null");

        if (config.Value.CatBoxUrl is null)
            throw new ArgumentNullException(nameof(config.Value.CatBoxUrl), "CatBox API URL cannot be null. Check that URL was set by calling .AddCatBoxServices(f => f.CatBoxUrl = new Uri(\"https://catbox.moe/user/api.php\"))");

        _config = config.Value;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string?> UploadMultipleImages(FileUploadRequest fileUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (fileUploadRequest is null)
            throw new ArgumentNullException(nameof(fileUploadRequest), "Argument cannot be null");

        foreach (var imageFile in fileUploadRequest.Files.Where(static f => IsFileExtensionValid(f.Extension)))
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
            using var content = new MultipartFormDataContent();
            
            await using var fileStream = File.OpenRead(imageFile.FullName);
            content.Add(new StringContent(CatBoxRequestTypes.UploadFile.ToRequest()), CatBoxRequestStrings.RequestType);

            if (!string.IsNullOrWhiteSpace(fileUploadRequest.UserHash))
                content.Add(new StringContent(fileUploadRequest.UserHash), CatBoxRequestStrings.UserHashType);

            content.Add(new StreamContent(fileStream), CatBoxRequestStrings.FileToUploadType, imageFile.Name);
            request.Content = content;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            yield return await response.Content.ReadAsStringAsync(ct);
        }
    }
    
    /// <inheritdoc/>
    public async Task<string?> UploadImage(StreamUploadRequest fileUploadRequest, CancellationToken ct = default)
    {
        if (fileUploadRequest is null)
            throw new ArgumentNullException(nameof(fileUploadRequest), "Argument cannot be null");

        if (fileUploadRequest.FileName is null)
            throw new ArgumentNullException(nameof(fileUploadRequest.FileName), "Argument cannot be null");

        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(CatBoxRequestTypes.UploadFile.ToRequest()), CatBoxRequestStrings.RequestType }
        };

        if (!string.IsNullOrWhiteSpace(fileUploadRequest.UserHash))
            content.Add(new StringContent(fileUploadRequest.UserHash), CatBoxRequestStrings.UserHashType);

        content.Add(new StreamContent(fileUploadRequest.Stream), CatBoxRequestStrings.FileToUploadType, fileUploadRequest.FileName);
        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string?> UploadMultipleUrls(UrlUploadRequest urlUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (urlUploadRequest is null)
            throw new ArgumentNullException(nameof(urlUploadRequest), "Argument cannot be null");

        foreach (var fileUrl in urlUploadRequest.Files)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
            using var content = new MultipartFormDataContent(); // Disposing of MultipartFormDataContent, cascades disposal of String / Stream / Content classes

            if (fileUrl is null)
                continue;

            content.Add(new StringContent(CatBoxRequestTypes.UrlUpload.ToRequest()), CatBoxRequestStrings.RequestType);

            if (!string.IsNullOrWhiteSpace(urlUploadRequest.UserHash))
                content.Add(new StringContent(urlUploadRequest.UserHash), CatBoxRequestStrings.UserHashType);

            content.Add(new StringContent(fileUrl.AbsoluteUri), CatBoxRequestStrings.UrlType);
            request.Content = content;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            yield return await response.Content.ReadAsStringAsync(ct);
        }
    }

    /// <inheritdoc/>
    public async Task<string?> DeleteMultipleFiles(DeleteFileRequest deleteFileRequest, CancellationToken ct = default)
    {
        if (deleteFileRequest is null)
            throw new ArgumentNullException(nameof(deleteFileRequest), "Argument cannot be null");

        if (string.IsNullOrWhiteSpace(deleteFileRequest.UserHash))
            throw new ArgumentNullException(nameof(deleteFileRequest.UserHash), "Argument cannot be null");

        var fileNames = string.Join(" ", deleteFileRequest.FileNames);
        if (string.IsNullOrWhiteSpace(fileNames))
            throw new ArgumentNullException(nameof(deleteFileRequest.FileNames), "File list cannot be empty");

        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(CatBoxRequestTypes.DeleteFile.ToRequest()), CatBoxRequestStrings.RequestType },
            { new StringContent(deleteFileRequest.UserHash), CatBoxRequestStrings.UserHashType },
            { new StringContent(fileNames), CatBoxRequestStrings.FileType }
        };
        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }
    
    /// <inheritdoc/>
    public async Task<string?> CreateAlbum(CreateAlbumRequest createAlbumRequest, CancellationToken ct = default)
    {
        if (createAlbumRequest is null)
            throw new ArgumentNullException(nameof(createAlbumRequest), "Argument cannot be null");

        if (string.IsNullOrWhiteSpace(createAlbumRequest.Description))
            throw new ArgumentNullException(nameof(createAlbumRequest.Description),
                "Album description cannot be null, empty, or whitespace");

        if (string.IsNullOrWhiteSpace(createAlbumRequest.Title))
            throw new ArgumentNullException(nameof(createAlbumRequest.Title),
                "Album title cannot be null, empty, or whitespace");

        var links = createAlbumRequest.Files.Select(link =>
        {
            if (link.Contains(_config.CatBoxUrl!.Host))
            {
                return new Uri(link).PathAndQuery[1..];
            }

            return link;
        });
        
        var fileNames = string.Join(" ", links);
        if (string.IsNullOrWhiteSpace(fileNames))
            throw new ArgumentNullException(nameof(createAlbumRequest.Files), "File list cannot be empty");

        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(CatBoxRequestTypes.CreateAlbum.ToRequest()), CatBoxRequestStrings.RequestType }
        };

        if (!string.IsNullOrWhiteSpace(createAlbumRequest.UserHash))
            content.Add(new StringContent(createAlbumRequest.UserHash), CatBoxRequestStrings.UserHashType);

        content.Add(new StringContent(createAlbumRequest.Title), CatBoxRequestStrings.TitleType);
        
        if (!string.IsNullOrWhiteSpace(createAlbumRequest.Description))
            content.Add(new StringContent(createAlbumRequest.Description), CatBoxRequestStrings.DescriptionType);
        
        content.Add(new StringContent(fileNames), CatBoxRequestStrings.FileType);
        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }
    
    /// <inheritdoc/>
    public async Task<string?> EditAlbum(EditAlbumRequest editAlbumRequest, CancellationToken ct = default)
    {
        if (editAlbumRequest is null)
            throw new ArgumentNullException(nameof(editAlbumRequest), "Argument cannot be null");

        if (string.IsNullOrWhiteSpace(editAlbumRequest.UserHash))
            throw new ArgumentNullException(nameof(editAlbumRequest.UserHash),
                "UserHash cannot be null, empty, or whitespace when attempting to modify an album");

        if (string.IsNullOrWhiteSpace(editAlbumRequest.Description))
            throw new ArgumentNullException(nameof(editAlbumRequest.Description),
                "Album description cannot be null, empty, or whitespace");

        if (string.IsNullOrWhiteSpace(editAlbumRequest.Title))
            throw new ArgumentNullException(nameof(editAlbumRequest.Title),
                "Album title cannot be null, empty, or whitespace");

        if (string.IsNullOrWhiteSpace(editAlbumRequest.AlbumId))
            throw new ArgumentNullException(nameof(editAlbumRequest.AlbumId),
                "AlbumId (Short) cannot be null, empty, or whitespace");

        var fileNames = string.Join(" ", editAlbumRequest.Files);
        if (string.IsNullOrWhiteSpace(fileNames))
            throw new ArgumentNullException(nameof(editAlbumRequest.Files), "File list cannot be empty");

        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(CatBoxRequestTypes.EditAlbum.ToRequest()), CatBoxRequestStrings.RequestType },
            { new StringContent(editAlbumRequest.UserHash), CatBoxRequestStrings.UserHashType },
            { new StringContent(editAlbumRequest.AlbumId), CatBoxRequestStrings.AlbumIdShortType },
            { new StringContent(editAlbumRequest.Title), CatBoxRequestStrings.TitleType },
            { new StringContent(editAlbumRequest.Description), CatBoxRequestStrings.DescriptionType },
            { new StringContent(fileNames), CatBoxRequestStrings.FileType }
        };
        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }
    
    /// <inheritdoc/>
    public async Task<string?> ModifyAlbum(AlbumRequest albumRequest, CancellationToken ct = default)
    {
        if (albumRequest is null)
            throw new ArgumentNullException(nameof(albumRequest), "Argument cannot be null");

        if (IsAlbumRequestTypeValid(albumRequest))
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            throw new ArgumentException("Invalid Request Type for album endpoint", nameof(albumRequest.Request));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

        if (string.IsNullOrWhiteSpace(albumRequest.UserHash))
            throw new ArgumentNullException(nameof(albumRequest.UserHash),
                "UserHash cannot be null, empty, or whitespace when attempting to modify an album");

        if (albumRequest.Request != CatBoxRequestTypes.AddToAlbum &&
            albumRequest.Request != CatBoxRequestTypes.RemoveFromAlbum &&
            albumRequest.Request != CatBoxRequestTypes.DeleteAlbum)
        {
            throw new InvalidOperationException(
                "The ModifyAlbum method only supports CatBoxRequestTypes.AddToAlbum, CatBoxRequestTypes.RemoveFromAlbum, and CatBoxRequestTypes.DeleteAlbum. " +
                "Use Task<string?> EditAlbum(EditAlbumRequest? editAlbumRequest, CancellationToken ct = default) to edit an album");
        }

        var fileNames = string.Join(" ", albumRequest.Files);
        if (string.IsNullOrWhiteSpace(fileNames))
            throw new ArgumentNullException(nameof(albumRequest.Files), "File list cannot be empty");

        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(albumRequest.Request.ToRequest()), CatBoxRequestStrings.RequestType },
            { new StringContent(albumRequest.UserHash), CatBoxRequestStrings.UserHashType },
            { new StringContent(albumRequest.AlbumId), CatBoxRequestStrings.AlbumIdShortType }
        };

        // If request type is AddToAlbum or RemoveFromAlbum
        if (albumRequest.Request is CatBoxRequestTypes.AddToAlbum or CatBoxRequestTypes.RemoveFromAlbum)
            content.Add(new StringContent(fileNames), CatBoxRequestStrings.FileType);

        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsync(ct);
    }

    /// <summary>
    /// 1. Filter Invalid Request Types on the Album Endpoint <br/>
    /// 2. Check that the user hash is not null, empty, or whitespace when attempting to modify or delete an album. User hash is required for those operations
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private static bool IsAlbumRequestTypeValid(AlbumRequest request)
    {
        switch (request.Request)
        {
            case CatBoxRequestTypes.CreateAlbum:
            case CatBoxRequestTypes.EditAlbum when !string.IsNullOrWhiteSpace(request.UserHash):
            case CatBoxRequestTypes.AddToAlbum when !string.IsNullOrWhiteSpace(request.UserHash):
            case CatBoxRequestTypes.RemoveFromAlbum when !string.IsNullOrWhiteSpace(request.UserHash):
            case CatBoxRequestTypes.DeleteAlbum when !string.IsNullOrWhiteSpace(request.UserHash):
                return true;

            case CatBoxRequestTypes.UploadFile:
            case CatBoxRequestTypes.UrlUpload:
            case CatBoxRequestTypes.DeleteFile:
            default:
                return false;
        }
    }
}