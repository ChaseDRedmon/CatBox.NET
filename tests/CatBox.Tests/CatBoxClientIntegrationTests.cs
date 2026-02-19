using CatBox.NET;
using CatBox.NET.Client;
using CatBox.NET.Enums;
using CatBox.NET.Requests.Album;
using CatBox.NET.Requests.Album.Create;
using CatBox.NET.Requests.Album.Modify;
using CatBox.NET.Requests.File;
using CatBox.NET.Requests.URL;
using CatBox.NET.Responses.Album;
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
                await TestContext.Out.WriteLineAsync($"Cleaning up {_createdAlbums.Count} album(s)...");
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
                        await TestContext.Out.WriteLineAsync($"Deleted album: {albumId}");
                    }
                    catch (Exception ex)
                    {
                        await TestContext.Out.WriteLineAsync($"Failed to delete album {albumId}: {ex.Message}");
                    }
                }
            }

            // Then delete individual files
            if (_uploadedFiles.Count > 0)
            {
                await TestContext.Out.WriteLineAsync($"Cleaning up {_uploadedFiles.Count} file(s)...");
                var deleteRequest = new DeleteFileRequest
                {
                    UserHash = IntegrationTestConfig.UserHash!,
                    FileNames = _uploadedFiles.ToList()
                };

                var result = await _client.DeleteMultipleFilesAsync(deleteRequest);
                await TestContext.Out.WriteLineAsync($"Delete result: {result}");
            }
        }
        catch (Exception ex)
        {
            await TestContext.Out.WriteLineAsync($"Cleanup error: {ex.Message}");
        }
    }

    private void TrackUploadedFile(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        var fileName = url.ToCatboxImageName()!;
        using (_lock.EnterScope())
        {
            if (!_uploadedFiles.Contains(fileName))
            {
                _uploadedFiles.Add(fileName);
                TestContext.Out.WriteLine($"Tracked file for cleanup: {fileName}");
            }
        }
    }

    private void TrackCreatedAlbum(string? albumUrl)
    {
        if (string.IsNullOrWhiteSpace(albumUrl))
            return;

        var albumId = albumUrl.ToAlbumShortCode()!;
        using (_lock.EnterScope())
        {
            if (!_createdAlbums.Contains(albumId))
            {
                _createdAlbums.Add(albumId);
                TestContext.Out.WriteLine($"Tracked album for cleanup: {albumId}");
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
        await TestContext.Out.WriteLineAsync($"Uploaded file URL: {results[0]}");
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
        await TestContext.Out.WriteLineAsync($"Uploaded stream URL: {results[0]}");
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
        await TestContext.Out.WriteLineAsync($"Uploaded from URL: {results[0]}");
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
            .Select(url => url.ToCatboxImageName()!)
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
        await TestContext.Out.WriteLineAsync($"Created album: {albumUrl}");
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
        var fileName = uploadedUrl!.ToCatboxImageName()!;

        var createAlbumRequest = new RemoteCreateAlbumRequest
        {
            Title = "CatBox.NET Modify Test Album",
            Description = "Testing album modification",
            UserHash = IntegrationTestConfig.UserHash,
            Files = [fileName]
        };

        var albumUrl = await _client.CreateAlbumAsync(createAlbumRequest);
        TrackCreatedAlbum(albumUrl);
        var albumId = albumUrl!.ToAlbumShortCode()!;

        // Upload another file to add to the album
        string? secondUploadUrl = null;
        await foreach (var url in _client.UploadFilesAsync(uploadRequest))
        {
            secondUploadUrl = url;
            break;
        }
        TrackUploadedFile(secondUploadUrl);
        var secondFileName = secondUploadUrl!.ToCatboxImageName()!;

        // Act - Add file to album
        var addRequest = new ModifyAlbumImagesRequest
        {
            Request = RequestType.AddToAlbum,
            UserHash = IntegrationTestConfig.UserHash!,
            AlbumId = albumId,
            Files = [secondFileName]
        };

        var addResult = await _client.ModifyAlbumAsync(addRequest);
        await TestContext.Out.WriteLineAsync($"Add to album result: {addResult}");

        // Act - Remove file from album
        var removeRequest = new ModifyAlbumImagesRequest
        {
            Request = RequestType.RemoveFromAlbum,
            UserHash = IntegrationTestConfig.UserHash!,
            AlbumId = albumId,
            Files = [secondFileName]
        };

        var removeResult = await _client.ModifyAlbumAsync(removeRequest);
        await TestContext.Out.WriteLineAsync($"Remove from album result: {removeResult}");

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
        var fileName = uploadedUrl!.ToCatboxImageName()!;

        var deleteRequest = new DeleteFileRequest
        {
            UserHash = IntegrationTestConfig.UserHash!,
            FileNames = [fileName]
        };

        // Act
        var result = await _client.DeleteMultipleFilesAsync(deleteRequest);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        await TestContext.Out.WriteLineAsync($"Delete result: {result}");
    }

    [Test]
    [Order(7)]
    public async Task GetAlbumAsync_WithCreatedAlbum_ReturnsAlbumInfo()
    {
        // Arrange - Upload a file and create an album
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
        var fileName = uploadedUrl!.ToCatboxImageName()!;

        var createAlbumRequest = new RemoteCreateAlbumRequest
        {
            Title = "GetAlbum Integration Test",
            Description = "Testing GetAlbumAsync",
            UserHash = IntegrationTestConfig.UserHash,
            Files = [fileName]
        };

        var albumUrl = await _client.CreateAlbumAsync(createAlbumRequest);
        TrackCreatedAlbum(albumUrl);
        var albumId = albumUrl!.ToAlbumShortCode()!;

        // Act
        var albumInfo = await _client.GetAlbumAsync(new GetAlbumRequest { AlbumId = albumId });

        // Assert
        albumInfo.ShouldNotBeNull();
        albumInfo.Title.ShouldBe("GetAlbum Integration Test");
        albumInfo.Description.ShouldBe("Testing GetAlbumAsync");
        albumInfo.AlbumId.ShouldBe(albumId);
        albumInfo.Files.ShouldContain(fileName);
        await TestContext.Out.WriteLineAsync($"Album info: {albumInfo.Title} ({albumInfo.AlbumId}), {albumInfo.Files.Length} file(s)");
    }

    [Test]
    [Order(8)]
    public async Task DownloadFileAsync_WithUploadedFile_DownloadsSuccessfully()
    {
        // Arrange - Upload a file
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
        var fileName = uploadedUrl!.ToCatboxImageName()!;

        var tempDir = Path.Combine(Path.GetTempPath(), $"catbox-test-{Guid.NewGuid():N}");
        try
        {
            // Act
            await _client.DownloadFileAsync(fileName, tempDir);

            // Assert
            var downloadedFile = Path.Combine(tempDir, fileName);
            File.Exists(downloadedFile).ShouldBeTrue($"Downloaded file not found: {downloadedFile}");
            new FileInfo(downloadedFile).Length.ShouldBeGreaterThan(0);
            await TestContext.Out.WriteLineAsync($"Downloaded file: {downloadedFile} ({new FileInfo(downloadedFile).Length} bytes)");
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Test]
    [Order(9)]
    public async Task EditAlbumAsync_WithCreatedAlbum_EditsSuccessfully()
    {
        // Arrange - Upload 2 files and create an album
        var testFilePath = IntegrationTestConfig.GetTestFilePath();
        var uploadRequest = new FileUploadRequest
        {
            Files = [new FileInfo(testFilePath), new FileInfo(testFilePath)],
            UserHash = IntegrationTestConfig.UserHash
        };

        var uploadedFileNames = new List<string>();
        await foreach (var url in _client!.UploadFilesAsync(uploadRequest))
        {
            TrackUploadedFile(url);
            uploadedFileNames.Add(url!.ToCatboxImageName()!);
        }
        uploadedFileNames.Count.ShouldBe(2);

        var createAlbumRequest = new RemoteCreateAlbumRequest
        {
            Title = "EditAlbum Integration Test",
            Description = "Before edit",
            UserHash = IntegrationTestConfig.UserHash,
            Files = uploadedFileNames
        };

        var albumUrl = await _client.CreateAlbumAsync(createAlbumRequest);
        TrackCreatedAlbum(albumUrl);
        var albumId = albumUrl!.ToAlbumShortCode()!;

        // Act - Edit the album with new title, description, and only 1 file
#pragma warning disable CS0618 // EditAlbumAsync is marked Obsolete as a safety warning
        var editResult = await _client.EditAlbumAsync(new EditAlbumRequest
        {
            UserHash = IntegrationTestConfig.UserHash!,
            AlbumId = albumId,
            Title = "Edited Title",
            Description = "Edited description",
            Files = [uploadedFileNames[0]]
        });
#pragma warning restore CS0618

        // Assert
        editResult.ShouldNotBeNullOrWhiteSpace();
        await TestContext.Out.WriteLineAsync($"Edit result: {editResult}");

        // Verify changes via GetAlbum
        var albumInfo = await _client.GetAlbumAsync(new GetAlbumRequest { AlbumId = albumId });
        albumInfo.Title.ShouldBe("Edited Title");
        albumInfo.Description.ShouldBe("Edited description");
        albumInfo.Files.Length.ShouldBe(1);
        albumInfo.Files.ShouldContain(uploadedFileNames[0]);
        await TestContext.Out.WriteLineAsync($"Verified album after edit: {albumInfo.Title}, {albumInfo.Files.Length} file(s)");
    }
}
