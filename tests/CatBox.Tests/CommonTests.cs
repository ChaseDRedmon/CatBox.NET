using CatBox.NET.Client;
using CatBox.NET.Enums;
using CatBox.NET.Requests.Album.Create;
using CatBox.NET.Requests.Album.Modify;
using NUnit.Framework;
using Shouldly;

namespace CatBox.Tests;

[TestFixture]
public class CommonTests
{
    [TestCase(".exe", "malware.exe")]
    [TestCase(".scr", "screensaver.scr")]
    [TestCase(".cpl", "control.cpl")]
    [TestCase(".jar", "application.jar")]
    [TestCase(".doc", "document.doc")]
    [TestCase(".docx", "document.docx")]
    public void IsFileExtensionValid_WithInvalidExtensions_ReturnsFalse(string extension, string filename)
    {
        // Arrange
        var file = new FileInfo(filename);

        // Act
        var result = Common.IsFileExtensionValid(file);

        // Assert
        result.ShouldBeFalse();
    }

    [TestCase(".jpg", "image.jpg")]
    [TestCase(".png", "image.png")]
    [TestCase(".gif", "animation.gif")]
    [TestCase(".mp4", "video.mp4")]
    public void IsFileExtensionValid_WithValidExtensions_ReturnsTrue(string extension, string filename)
    {
        // Arrange
        var file = new FileInfo(filename);

        // Act
        var result = Common.IsFileExtensionValid(file);

        // Assert
        result.ShouldBeTrue();
    }

    [Test]
    public void ThrowIfAlbumCreationRequestIsInvalid_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Common.ThrowIfAlbumCreationRequestIsInvalid(null!));
    }

    [Test]
    public void ThrowIfAlbumCreationRequestIsInvalid_WithNullTitle_ThrowsArgumentException()
    {
        // Arrange
        var request = new RemoteCreateAlbumRequest
        {
            Title = null!,
            Description = "Test Description",
            UserHash = "test-hash",
            Files = ["file1.jpg"]
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => Common.ThrowIfAlbumCreationRequestIsInvalid(request));
    }

    [Test]
    public void ThrowIfAlbumCreationRequestIsInvalid_WithWhitespaceTitle_ThrowsArgumentException()
    {
        // Arrange
        var request = new RemoteCreateAlbumRequest
        {
            Title = "   ",
            Description = "Test Description",
            UserHash = "test-hash",
            Files = ["file1.jpg"]
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => Common.ThrowIfAlbumCreationRequestIsInvalid(request));
    }

    [Test]
    public void ThrowIfAlbumCreationRequestIsInvalid_WithNullDescription_ThrowsArgumentException()
    {
        // Arrange
        var request = new RemoteCreateAlbumRequest
        {
            Title = "Test Title",
            Description = null!,
            UserHash = "test-hash",
            Files = ["file1.jpg"]
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => Common.ThrowIfAlbumCreationRequestIsInvalid(request));
    }

    [Test]
    public void ThrowIfAlbumCreationRequestIsInvalid_WithWhitespaceDescription_ThrowsArgumentException()
    {
        // Arrange
        var request = new RemoteCreateAlbumRequest
        {
            Title = "Test Title",
            Description = "   ",
            UserHash = "test-hash",
            Files = ["file1.jpg"]
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => Common.ThrowIfAlbumCreationRequestIsInvalid(request));
    }

    [Test]
    public void ThrowIfAlbumCreationRequestIsInvalid_WithValidRequest_DoesNotThrow()
    {
        // Arrange
        var request = new RemoteCreateAlbumRequest
        {
            Title = "Test Title",
            Description = "Test Description",
            UserHash = "test-hash",
            Files = ["file1.jpg"]
        };

        // Act & Assert
        Should.NotThrow(() => Common.ThrowIfAlbumCreationRequestIsInvalid(request));
    }

    private static IEnumerable<TestCaseData> ValidAlbumRequestCases()
    {
        yield return new TestCaseData(RequestType.CreateAlbum, "test-hash").SetName("CreateAlbum with UserHash");
        yield return new TestCaseData(RequestType.CreateAlbum, "").SetName("CreateAlbum without UserHash");
        yield return new TestCaseData(RequestType.EditAlbum, "test-hash").SetName("EditAlbum with UserHash");
        yield return new TestCaseData(RequestType.AddToAlbum, "test-hash").SetName("AddToAlbum with UserHash");
        yield return new TestCaseData(RequestType.RemoveFromAlbum, "test-hash").SetName("RemoveFromAlbum with UserHash");
        yield return new TestCaseData(RequestType.DeleteAlbum, "test-hash").SetName("DeleteAlbum with UserHash");
    }

    [TestCaseSource(nameof(ValidAlbumRequestCases))]
    public void IsAlbumRequestTypeValid_WithValidRequestTypeAndRequiredUserHash_ReturnsTrue(RequestType requestType, string userHash)
    {
        // Arrange
        var request = new ModifyAlbumImagesRequest
        {
            Request = requestType,
            UserHash = userHash,
            AlbumId = "abc123",
            Files = requestType == RequestType.DeleteAlbum ? Array.Empty<string>() : new[] { "file1.jpg" }
        };

        // Act
        var result = Common.IsAlbumRequestTypeValid(request);

        // Assert
        result.ShouldBeTrue();
    }

    private static IEnumerable<TestCaseData> InvalidAlbumRequestMissingUserHashCases()
    {
        yield return new TestCaseData(RequestType.EditAlbum, "").SetName("EditAlbum with empty UserHash");
        yield return new TestCaseData(RequestType.EditAlbum, null).SetName("EditAlbum with null UserHash");
        yield return new TestCaseData(RequestType.AddToAlbum, "").SetName("AddToAlbum with empty UserHash");
        yield return new TestCaseData(RequestType.AddToAlbum, null).SetName("AddToAlbum with null UserHash");
        yield return new TestCaseData(RequestType.RemoveFromAlbum, "").SetName("RemoveFromAlbum with empty UserHash");
        yield return new TestCaseData(RequestType.RemoveFromAlbum, null).SetName("RemoveFromAlbum with null UserHash");
        yield return new TestCaseData(RequestType.DeleteAlbum, "").SetName("DeleteAlbum with empty UserHash");
        yield return new TestCaseData(RequestType.DeleteAlbum, null).SetName("DeleteAlbum with null UserHash");
    }

    [TestCaseSource(nameof(InvalidAlbumRequestMissingUserHashCases))]
    public void IsAlbumRequestTypeValid_WithRequiredUserHashMissing_ReturnsFalse(RequestType requestType, string? userHash)
    {
        // Arrange
        var request = new ModifyAlbumImagesRequest
        {
            Request = requestType,
            UserHash = userHash!,
            AlbumId = "abc123",
            Files = requestType == RequestType.DeleteAlbum ? Array.Empty<string>() : new[] { "file1.jpg" }
        };

        // Act
        var result = Common.IsAlbumRequestTypeValid(request);

        // Assert
        result.ShouldBeFalse();
    }

    private static IEnumerable<TestCaseData> InvalidRequestTypeCases()
    {
        yield return new TestCaseData(RequestType.UploadFile).SetName("UploadFile RequestType");
        yield return new TestCaseData(RequestType.DeleteFile).SetName("DeleteFile RequestType");
    }

    [TestCaseSource(nameof(InvalidRequestTypeCases))]
    public void IsAlbumRequestTypeValid_WithInvalidRequestType_ReturnsFalse(RequestType requestType)
    {
        // Arrange
        var request = new ModifyAlbumImagesRequest
        {
            Request = requestType,
            UserHash = "test-hash",
            AlbumId = "abc123",
            Files = ["file1.jpg"]
        };

        // Act
        var result = Common.IsAlbumRequestTypeValid(request);

        // Assert
        result.ShouldBeFalse();
    }
}
