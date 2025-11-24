using CatBox.NET;
using CatBox.NET.Client;
using CatBox.NET.Enums;
using CatBox.NET.Requests.Album.Create;
using CatBox.NET.Requests.Album.Modify;
using CatBox.NET.Requests.File;
using CatBox.NET.Requests.URL;
using CatBox.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;

namespace CatBox.Tests;

/// <summary>
/// Integration tests for CatBoxClient that make real API calls
/// Requires CATBOX_USER_HASH environment variable to be set
/// Run with: dotnet test --filter Category=Integration
/// </summary>
[TestFixture]
[Category("Integration")]
public class CatBoxClientIntegrationTests
{
    private CatBoxClient? _client;
    private static readonly List<string> _uploadedFiles = new();
    private static readonly List<string> _createdAlbums = new();
    private static readonly Lock _lock = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        if (!IntegrationTestConfig.IsConfigured)
        {
            Assert.Ignore("Integration tests skipped: CatBox:UserHash not configured. " +
                         "Set via: dotnet user-secrets set \"CatBox:UserHash\" \"your-hash\" " +
                         "or environment variable: CATBOX_USER_HASH=your-hash");
        }

        // Use DI container to properly configure the client with resilience handlers
        var services = new ServiceCollection();
        services.AddCatBoxServices(options =>
        {
            options.CatBoxUrl = IntegrationTestConfig.CatBoxUrl;
        });

        var serviceProvider = services.BuildServiceProvider();
        _client = serviceProvider.GetRequiredService<ICatBoxClient>() as CatBoxClient;
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_client == null || !IntegrationTestConfig.IsConfigured)
            return;

        try
        {
            // Delete albums first (they reference files)
            if (_createdAlbums.Count > 0)
            {
                TestContext.WriteLine($"Cleaning up {_createdAlbums.Count} album(s)...");
                foreach (var albumId in _createdAlbums)
                {
                    try
                    {
                        var deleteAlbumRequest = new ModifyAlbumImagesRequest
                        {
                            Request = RequestType.DeleteAlbum,
                            UserHash = IntegrationTestConfig.UserHash!,
                            AlbumId = albumId,
                            Files = []
                        };
                        await _client.ModifyAlbumAsync(deleteAlbumRequest);
                        TestContext.WriteLine($"Deleted album: {albumId}");
                    }
                    catch (Exception ex)
                    {
                        TestContext.WriteLine($"Failed to delete album {albumId}: {ex.Message}");
                    }
                }
            }

            // Then delete individual files
            if (_uploadedFiles.Count > 0)
            {
                TestContext.WriteLine($"Cleaning up {_uploadedFiles.Count} file(s)...");
                var deleteRequest = new DeleteFileRequest
                {
                    UserHash = IntegrationTestConfig.UserHash!,
                    FileNames = _uploadedFiles.ToList()
                };

                var result = await _client.DeleteMultipleFilesAsync(deleteRequest);
                TestContext.WriteLine($"Delete result: {result}");
            }
        }
        catch (Exception ex)
        {
            TestContext.WriteLine($"Cleanup error: {ex.Message}");
        }
    }

    private void TrackUploadedFile(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        // Extract filename from URL (e.g., "abc123.png" from "https://files.catbox.moe/abc123.png")
        var fileName = new Uri(url).Segments.Last();
        using (_lock.EnterScope())
        {
            if (!_uploadedFiles.Contains(fileName))
            {
                _uploadedFiles.Add(fileName);
                TestContext.WriteLine($"Tracked file for cleanup: {fileName}");
            }
        }
    }

    private void TrackCreatedAlbum(string? albumUrl)
    {
        if (string.IsNullOrWhiteSpace(albumUrl))
            return;

        // Extract album ID from URL (e.g., "abc123" from "https://catbox.moe/c/abc123")
        var albumId = new Uri(albumUrl).Segments.Last();
        using (_lock.EnterScope())
        {
            if (!_createdAlbums.Contains(albumId))
            {
                _createdAlbums.Add(albumId);
                TestContext.WriteLine($"Tracked album for cleanup: {albumId}");
            }
        }
    }

    [Test]
    [Order(1)]
    public async Task UploadFilesAsync_WithFileFromDisk_Succeeds()
    {
        // Arrange
        var testFilePath = IntegrationTestConfig.GetTestFilePath();
        File.Exists(testFilePath).ShouldBeTrue($"Test file not found: {testFilePath}");

        var request = new FileUploadRequest
        {
            Files = [new FileInfo(testFilePath)],
            UserHash = IntegrationTestConfig.UserHash
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in _client!.UploadFilesAsync(request))
        {
            results.Add(result);
            TrackUploadedFile(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldNotBeNullOrWhiteSpace();
        results[0].ShouldStartWith("https://files.catbox.moe/");
        TestContext.WriteLine($"Uploaded file URL: {results[0]}");
    }

    [Test]
    [Order(2)]
    public async Task UploadFilesAsStreamAsync_WithMemoryStream_Succeeds()
    {
        // Arrange
        var testFilePath = IntegrationTestConfig.GetTestFilePath();
        var fileBytes = await File.ReadAllBytesAsync(testFilePath);
        var stream = new MemoryStream(fileBytes);

        var requests = new[]
        {
            new StreamUploadRequest
            {
                FileName = "test-stream.png",
                Stream = stream,
                UserHash = IntegrationTestConfig.UserHash
            }
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in _client!.UploadFilesAsStreamAsync(requests))
        {
            results.Add(result);
            TrackUploadedFile(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldNotBeNullOrWhiteSpace();
        results[0].ShouldStartWith("https://files.catbox.moe/");
        TestContext.WriteLine($"Uploaded stream URL: {results[0]}");
    }

    [Test]
    [Order(3)]
    public async Task UploadFilesAsUrlAsync_WithPublicUrl_Succeeds()
    {
        // Arrange - Using a public SVG from Wikipedia
        var request = new UrlUploadRequest
        {
            Files = [new Uri("https://upload.wikimedia.org/wikipedia/commons/6/6b/Bitmap_VS_SVG.svg")],
            UserHash = IntegrationTestConfig.UserHash
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in _client!.UploadFilesAsUrlAsync(request))
        {
            results.Add(result);
            TrackUploadedFile(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldNotBeNullOrWhiteSpace();
        results[0].ShouldStartWith("https://files.catbox.moe/");
        TestContext.WriteLine($"Uploaded from URL: {results[0]}");
    }

    [Test]
    [Order(4)]
    public async Task CreateAlbumAsync_WithUploadedFiles_Succeeds()
    {
        // Arrange - Upload two files first
        var testFilePath = IntegrationTestConfig.GetTestFilePath();
        var uploadRequest = new FileUploadRequest
        {
            Files = [new FileInfo(testFilePath), new FileInfo(testFilePath)],
            UserHash = IntegrationTestConfig.UserHash
        };

        var uploadedFileUrls = new List<string?>();
        await foreach (var url in _client!.UploadFilesAsync(uploadRequest))
        {
            uploadedFileUrls.Add(url);
            TrackUploadedFile(url);
        }

        // Extract filenames from URLs
        var fileNames = uploadedFileUrls
            .Where(url => !string.IsNullOrWhiteSpace(url))
            .Select(url => new Uri(url!).Segments.Last())
            .ToList();

        var albumRequest = new RemoteCreateAlbumRequest
        {
            Title = "CatBox.NET Integration Test Album",
            Description = "Test album created by integration tests",
            UserHash = IntegrationTestConfig.UserHash,
            Files = fileNames
        };

        // Act
        var albumUrl = await _client.CreateAlbumAsync(albumRequest);
        TrackCreatedAlbum(albumUrl);

        // Assert
        albumUrl.ShouldNotBeNullOrWhiteSpace();
        albumUrl.ShouldStartWith("https://catbox.moe/c/");
        TestContext.WriteLine($"Created album: {albumUrl}");
    }

    [Test]
    [Order(5)]
    public async Task ModifyAlbumAsync_AddAndRemoveFiles_Succeeds()
    {
        // Arrange - Create an album with one file
        var testFilePath = IntegrationTestConfig.GetTestFilePath();
        var uploadRequest = new FileUploadRequest
        {
            Files = [new FileInfo(testFilePath)],
            UserHash = IntegrationTestConfig.UserHash
        };

        string? uploadedUrl = null;
        await foreach (var url in _client!.UploadFilesAsync(uploadRequest))
        {
            uploadedUrl = url;
            break;
        }
        TrackUploadedFile(uploadedUrl);
        var fileName = new Uri(uploadedUrl!).Segments.Last();

        var createAlbumRequest = new RemoteCreateAlbumRequest
        {
            Title = "CatBox.NET Modify Test Album",
            Description = "Testing album modification",
            UserHash = IntegrationTestConfig.UserHash,
            Files = [fileName]
        };

        var albumUrl = await _client.CreateAlbumAsync(createAlbumRequest);
        TrackCreatedAlbum(albumUrl);
        var albumId = new Uri(albumUrl!).Segments.Last();

        // Upload another file to add to the album
        string? secondUploadUrl = null;
        await foreach (var url in _client.UploadFilesAsync(uploadRequest))
        {
            secondUploadUrl = url;
            break;
        }
        TrackUploadedFile(secondUploadUrl);
        var secondFileName = new Uri(secondUploadUrl!).Segments.Last();

        // Act - Add file to album
        var addRequest = new ModifyAlbumImagesRequest
        {
            Request = RequestType.AddToAlbum,
            UserHash = IntegrationTestConfig.UserHash!,
            AlbumId = albumId,
            Files = [secondFileName]
        };

        var addResult = await _client.ModifyAlbumAsync(addRequest);
        TestContext.WriteLine($"Add to album result: {addResult}");

        // Act - Remove file from album
        var removeRequest = new ModifyAlbumImagesRequest
        {
            Request = RequestType.RemoveFromAlbum,
            UserHash = IntegrationTestConfig.UserHash!,
            AlbumId = albumId,
            Files = [secondFileName]
        };

        var removeResult = await _client.ModifyAlbumAsync(removeRequest);
        TestContext.WriteLine($"Remove from album result: {removeResult}");

        // Assert
        addResult.ShouldNotBeNullOrWhiteSpace();
        removeResult.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    [Order(6)]
    public async Task DeleteMultipleFilesAsync_WithUploadedFiles_Succeeds()
    {
        // Arrange - Upload a file specifically for deletion test
        var testFilePath = IntegrationTestConfig.GetTestFilePath();
        var uploadRequest = new FileUploadRequest
        {
            Files = [new FileInfo(testFilePath)],
            UserHash = IntegrationTestConfig.UserHash
        };

        string? uploadedUrl = null;
        await foreach (var url in _client!.UploadFilesAsync(uploadRequest))
        {
            uploadedUrl = url;
            break;
        }
        uploadedUrl.ShouldNotBeNullOrWhiteSpace();
        var fileName = new Uri(uploadedUrl!).Segments.Last();

        var deleteRequest = new DeleteFileRequest
        {
            UserHash = IntegrationTestConfig.UserHash!,
            FileNames = [fileName]
        };

        // Act
        var result = await _client.DeleteMultipleFilesAsync(deleteRequest);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        TestContext.WriteLine($"Delete result: {result}");
    }
}
