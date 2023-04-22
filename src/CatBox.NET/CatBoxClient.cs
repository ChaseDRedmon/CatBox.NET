﻿using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Options;

namespace CatBox.NET;

public interface ICatBoxClient
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileUploadRequest"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    IAsyncEnumerable<string?> UploadMultipleImages(FileUploadRequest? fileUploadRequest, CancellationToken ct = default);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="urlUploadRequest"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    IAsyncEnumerable<string?> UploadMultipleUrls(UrlUploadRequest? urlUploadRequest, CancellationToken ct = default);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="deleteFileRequest"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    Task<string?> DeleteMultipleFiles(DeleteFileRequest? deleteFileRequest, CancellationToken ct = default);
    
    /// <summary>
    /// Modify an album on CatBox by file name
    /// </summary>
    /// <param name="albumRequest"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    Task ModifyAlbum(AlbumRequest? albumRequest, CancellationToken ct = default);
}

public class CatBoxClient : ICatBoxClient
{
    private readonly HttpClient _client;
    private readonly CatBoxConfig _config;

    public CatBoxClient(HttpClient client, IOptions<CatBoxConfig> config)
    {
        _client = client;
        _config = config.Value;
    }
    
    /// <inheritdoc/>
    public async IAsyncEnumerable<string?> UploadMultipleImages(FileUploadRequest? fileUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (fileUploadRequest is null)
            throw new ArgumentNullException(nameof(fileUploadRequest), "Argument cannot be null");
        
        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent();

        foreach (var imageFile in fileUploadRequest.Files.Where(static f => CheckExtension(f.Extension)))
        {
            await using var fileStream = File.OpenRead(imageFile.FullName);
            content.Add(new StringContent(CatBoxRequestTypes.UploadFile.ToRequest()), CatBoxRequestStrings.RequestType);
            content.Add(new StringContent(fileUploadRequest.UserHash), CatBoxRequestStrings.UserHashType);
            content.Add(new StreamContent(fileStream), "fileToUpload", imageFile.Name);
            request.Content = content;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            yield return await response.Content.ReadAsStringAsync();
        }
    }
    
    /// <inheritdoc/>
    public async IAsyncEnumerable<string?> UploadMultipleUrls(UrlUploadRequest? urlUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (urlUploadRequest is null)
            throw new ArgumentNullException(nameof(urlUploadRequest), "Argument cannot be null");
        
        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent(); // Disposing of MultipartFormDataContent, cascades disposal of String / Stream / Content classes
        
        foreach (var fileUrl in urlUploadRequest.Files)
        {
            if (fileUrl is null)
                continue;
            
            content.Add(new StringContent(CatBoxRequestTypes.UrlUpload.ToRequest()), CatBoxRequestStrings.RequestType);
            content.Add(new StringContent(urlUploadRequest.UserHash), CatBoxRequestStrings.UserHashType);
            content.Add(new StringContent(fileUrl.AbsoluteUri), CatBoxRequestStrings.UrlType);
            request.Content = content;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            yield return await response.Content.ReadAsStringAsync();
        }
    }
    
    /// <inheritdoc/>
    public async Task<string?> DeleteMultipleFiles(DeleteFileRequest? deleteFileRequest, CancellationToken ct = default)
    {
        if (deleteFileRequest is null)
            throw new ArgumentNullException(nameof(deleteFileRequest), "Argument cannot be null");
        
        if (string.IsNullOrWhiteSpace(deleteFileRequest.UserHash))
            throw new ArgumentNullException(nameof(deleteFileRequest.UserHash), "Argument cannot be null");
        
        var fileNames = string.Join(" ", deleteFileRequest.FileNames);
        if (string.IsNullOrWhiteSpace(fileNames))
            throw new ArgumentException("File list cannot be empty", nameof(deleteFileRequest.FileNames));
        
        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent();
        
        content.Add(new StringContent(CatBoxRequestTypes.DeleteFile.ToRequest()), CatBoxRequestStrings.RequestType);
        content.Add(new StringContent(deleteFileRequest.UserHash), CatBoxRequestStrings.UserHashType);
        content.Add(new StringContent(fileNames), CatBoxRequestStrings.FileType);
        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public Task ModifyAlbum(AlbumRequest? albumRequest, CancellationToken ct = default)
    {
        if (albumRequest is null)
            throw new ArgumentNullException(nameof(albumRequest), "Argument cannot be null");

        if (ValidateAlbumRequestType(albumRequest))
            throw new ArgumentException("Invalid Request Type for album endpoint", nameof(albumRequest.Request));
        
        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(albumRequest.Request.ToRequest()), CatBoxRequestStrings.RequestType);
        content.Add(new StringContent(albumRequest.UserHash), CatBoxRequestStrings.UserHashType);
        
        // Need to rewrite this Modify Album code because the Create Album and Edit Album endpoints take different parameters
        // Add files and remove files take the same parameters
        // Delete Album needs it's own endpoint

        throw new NotImplementedException();
    }

    /// <summary>
    /// 1. Filter Invalid Request Types on the Album Endpoint <br/>
    /// 2. Check that the user hash is not null, empty, or whitespace when attempting to modify or delete an album. User hash is required for those operations
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    private static bool ValidateAlbumRequestType(AlbumRequest request)
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

    /// <summary>
    /// These file extensions are not allowed by the API, so filter them out
    /// </summary>
    /// <param name="extension"></param>
    /// <returns></returns>
    private static bool CheckExtension(string extension)
    {
        switch (extension)
        {
            case ".exe":
            case ".scr":
            case ".cpl":
            case var _ when extension.Contains(".doc"):
            case ".jar":
                return false;
            default:
                return true;
        }
    }
}