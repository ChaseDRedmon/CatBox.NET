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
    public void IfAlbumCreationRequestIsInvalid_WithNullRequest_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => Throw.IfAlbumCreationRequestIsInvalid(null!));
    }

    private static IEnumerable<TestCaseData> InvalidAlbumCreationRequestCases()
    {
        yield return new TestCaseData(null, "Test Description").SetName("Null Title");
        yield return new TestCaseData("   ", "Test Description").SetName("Whitespace Title");
        yield return new TestCaseData("Test Title", null).SetName("Null Description");
        yield return new TestCaseData("Test Title", "   ").SetName("Whitespace Description");
    }

    [TestCaseSource(nameof(InvalidAlbumCreationRequestCases))]
    public void IfAlbumCreationRequestIsInvalid_WithInvalidRequest_ThrowsArgumentException(
        string? title, string? description)
    {
        // Arrange
        var request = new RemoteCreateAlbumRequest
        {
            Title = title!,
            Description = description!,
            UserHash = "test-hash",
            Files = ["file1.jpg"]
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => Throw.IfAlbumCreationRequestIsInvalid(request));
    }

    [Test]
    public void IfAlbumCreationRequestIsInvalid_WithValidRequest_DoesNotThrow()
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
        Should.NotThrow(() => Throw.IfAlbumCreationRequestIsInvalid(request));
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

    [TestCase("https://files.catbox.moe/abc123.jpg", "abc123.jpg")]
    [TestCase("http://files.catbox.moe/xyz789.png", "xyz789.png")]
    [TestCase("https://files.catbox.moe/test.gif", "test.gif")]
    public void ToCatboxImageName_WithFullCatboxUrl_ExtractsFilename(string url, string expected)
    {
        url.ToCatboxImageName().ShouldBe(expected);
    }

    [TestCase("abc123.jpg", "abc123.jpg")]
    [TestCase("myfile.png", "myfile.png")]
    public void ToCatboxImageName_WithPlainFilename_ReturnsOriginal(string input, string expected)
    {
        input.ToCatboxImageName().ShouldBe(expected);
    }

    [TestCase("https://example.com/image.jpg")]
    [TestCase("https://otherdomain.moe/file.png")]
    public void ToCatboxImageName_WithNonCatboxUrl_ReturnsOriginal(string url)
    {
        url.ToCatboxImageName().ShouldBe(url);
    }

    [TestCase(null, null)]
    [TestCase("", "")]
    [TestCase("   ", "   ")]
    public void ToCatboxImageName_WithNullOrWhitespace_ReturnsInput(string? input, string? expected)
    {
        input.ToCatboxImageName().ShouldBe(expected);
    }

    [TestCase("https://catbox.moe/c/abc123", "abc123")]
    [TestCase("http://catbox.moe/c/xyz789", "xyz789")]
    [TestCase("https://catbox.moe/c/test", "test")]
    public void ToAlbumShortCode_WithFullAlbumUrl_ExtractsShortCode(string url, string expected)
    {
        url.ToAlbumShortCode().ShouldBe(expected);
    }

    [TestCase("abc123", "abc123")]
    [TestCase("xyz789", "xyz789")]
    public void ToAlbumShortCode_WithPlainShortCode_ReturnsOriginal(string input, string expected)
    {
        input.ToAlbumShortCode().ShouldBe(expected);
    }

    [TestCase("https://catbox.moe/other/abc123")]
    [TestCase("https://files.catbox.moe/abc123.jpg")]
    [TestCase("https://example.com/c/abc123")]
    public void ToAlbumShortCode_WithNonAlbumUrl_ReturnsOriginal(string url)
    {
        url.ToAlbumShortCode().ShouldBe(url);
    }

    [TestCase(null, null)]
    [TestCase("", "")]
    [TestCase("   ", "   ")]
    public void ToAlbumShortCode_WithNullOrWhitespace_ReturnsInput(string? input, string? expected)
    {
        input.ToAlbumShortCode().ShouldBe(expected);
    }

    [Test]
    public void IfAlbumFileLimitExceeds_At501Files_Throws()
    {
        Should.Throw<Exception>(() => Throw.IfAlbumFileLimitExceeds(501));
    }

    [Test]
    public void IfAlbumFileLimitExceeds_At500Files_DoesNotThrow()
    {
        Should.NotThrow(() => Throw.IfAlbumFileLimitExceeds(500));
    }

    [Test]
    public void IfAlbumFileLimitExceeds_At1File_DoesNotThrow()
    {
        Should.NotThrow(() => Throw.IfAlbumFileLimitExceeds(1));
    }

    [Test]
    public void IfAlbumFileLimitExceeds_WithCustomLimit_ThrowsAtExceedingLimit()
    {
        Should.Throw<Exception>(() => Throw.IfAlbumFileLimitExceeds(11, maxFiles: 10));
    }

    [Test]
    public void IfAlbumFileLimitExceeds_WithCustomLimit_DoesNotThrowAtLimit()
    {
        Should.NotThrow(() => Throw.IfAlbumFileLimitExceeds(10, maxFiles: 10));
    }
}
