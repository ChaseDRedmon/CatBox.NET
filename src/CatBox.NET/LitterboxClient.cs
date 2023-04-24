using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using static CatBox.NET.Common;

namespace CatBox.NET;

public interface ILitterboxClient
{
    /// <summary>
    /// Enables uploading multiple files from disk (FileStream) to the API
    /// </summary>
    /// <param name="temporaryFileUploadRequest"></param>
    /// <param name="ct">Cancellation Token</param>
    /// <exception cref="ArgumentNullException">When <see cref="FileUploadRequest"/> is null</exception>
    /// <returns>Response string from the API</returns>
    IAsyncEnumerable<string?> UploadMultipleImages(TemporaryFileUploadRequest temporaryFileUploadRequest, CancellationToken ct = default);

    /// <summary>
    /// Streams a single image to be uploaded
    /// </summary>
    /// <param name="temporaryStreamUploadRequest"></param>
    /// <param name="ct">Cancellation Token</param>
    /// <exception cref="ArgumentNullException">When <see cref="StreamUploadRequest"/> is null</exception>
    /// <exception cref="ArgumentNullException">When <see cref="StreamUploadRequest.FileName"/> is null</exception>
    /// <exception cref="HttpRequestException"> when something bad happens when talking to the API</exception>
    /// <returns>Response string from the API</returns>
    Task<string?> UploadImage(TemporaryStreamUploadRequest temporaryStreamUploadRequest, CancellationToken ct = default);
}

public class LitterboxClient : ILitterboxClient
{
    private readonly HttpClient _client;
    private readonly CatBoxConfig _config;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <param name="config"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public LitterboxClient(HttpClient client, IOptions<CatBoxConfig> config)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client), "HttpClient cannot be null");

        if (config.Value.LitterboxUrl is null)
            throw new ArgumentNullException(nameof(config.Value.CatBoxUrl), "CatBox API URL cannot be null. Check that URL was set by calling .AddCatBoxServices(f => f.CatBoxUrl = new Uri(\"https://litterbox.catbox.moe/resources/internals/api.php\"))");

        _config = config.Value;
    }
    
    /// <inheritdoc/>
    public async IAsyncEnumerable<string?> UploadMultipleImages(TemporaryFileUploadRequest temporaryFileUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (temporaryFileUploadRequest is null)
            throw new ArgumentNullException(nameof(temporaryFileUploadRequest), "Argument cannot be null");

        foreach (var imageFile in temporaryFileUploadRequest.Files.Where(static f => IsFileExtensionValid(f.Extension)))
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, _config.LitterboxUrl);
            using var content = new MultipartFormDataContent();
            
            await using var fileStream = File.OpenRead(imageFile.FullName);
            content.Add(new StringContent(temporaryFileUploadRequest.ExpireAfter.ToRequest()), CatBoxRequestStrings.ExpiryType);
            content.Add(new StringContent(CatBoxRequestTypes.UploadFile.ToRequest()), CatBoxRequestStrings.RequestType);
            content.Add(new StreamContent(fileStream), CatBoxRequestStrings.FileToUploadType, imageFile.Name);
            request.Content = content;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            // response.EnsureSuccessStatusCode();

            yield return await response.Content.ReadAsStringAsync();
        }
    }
    
    /// <inheritdoc/>
    public async Task<string?> UploadImage(TemporaryStreamUploadRequest temporaryStreamUploadRequest, CancellationToken ct = default)
    {
        if (temporaryStreamUploadRequest is null)
            throw new ArgumentNullException(nameof(temporaryStreamUploadRequest), "Argument cannot be null");

        if (temporaryStreamUploadRequest.FileName is null)
            throw new ArgumentNullException(nameof(temporaryStreamUploadRequest.FileName), "Argument cannot be null");

        using var request = new HttpRequestMessage(HttpMethod.Post, _config.LitterboxUrl);
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(temporaryStreamUploadRequest.ExpireAfter.ToRequest()), CatBoxRequestStrings.ExpiryType);
        content.Add(new StringContent(CatBoxRequestTypes.UploadFile.ToRequest()), CatBoxRequestStrings.RequestType);
        content.Add(new StreamContent(temporaryStreamUploadRequest.Stream), CatBoxRequestStrings.FileToUploadType, temporaryStreamUploadRequest.FileName);
        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        // response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}