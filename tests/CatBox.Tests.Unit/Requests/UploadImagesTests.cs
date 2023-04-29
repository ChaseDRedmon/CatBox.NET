using CatBox.NET;
using Moq;

namespace CatBox.Tests.Unit.Requests;

public class UploadImagesTests
{
    private readonly Uri CatBoxUrl = new("https://catbox.moe/user/api.php");
    private readonly Uri LitterboxUrl = new("https://litterbox.catbox.moe/resources/internals/api.php");

    private readonly CatBoxClient _sut;
    private readonly HttpClient _httpClient = new Mock<HttpClient>().Object;

    public UploadImagesTests()
    {
        var config = Helper.GetCatBoxConfig(CatBoxUrl, LitterboxUrl);
        _sut = new CatBoxClient(_httpClient, config);
    }
    
    [Fact]
    public async Task UploadImages_ShouldThrowArgumentNullException_When_StreamUploadRequest_IsNull()
    {
        // Arrange
        StreamUploadRequest fileUploadRequest = null;
        
        // Act
        Task<string?> fileUrls = _sut.UploadImage(fileUploadRequest, CancellationToken.None);
        
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => fileUrls);
    }
    
    [Fact]
    public async Task UploadImages_ShouldThrowArgumentNullException_When_StreamUploadRequest_Filename_IsNull()
    {
        // Arrange
        StreamUploadRequest fileUploadRequest = new StreamUploadRequest
        {
            UserHash = null,
            FileName = null,
            Stream = null
        };
        
        // Act
        Task<string?> fileUrls = _sut.UploadImage(fileUploadRequest, CancellationToken.None);
        
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => fileUrls);
    }
}