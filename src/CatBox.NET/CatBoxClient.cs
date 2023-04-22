using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;

namespace CatBox.NET;

public interface ICatBoxClient
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileUploadRequest"></param>
    /// <param name="ct"></param>
    /// <exception cref="HttpRequestException">if response does not have successful HTTP status code</exception>
    /// <returns></returns>
    IAsyncEnumerable<string> UploadMultipleImages(FileUploadRequest fileUploadRequest, CancellationToken ct = default);
}

public class CatBoxClient : ICatBoxClient
{
    private readonly HttpClient _client;
    private readonly CatBoxConfig _config;
    private const string RequestType = "reqtype";
    private const string UserHashType = "userhash";
    private const string UrlType = "url";

    public CatBoxClient(HttpClient client, IOptions<CatBoxConfig> config)
    {
        _client = client;
        _config = config.Value;
    }

    public async IAsyncEnumerable<string> UploadMultipleImages(FileUploadRequest fileUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent();

        foreach (var imageFile in fileUploadRequest.Files.Where(static f => CheckExtension(f.Extension)))
        {
            await using var fileStream = File.OpenRead(imageFile.FullName);
            content.Add(new StringContent(CatBoxRequestTypes.UploadFile.ToRequest()), RequestType);
            content.Add(new StringContent(fileUploadRequest.UserHash), UserHashType);
            content.Add(new StreamContent(fileStream), "fileToUpload", imageFile.Name);

            using var response = await _client.PostAsync(_config.CatBoxUrl, content, ct);
            response.EnsureSuccessStatusCode();

            yield return await response.Content.ReadAsStringAsync();
        }
    }

    public async IAsyncEnumerable<string> UploadMultipleUrls(UrlUploadRequest urlUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _config.CatBoxUrl);
        using var content = new MultipartFormDataContent();
        
        foreach (var fileUrl in urlUploadRequest.Files)
        {
            content.Add(new StringContent(CatBoxRequestTypes.UrlUpload.ToRequest()), RequestType);
            content.Add(new StringContent(urlUploadRequest.UserHash), UserHashType);
            content.Add(new StringContent(fileUrl.AbsoluteUri), UrlType);

            using var response = await _client.PostAsync(_config.CatBoxUrl, content, ct);
            response.EnsureSuccessStatusCode();

            yield return await response.Content.ReadAsStringAsync();
        }
    }

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