using System.Runtime.CompilerServices;
using CatBox.NET.Enums;
using CatBox.NET.Requests.Litterbox;
using Microsoft.Extensions.Options;
using static CatBox.NET.Client.Common;

namespace CatBox.NET.Client;

public class LitterboxClient : ILitterboxClient
{
    private readonly HttpClient _client;
    private readonly CatboxOptions _catboxOptions;

    /// <summary>
    /// Creates a new <see cref="LitterboxClient"/>
    /// </summary>
    /// <param name="client"><see cref="HttpClient"/></param>
    /// <param name="catboxOptions"><see cref="IOptions{TOptions}"/></param>
    /// <exception cref="ArgumentNullException"></exception>
    public LitterboxClient(HttpClient client, IOptions<CatboxOptions> catboxOptions)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client), "HttpClient cannot be null");

        if (catboxOptions.Value.LitterboxUrl is null)
            throw new ArgumentNullException(nameof(catboxOptions.Value.CatBoxUrl), "CatBox API URL cannot be null. Check that URL was set by calling .AddCatBoxServices(f => f.CatBoxUrl = new Uri(\"https://litterbox.catbox.moe/resources/internals/api.php\"))");

        _catboxOptions = catboxOptions.Value;
    }
    
    /// <inheritdoc/>
    public async IAsyncEnumerable<string?> UploadMultipleImages(TemporaryFileUploadRequest temporaryFileUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (temporaryFileUploadRequest is null)
            throw new ArgumentNullException(nameof(temporaryFileUploadRequest), "Argument cannot be null");

        foreach (var imageFile in temporaryFileUploadRequest.Files.Where(static f => IsFileExtensionValid(f.Extension)))
        {
            await using var fileStream = File.OpenRead(imageFile.FullName);
            
            using var request = new HttpRequestMessage(HttpMethod.Post, _catboxOptions.LitterboxUrl);
            using var content = new MultipartFormDataContent
            {
                { new StringContent(temporaryFileUploadRequest.Expiry.ToRequest()), RequestParameters.Expiry },
                { new StringContent(ApiAction.UploadFile), RequestParameters.Request },
                { new StreamContent(fileStream), RequestParameters.FileToUpload, imageFile.Name }
            };
            request.Content = content;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            yield return await response.Content.ReadAsStringAsyncCore(ct);
        }
    }
    
    /// <inheritdoc/>
    public async Task<string?> UploadImage(TemporaryStreamUploadRequest temporaryStreamUploadRequest, CancellationToken ct = default)
    {
        if (temporaryStreamUploadRequest is null)
            throw new ArgumentNullException(nameof(temporaryStreamUploadRequest), "Argument cannot be null");

        if (temporaryStreamUploadRequest.FileName is null)
            throw new ArgumentNullException(nameof(temporaryStreamUploadRequest.FileName), "Argument cannot be null");

        using var request = new HttpRequestMessage(HttpMethod.Post, _catboxOptions.LitterboxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(temporaryStreamUploadRequest.Expiry.ToRequest()), RequestParameters.Expiry },
            { new StringContent(ApiAction.UploadFile), RequestParameters.Request },
            { new StreamContent(temporaryStreamUploadRequest.Stream), RequestParameters.FileToUpload, temporaryStreamUploadRequest.FileName }
        };
        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsyncCore(ct);
    }
}