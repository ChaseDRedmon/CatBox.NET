using System.Runtime.CompilerServices;
using CatBox.NET.Enums;
using CatBox.NET.Requests.CatBox;
using Microsoft.Extensions.Options;
using static CatBox.NET.Client.Common;

namespace CatBox.NET.Client;

public class CatBoxClient : ICatBoxClient
{
    private readonly HttpClient _client;
    private readonly CatboxOptions _catboxOptions;

    /// <summary>
    /// Creates a new <see cref="CatBoxClient"/>
    /// </summary>
    /// <param name="client"><see cref="HttpClient"/></param>
    /// <param name="catboxOptions"><see cref="IOptions{TOptions}"/></param>
    /// <exception cref="ArgumentNullException"></exception>
    public CatBoxClient(HttpClient client, IOptions<CatboxOptions> catboxOptions)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client), "HttpClient cannot be null");

        if (catboxOptions.Value.CatBoxUrl is null)
            throw new ArgumentNullException(nameof(catboxOptions.Value.CatBoxUrl), "CatBox API URL cannot be null. Check that URL was set by calling .AddCatBoxServices(f => f.CatBoxUrl = new Uri(\"https://catbox.moe/user/api.php\"))");

        _catboxOptions = catboxOptions.Value;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string?> UploadFiles(FileUploadRequest fileUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        Throw.IfNull(fileUploadRequest);

        foreach (var imageFile in fileUploadRequest.Files.Where(static f => IsFileExtensionValid(f.Extension)))
        {
            await using var fileStream = File.OpenRead(imageFile.FullName);
            
            using var request = new HttpRequestMessage(HttpMethod.Post, _catboxOptions.CatBoxUrl);
            using var content = new MultipartFormDataContent
            {
                { new StringContent(RequestType.UploadFile.Value()), RequestParameters.Request },
                { new StreamContent(fileStream), RequestParameters.FileToUpload, imageFile.Name }
            };

            if (!string.IsNullOrWhiteSpace(fileUploadRequest.UserHash))
                content.Add(new StringContent(fileUploadRequest.UserHash), RequestParameters.UserHash);
            
            request.Content = content;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            yield return await response.Content.ReadAsStringAsyncInternal(ct);
        }
    }
    
    /// <inheritdoc/>
    public async IAsyncEnumerable<string?> UploadFilesAsStream(IEnumerable<StreamUploadRequest> fileUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        Throw.IfNull(fileUploadRequest);

        foreach (var uploadRequest in fileUploadRequest)
        {
            Throw.IfStringIsNullOrWhitespace(uploadRequest.FileName, "Argument cannot be null, empty, or whitespace");

            using var request = new HttpRequestMessage(HttpMethod.Post, _catboxOptions.CatBoxUrl);
            using var content = new MultipartFormDataContent
            {
                { new StringContent(RequestType.UploadFile.Value()), RequestParameters.Request },
                { new StreamContent(uploadRequest.Stream), RequestParameters.FileToUpload, uploadRequest.FileName }
            };

            if (!string.IsNullOrWhiteSpace(uploadRequest.UserHash))
                content.Add(new StringContent(uploadRequest.UserHash), RequestParameters.UserHash);
        
            request.Content = content;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            yield return await response.Content.ReadAsStringAsyncInternal(ct);
        }
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string?> UploadFilesAsUrl(UrlUploadRequest urlUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        Throw.IfNull(urlUploadRequest);

        foreach (var fileUrl in urlUploadRequest.Files)
        {
            if (fileUrl is null) continue;
            
            using var request = new HttpRequestMessage(HttpMethod.Post, _catboxOptions.CatBoxUrl);
            using var content = new MultipartFormDataContent // Disposing of MultipartFormDataContent, cascades disposal of String / Stream / Content classes
            {
                { new StringContent(RequestType.UrlUpload.Value()), RequestParameters.Request },
                { new StringContent(fileUrl.AbsoluteUri), RequestParameters.Url }
            }; 

            if (!string.IsNullOrWhiteSpace(urlUploadRequest.UserHash))
                content.Add(new StringContent(urlUploadRequest.UserHash), RequestParameters.UserHash);
            
            request.Content = content;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            yield return await response.Content.ReadAsStringAsyncInternal(ct);
        }
    }

    /// <inheritdoc/>
    public async Task<string?> DeleteMultipleFiles(DeleteFileRequest deleteFileRequest, CancellationToken ct = default)
    {
        Throw.IfNull(deleteFileRequest);
        Throw.IfStringIsNullOrWhitespace(deleteFileRequest.UserHash, "Argument cannot be null, empty, or whitespace");

        var fileNames = string.Join(" ", deleteFileRequest.FileNames);
        Throw.IfStringIsNullOrWhitespace(fileNames, "File list cannot be empty");

        using var request = new HttpRequestMessage(HttpMethod.Post, _catboxOptions.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(RequestType.DeleteFile.Value()), RequestParameters.Request },
            { new StringContent(deleteFileRequest.UserHash), RequestParameters.UserHash },
            { new StringContent(fileNames), RequestParameters.Files }
        };
        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsyncInternal(ct);
    }
    
    /// <inheritdoc/>
    public async Task<string?> CreateAlbum(RemoteCreateAlbumRequest remoteCreateAlbumRequest, CancellationToken ct = default)
    {
        ThrowIfAlbumCreationRequestIsInvalid(remoteCreateAlbumRequest);

        var links = remoteCreateAlbumRequest.Files.Select(link =>
        {
            if (link?.Contains(_catboxOptions.CatBoxUrl!.Host) is true)
            {
                return new Uri(link).PathAndQuery[1..];
            }

            return link;
        });
        
        var fileNames = string.Join(" ", links);
        Throw.IfStringIsNullOrWhitespace(fileNames, "File list cannot be empty");
        
        using var request = new HttpRequestMessage(HttpMethod.Post, _catboxOptions.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(RequestType.CreateAlbum.Value()), RequestParameters.Request },
            { new StringContent(remoteCreateAlbumRequest.Title), RequestParameters.Title },
            { new StringContent(fileNames), RequestParameters.Files }
        };

        if (!string.IsNullOrWhiteSpace(remoteCreateAlbumRequest.UserHash))
            content.Add(new StringContent(remoteCreateAlbumRequest.UserHash), RequestParameters.UserHash);

        if (!string.IsNullOrWhiteSpace(remoteCreateAlbumRequest.Description))
            content.Add(new StringContent(remoteCreateAlbumRequest.Description), RequestParameters.Description);

        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsyncInternal(ct);
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

        using var request = new HttpRequestMessage(HttpMethod.Post, _catboxOptions.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(RequestType.EditAlbum.Value()), RequestParameters.Request },
            { new StringContent(editAlbumRequest.UserHash), RequestParameters.UserHash },
            { new StringContent(editAlbumRequest.AlbumId), RequestParameters.AlbumIdShort },
            { new StringContent(editAlbumRequest.Title), RequestParameters.Title },
            { new StringContent(editAlbumRequest.Description), RequestParameters.Description },
            { new StringContent(fileNames), RequestParameters.Files }
        };
        
        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsyncInternal(ct: ct);
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

        if (modifyAlbumImagesRequest.Request != RequestType.AddToAlbum &&
            modifyAlbumImagesRequest.Request != RequestType.RemoveFromAlbum &&
            modifyAlbumImagesRequest.Request != RequestType.DeleteAlbum)
        {
            throw new InvalidOperationException(
                "The ModifyAlbum method only supports CatBoxRequestTypes.AddToAlbum, CatBoxRequestTypes.RemoveFromAlbum, and CatBoxRequestTypes.DeleteAlbum. " +
                "Use Task<string?> EditAlbum(EditAlbumRequest? editAlbumRequest, CancellationToken ct = default) to edit an album");
        }

        var fileNames = string.Join(" ", modifyAlbumImagesRequest.Files);
        Throw.IfStringIsNullOrWhitespace(fileNames, "File list cannot be empty");

        using var request = new HttpRequestMessage(HttpMethod.Post, _catboxOptions.CatBoxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(modifyAlbumImagesRequest.Request.Value()), RequestParameters.Request },
            { new StringContent(modifyAlbumImagesRequest.UserHash), RequestParameters.UserHash },
            { new StringContent(modifyAlbumImagesRequest.AlbumId), RequestParameters.AlbumIdShort }
        };

        if (modifyAlbumImagesRequest.Request is RequestType.AddToAlbum or RequestType.RemoveFromAlbum)
            content.Add(new StringContent(fileNames), RequestParameters.Files);

        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsyncInternal(ct: ct);
    }
}