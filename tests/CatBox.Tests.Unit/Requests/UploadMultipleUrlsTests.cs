using CatBox.NET.Client;
using CatBox.NET.Requests;
using FluentAssertions;
using Moq;

namespace CatBox.Tests.Unit.Requests;

public class UploadMultipleUrlsTests
{
    private readonly Uri CatBoxUrl = new("https://catbox.moe/user/api.php");
    private readonly Uri LitterboxUrl = new("https://litterbox.catbox.moe/resources/internals/api.php");

    private readonly CatBoxClient _sut;
    private readonly HttpClient _httpClient = new Mock<HttpClient>().Object;

    public UploadMultipleUrlsTests()
    {
        var config = Helper.GetCatBoxConfig(CatBoxUrl, LitterboxUrl);
        _sut = new CatBoxClient(_httpClient, config);
    }
    
    [Fact]
    public void UploadMultipleUrls_ShouldThrowArgumentNullException_When_FileUploadRequest_IsNull()
    {
        // Arrange
        UrlUploadRequest urlUploadRequest = null;
        
        // Act
        IAsyncEnumerable<string?> fileUrls = _sut.UploadMultipleUrls(urlUploadRequest, CancellationToken.None);
        
        // Assert
        fileUrls.Enumerating(x => x.ToEnumerable().First()).Should().Throw<ArgumentNullException>();
    }
}