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
    /// <exception cref="ArgumentNullException"> when <see cref="HttpClient"/> is null</exception>
    /// /// <exception cref="ArgumentNullException"> when <see cref="CatboxOptions.LitterboxUrl"/> is null</exception>
    /// <remarks>LitterboxUrl API URL cannot be null. Check that URL was set by calling: <br/><code>.AddCatBoxServices(f => f.LitterboxUrl = new Uri(\"https://litterbox.catbox.moe/resources/internals/api.php\"));</code></remarks>
    public LitterboxClient(HttpClient client, IOptions<CatboxOptions> catboxOptions)
    {
        Throw.IfNull(client);
        Throw.IfNull(catboxOptions?.Value?.LitterboxUrl);

        _client = client;
        _catboxOptions = catboxOptions!.Value!;
    }
    
    /// <inheritdoc/>
    public async IAsyncEnumerable<string?> UploadMultipleImages(TemporaryFileUploadRequest temporaryFileUploadRequest, [EnumeratorCancellation] CancellationToken ct = default)
    {
        Throw.IfNull(temporaryFileUploadRequest);

        foreach (var imageFile in temporaryFileUploadRequest.Files.Where(static f => IsFileExtensionValid(f.Extension)))
        {
            await using var fileStream = File.OpenRead(imageFile.FullName);
            
            using var request = new HttpRequestMessage(HttpMethod.Post, _catboxOptions.LitterboxUrl);
            using var content = new MultipartFormDataContent
            {
                { new StringContent(temporaryFileUploadRequest.Expiry.Value()), RequestParameters.Expiry },
                { new StringContent(RequestType.UploadFile.Value()), RequestParameters.Request },
                { new StreamContent(fileStream), RequestParameters.FileToUpload, imageFile.Name }
            };
            request.Content = content;

            using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            yield return await response.Content.ReadAsStringAsyncInternal(ct);
        }
    }
    
    /// <inheritdoc/>
    public async Task<string?> UploadImage(TemporaryStreamUploadRequest temporaryStreamUploadRequest, CancellationToken ct = default)
    {
        Throw.IfNull(temporaryStreamUploadRequest?.FileName);

        using var request = new HttpRequestMessage(HttpMethod.Post, _catboxOptions.LitterboxUrl);
        using var content = new MultipartFormDataContent
        {
            { new StringContent(temporaryStreamUploadRequest!.Expiry.Value()), RequestParameters.Expiry },
            { new StringContent(RequestType.UploadFile.Value()), RequestParameters.Request },
            { new StreamContent(temporaryStreamUploadRequest.Stream), RequestParameters.FileToUpload, temporaryStreamUploadRequest.FileName }
        };
        request.Content = content;

        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        return await response.Content.ReadAsStringAsyncInternal(ct);
    }
}