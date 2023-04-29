using CatBox.NET;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;

namespace CatBox.Tests.Unit;

public class CatBoxClientTests
{
    private readonly Uri CatBoxUrl = new("https://catbox.moe/user/api.php");
    private readonly Uri LitterboxUrl = new("https://litterbox.catbox.moe/resources/internals/api.php");

    private readonly CatBoxClient _sut;
    private readonly HttpClient _httpClient = new Mock<HttpClient>().Object;

    public CatBoxClientTests()
    {
        var config = GetCatBoxConfig(CatBoxUrl, LitterboxUrl);
        _sut = new CatBoxClient(_httpClient, config);
    }
    
    [Fact]
    public void UploadImagesMultiple_ShouldThrowArgumentNullException_WhenFileUploadRequestIsNull()
    {
        // Arrange
        FileUploadRequest fileUploadRequest = null;
        
        // Act
        IAsyncEnumerable<string?> fileUrls = _sut.UploadMultipleImages(fileUploadRequest, CancellationToken.None);
        
        // Assert
        fileUrls.Enumerating(x => x.ToEnumerable().First()).Should().Throw<ArgumentNullException>();
    }

    private static IOptions<CatBoxConfig> GetCatBoxConfig(Uri catboxUrl, Uri litterboxUrl)
    {
        return Options.Create(
            new CatBoxConfig
            {
                CatBoxUrl = catboxUrl,
                LitterboxUrl = litterboxUrl
            }
        );
    }
}