using CatBox.NET.Client;
using CatBox.NET.Requests;
using Moq;

namespace CatBox.Tests.Unit.Requests;

public class DeleteMultipleFilesTests
{
    private readonly Uri CatBoxUrl = new("https://catbox.moe/user/api.php");
    private readonly Uri LitterboxUrl = new("https://litterbox.catbox.moe/resources/internals/api.php");

    private readonly CatBoxClient _sut;
    private readonly HttpClient _httpClient = new Mock<HttpClient>().Object;

    public DeleteMultipleFilesTests()
    {
        var config = Helper.GetCatBoxConfig(CatBoxUrl, LitterboxUrl);
        _sut = new CatBoxClient(_httpClient, config);
    }
    
    [Fact]
    public async Task DeleteMultipleFiles_ShouldThrowArgumentNullException_When_DeleteFileRequest_IsNull()
    {
        // Arrange
        DeleteFileRequest urlUploadRequest = null;
        
        // Act
        Task<string?> fileUrls = _sut.DeleteMultipleFiles(urlUploadRequest, CancellationToken.None);
        
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => fileUrls);
    }
    
    [Fact]
    public async Task DeleteMultipleFiles_ShouldThrowArgumentNullException_When_DeleteFileRequest_UserHash_IsNull()
    {
        // Arrange
        DeleteFileRequest urlUploadRequest = new()
        {
            UserHash = null,
            FileNames = null
        };
        
        // Act
        Task<string?> fileUrls = _sut.DeleteMultipleFiles(urlUploadRequest, CancellationToken.None);
        
        // Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => fileUrls);
    }
}