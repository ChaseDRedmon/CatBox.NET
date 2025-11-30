using System.Runtime.CompilerServices;
using CatBox.NET.Enums;
using CatBox.NET.Requests.File;
using CatBox.NET.Requests.Litterbox;
using Microsoft.Extensions.Options;
using static CatBox.NET.Client.Common;

namespace CatBox.NET.Client;

public interface ILitterboxClient
{
    /// <summary>
    /// Enables uploading multiple files from disk (FileStream) to the API
    /// </summary>
    /// <param name="temporaryFileUploadRequest"></param>
    /// <param name="ct">Cancellation Token</param>
    /// <exception cref="ArgumentNullException">When <see cref="FileUploadRequest"/> is null</exception>
    /// <returns>Response string from the API</returns>
    IAsyncEnumerable<string?> UploadMultipleImagesAsync(TemporaryFileUploadRequest temporaryFileUploadRequest, CancellationToken ct = default);

    /// <summary>
    /// Streams a single image to be uploaded
    /// </summary>
    /// <param name="temporaryStreamUploadRequest"></param>
    /// <param name="ct">Cancellation Token</param>
    /// <exception cref="ArgumentNullException">When <see cref="StreamUploadRequest"/> is null</exception>
    /// <exception cref="ArgumentNullException">When <see cref="StreamUploadRequest.FileName"/> is null</exception>
    /// <exception cref="HttpRequestException"> when something bad happens when talking to the API</exception>
    /// <returns>Response string from the API</returns>
    Task<string?> UploadImageAsync(TemporaryStreamUploadRequest temporaryStreamUploadRequest, CancellationToken ct = default);
}

public sealed class LitterboxClient : ILitterboxClient
{
    private const long MaxFileSize = 1_073_741_824L; // 1GB in bytes

    private readonly HttpClient _client;
    private readonly CatboxOptions _catboxOptions;

    /// <summary>
    /// Creates a new <see cref="LitterboxClient"/>
    /// </summary>
    /// <param name="client"><see cref="HttpClient"/></param>
    /// <param name="catboxOptions"><see cref="IOptions{TOptions}"/></param>
    /// <exception cref="ArgumentNullException"> when <see cref="HttpClient"/> is null</exception>
    /// /// <exception cref="ArgumentNullException"> when <see cref="CatboxOptions.LitterboxUrl"/> is null</exception>
    /// <remarks>LitterboxUrl API URL cannot be null. Check that URL was set by calling: <br/><code>.AddCatBoxServices(f => f.LitterboxUrl = new Uri(\"https://litterbox.catbox.moe/resources/internals/api.php\"));</code></remarks>
    public LitterboxClient(HttpClient client, IOptions<CatboxOptions> catboxOptions)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(catboxOptions?.Value?.LitterboxUrl);

        _client = client;
        _catboxOptions = catboxOptions!.Value!;
    }
    
    /// <inheritdoc/>
    public async IAsyncEnumerable<string?> UploadMultipleImagesAsync(TemporaryFileUploadRequest temporaryFileUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(temporaryFileUploadRequest);

        foreach (var imageFile in temporaryFileUploadRequest.Files.Where(IsFileExtensionValid))
        {
            ct.ThrowIfCancellationRequested();
            await using var fileStream = File.OpenRead(imageFile.FullName);

            Throw.IfLitterboxFileSizeExceeds(fileStream.Length, MaxFileSize);

            using var response = await _client.PostAsync(_catboxOptions.LitterboxUrl, new MultipartFormDataContent
            {
                { new StringContent(RequestType.UploadFile), RequestParameters.Request },
                { new StringContent(temporaryFileUploadRequest.Expiry), RequestParameters.Expiry },
                { new StreamContent(fileStream), RequestParameters.FileToUpload, imageFile.Name }
            }, ct).ConfigureAwait(false);
            
            yield return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        }
    }
    
    /// <inheritdoc/>
    public async Task<string?> UploadImageAsync(TemporaryStreamUploadRequest temporaryStreamUploadRequest, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(temporaryStreamUploadRequest?.FileName);
        ct.ThrowIfCancellationRequested();

        if (temporaryStreamUploadRequest!.Stream.CanSeek)
            Throw.IfLitterboxFileSizeExceeds(temporaryStreamUploadRequest.Stream.Length, MaxFileSize);

        using var response = await _client.PostAsync(_catboxOptions.LitterboxUrl, new MultipartFormDataContent
        {
            { new StringContent(RequestType.UploadFile), RequestParameters.Request },
            { new StringContent(temporaryStreamUploadRequest!.Expiry), RequestParameters.Expiry },
            {
                new StreamContent(temporaryStreamUploadRequest.Stream), RequestParameters.FileToUpload,
                temporaryStreamUploadRequest.FileName
            }
        }, ct).ConfigureAwait(false);
        
        return await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }
}