using System.Runtime.CompilerServices;
using CatBox.NET.Enums;
using CatBox.NET.Requests.CatBox;
using Microsoft.Extensions.Options;
using static CatBox.NET.Client.Common;

namespace CatBox.NET.Client;

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
        Throw.IfNull(fileUploadRequest);

        foreach (var imageFile in fileUploadRequest.Files.Where(static f => IsFileExtensionValid(f.Extension)))
        {
            await using var fileStream = File.OpenRead(imageFile.FullName);
            
            using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
            using var content = new MultipartFormDataContent
            {
                { new StringContent(CatBoxRequestTypes.UploadFile.ToRequest()), CatBoxRequestStrings.RequestType },
                { new StreamContent(fileStream), CatBoxRequestStrings.FileToUploadType, imageFile.Name }
            };

            if (!string.IsNullOrWhiteSpace(fileUploadRequest.UserHash))
                content.Add(new StringContent(fileUploadRequest.UserHash), CatBoxRequestStrings.UserHashType);
            
            request.Content = content;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            yield return await response.Content.ReadAsStringAsyncCore(ct);
        }
    }
    
    /// <inheritdoc/>
    public async Task<string?> UploadImage(StreamUploadRequest fileUploadRequest, CancellationToken ct = default)
    {
        Throw.IfNull(fileUploadRequest);
        Throw.IfStringIsNullOrWhitespace(fileUploadRequest.FileName, "Argument cannot be null, empty, or whitespace");

        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(CatBoxRequestTypes.UploadFile.ToRequest()), CatBoxRequestStrings.RequestType },
            { new StreamContent(fileUploadRequest.Stream), CatBoxRequestStrings.FileToUploadType, fileUploadRequest.FileName }
        };

        if (!string.IsNullOrWhiteSpace(fileUploadRequest.UserHash))
            content.Add(new StringContent(fileUploadRequest.UserHash), CatBoxRequestStrings.UserHashType);
        
        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsyncCore(ct);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string?> UploadMultipleUrls(UrlUploadRequest urlUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        Throw.IfNull(urlUploadRequest);

        foreach (var fileUrl in urlUploadRequest.Files)
        {
            if (fileUrl is null) continue;
            
            using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
            using var content = new MultipartFormDataContent // Disposing of MultipartFormDataContent, cascades disposal of String / Stream / Content classes
            {
                { new StringContent(CatBoxRequestTypes.UrlUpload.ToRequest()), CatBoxRequestStrings.RequestType },
                { new StringContent(fileUrl.AbsoluteUri), CatBoxRequestStrings.UrlType }
            }; 

            if (!string.IsNullOrWhiteSpace(urlUploadRequest.UserHash))
                content.Add(new StringContent(urlUploadRequest.UserHash), CatBoxRequestStrings.UserHashType);
            
            request.Content = content;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            yield return await response.Content.ReadAsStringAsyncCore(ct);
        }
    }

    /// <inheritdoc/>
    public async Task<string?> DeleteMultipleFiles(DeleteFileRequest deleteFileRequest, CancellationToken ct = default)
    {
        Throw.IfNull(deleteFileRequest);
        Throw.IfStringIsNullOrWhitespace(deleteFileRequest.UserHash, "Argument cannot be null, empty, or whitespace");

        var fileNames = string.Join(" ", deleteFileRequest.FileNames);
        Throw.IfStringIsNullOrWhitespace(fileNames, "File list cannot be empty");

        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(CatBoxRequestTypes.DeleteFile.ToRequest()), CatBoxRequestStrings.RequestType },
            { new StringContent(deleteFileRequest.UserHash), CatBoxRequestStrings.UserHashType },
            { new StringContent(fileNames), CatBoxRequestStrings.FileType }
        };
        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsyncCore(ct);
    }
    
    /// <inheritdoc/>
    public async Task<string?> CreateAlbum(RemoteCreateAlbumRequest remoteCreateAlbumRequest, CancellationToken ct = default)
    {
        ThrowIfAlbumCreationRequestIsInvalid(remoteCreateAlbumRequest);

        var links = remoteCreateAlbumRequest.Files.Select(link =>
        {
            if (link?.Contains(_config.CatBoxUrl!.Host) is true)
            {
                return new Uri(link).PathAndQuery[1..];
            }

            return link;
        });
        
        var fileNames = string.Join(" ", links);
        Throw.IfStringIsNullOrWhitespace(fileNames, "File list cannot be empty");
        
        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(CatBoxRequestTypes.CreateAlbum.ToRequest()), CatBoxRequestStrings.RequestType },
            { new StringContent(remoteCreateAlbumRequest.Title), CatBoxRequestStrings.TitleType },
            { new StringContent(fileNames), CatBoxRequestStrings.FileType }
        };

        if (!string.IsNullOrWhiteSpace(remoteCreateAlbumRequest.UserHash))
            content.Add(new StringContent(remoteCreateAlbumRequest.UserHash), CatBoxRequestStrings.UserHashType);

        if (!string.IsNullOrWhiteSpace(remoteCreateAlbumRequest.Description))
            content.Add(new StringContent(remoteCreateAlbumRequest.Description), CatBoxRequestStrings.DescriptionType);

        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsyncCore(ct);
    }

    /// <inheritdoc/>
#pragma warning disable CS0618 // API is not Obsolete, but should warn the user of dangerous functionality
    public async Task<string?> EditAlbum(EditAlbumRequest editAlbumRequest, CancellationToken ct = default)
#pragma warning restore CS0618 // API is not Obsolete, but should warn the user of dangerous functionality
    {
        Throw.IfNull(editAlbumRequest);
        Throw.IfStringIsNullOrWhitespace(editAlbumRequest.UserHash, "UserHash cannot be null, empty, or whitespace when attempting to modify an album");
        Throw.IfStringIsNullOrWhitespace(editAlbumRequest.Description, "Album description cannot be null, empty, or whitespace");
        Throw.IfStringIsNullOrWhitespace(editAlbumRequest.Title, "Album title cannot be null, empty, or whitespace");
        Throw.IfStringIsNullOrWhitespace(editAlbumRequest.AlbumId, "AlbumId (Short) cannot be null, empty, or whitespace");
        
        var fileNames = string.Join(" ", editAlbumRequest.Files);
        Throw.IfStringIsNullOrWhitespace(fileNames, "File list cannot be empty");

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
        return await response.Content.ReadAsStringAsyncCore(ct: ct);
    }
    
    /// <inheritdoc/>
    public async Task<string?> ModifyAlbum(ModifyAlbumImagesRequest modifyAlbumImagesRequest, CancellationToken ct = default)
    {
        Throw.IfNull(modifyAlbumImagesRequest);
        Throw.IfStringIsNullOrWhitespace(modifyAlbumImagesRequest.UserHash, "UserHash cannot be null, empty, or whitespace when attempting to modify an album");
        
        if (IsAlbumRequestTypeValid(modifyAlbumImagesRequest))
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            throw new ArgumentException("Invalid Request Type for album endpoint", nameof(modifyAlbumImagesRequest.Request));
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

        if (modifyAlbumImagesRequest.Request != CatBoxRequestTypes.AddToAlbum &&
            modifyAlbumImagesRequest.Request != CatBoxRequestTypes.RemoveFromAlbum &&
            modifyAlbumImagesRequest.Request != CatBoxRequestTypes.DeleteAlbum)
        {
            throw new InvalidOperationException(
                "The ModifyAlbum method only supports CatBoxRequestTypes.AddToAlbum, CatBoxRequestTypes.RemoveFromAlbum, and CatBoxRequestTypes.DeleteAlbum. " +
                "Use Task<string?> EditAlbum(EditAlbumRequest? editAlbumRequest, CancellationToken ct = default) to edit an album");
        }

        var fileNames = string.Join(" ", modifyAlbumImagesRequest.Files);
        Throw.IfStringIsNullOrWhitespace(fileNames, "File list cannot be empty");

        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(modifyAlbumImagesRequest.Request.ToRequest()), CatBoxRequestStrings.RequestType },
            { new StringContent(modifyAlbumImagesRequest.UserHash), CatBoxRequestStrings.UserHashType },
            { new StringContent(modifyAlbumImagesRequest.AlbumId), CatBoxRequestStrings.AlbumIdShortType }
        };

        // If request type is AddToAlbum or RemoveFromAlbum
        if (modifyAlbumImagesRequest.Request is CatBoxRequestTypes.AddToAlbum or CatBoxRequestTypes.RemoveFromAlbum)
            content.Add(new StringContent(fileNames), CatBoxRequestStrings.FileType);

        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsyncCore(ct: ct);

        // TODO: Find API Error Messages for Missing UserHashes and other required parameters
    }
    
    /// <summary>
    /// Validates an Album Creation Request
    /// </summary>
    /// <param name="request">The album creation request to validate</param>
    /// <exception cref="ArgumentNullException">when the request is null</exception>
    /// <exception cref="ArgumentNullException">when the description is null</exception>
    /// <exception cref="ArgumentNullException">when the title is null</exception>
    private void ThrowIfAlbumCreationRequestIsInvalid(AlbumCreationRequest request)
    {
        Throw.IfNull(request);
        Throw.IfStringIsNullOrWhitespace(request.Description, "Album description cannot be null, empty, or whitespace");
        Throw.IfStringIsNullOrWhitespace(request.Title, "Album title cannot be null, empty, or whitespace");
    }
    
    /// <summary>
    /// 1. Filter Invalid Request Types on the Album Endpoint <br/>
    /// 2. Check that the user hash is not null, empty, or whitespace when attempting to modify or delete an album. User hash is required for those operations
    /// </summary>
    /// <param name="imagesRequest"></param>
    /// <returns></returns>
    private static bool IsAlbumRequestTypeValid(ModifyAlbumImagesRequest imagesRequest)
    {
        switch (imagesRequest.Request)
        {
            case CatBoxRequestTypes.CreateAlbum:
            case CatBoxRequestTypes.EditAlbum when !string.IsNullOrWhiteSpace(imagesRequest.UserHash):
            case CatBoxRequestTypes.AddToAlbum when !string.IsNullOrWhiteSpace(imagesRequest.UserHash):
            case CatBoxRequestTypes.RemoveFromAlbum when !string.IsNullOrWhiteSpace(imagesRequest.UserHash):
            case CatBoxRequestTypes.DeleteAlbum when !string.IsNullOrWhiteSpace(imagesRequest.UserHash):
                return true;

            case CatBoxRequestTypes.UploadFile:
            case CatBoxRequestTypes.UrlUpload:
            case CatBoxRequestTypes.DeleteFile:
            default:
                return false;
        }
    }
}