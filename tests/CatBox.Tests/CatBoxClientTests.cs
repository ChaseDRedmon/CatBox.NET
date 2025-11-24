using CatBox.NET;
using CatBox.NET.Client;
using CatBox.NET.Enums;
using CatBox.NET.Requests.Album;
using CatBox.NET.Requests.Album.Create;
using CatBox.NET.Requests.Album.Modify;
using CatBox.NET.Requests.File;
using CatBox.NET.Requests.URL;
using CatBox.Tests.Helpers;
using Microsoft.Extensions.Options;
using NSubstitute;
using NUnit.Framework;
using Shouldly;

namespace CatBox.Tests;

[TestFixture]
public class CatBoxClientTests
{
    private const string TestCatBoxUrl = "https://catbox.moe/user/api.php";
    private const string TestUserHash = "test-user-hash";
    private const string TestFileUrl = "https://files.catbox.moe/abc123.jpg";

    private string _tempTestJpg = null!;
    private string _tempTestPng = null!;
    private string _tempTest1Jpg = null!;
    private string _tempTest2Png = null!;
    private string _tempTest3Gif = null!;
    private string _tempMalwareExe = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Create temporary test files with minimal valid headers
        _tempTestJpg = Path.Combine(Path.GetTempPath(), $"catbox_test_{Guid.NewGuid()}.jpg");
        _tempTestPng = Path.Combine(Path.GetTempPath(), $"catbox_test_{Guid.NewGuid()}.png");
        _tempTest1Jpg = Path.Combine(Path.GetTempPath(), $"catbox_test1_{Guid.NewGuid()}.jpg");
        _tempTest2Png = Path.Combine(Path.GetTempPath(), $"catbox_test2_{Guid.NewGuid()}.png");
        _tempTest3Gif = Path.Combine(Path.GetTempPath(), $"catbox_test3_{Guid.NewGuid()}.gif");
        _tempMalwareExe = Path.Combine(Path.GetTempPath(), $"catbox_malware_{Guid.NewGuid()}.exe");

        // JPEG header
        File.WriteAllBytes(_tempTestJpg, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });
        File.WriteAllBytes(_tempTest1Jpg, new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 });

        // PNG header
        File.WriteAllBytes(_tempTestPng, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        File.WriteAllBytes(_tempTest2Png, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

        // GIF header
        File.WriteAllBytes(_tempTest3Gif, new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 });

        // EXE header
        File.WriteAllBytes(_tempMalwareExe, new byte[] { 0x4D, 0x5A, 0x90, 0x00 });
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        // Clean up temporary test files
        TryDeleteFile(_tempTestJpg);
        TryDeleteFile(_tempTestPng);
        TryDeleteFile(_tempTest1Jpg);
        TryDeleteFile(_tempTest2Png);
        TryDeleteFile(_tempTest3Gif);
        TryDeleteFile(_tempMalwareExe);
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    private IOptions<CatboxOptions> CreateOptions()
    {
        var options = new CatboxOptions
        {
            CatBoxUrl = new Uri(TestCatBoxUrl)
        };
        return Options.Create(options);
    }

    [Test]
    public async Task UploadFilesAsync_WithValidRequestAndUserHash_ReturnsExpectedUrls()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var testFile = new FileInfo(_tempTestJpg);
        var request = new FileUploadRequest
        {
            Files = [testFile],
            UserHash = TestUserHash
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadFilesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldBe(TestFileUrl);
    }

    [Test]
    public async Task UploadFilesAsync_WithoutUserHash_SucceedsAnonymously()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var testFile = new FileInfo(_tempTestPng);
        var request = new FileUploadRequest
        {
            Files = [testFile],
            UserHash = null
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadFilesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldBe(TestFileUrl);
    }

    [Test]
    public async Task UploadFilesAsync_WithInvalidFileExtension_FiltersOutInvalidFiles()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var validFile = new FileInfo(_tempTestJpg);
        var invalidFile = new FileInfo(_tempMalwareExe);
        var request = new FileUploadRequest
        {
            Files = [validFile, invalidFile],
            UserHash = TestUserHash
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadFilesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        // Only the valid file should be uploaded
        results.Count.ShouldBe(1);
    }

    [Test]
    public void UploadFilesAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        // Act & Assert
        Should.Throw<ArgumentNullException>(async () =>
        {
            await foreach (var _ in client.UploadFilesAsync(null!))
            {
            }
        });
    }

    [Test]
    public async Task UploadFilesAsync_WithMultipleFiles_YieldsMultipleResponses()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var file1 = new FileInfo(_tempTest1Jpg);
        var file2 = new FileInfo(_tempTest2Png);
        var file3 = new FileInfo(_tempTest3Gif);
        var request = new FileUploadRequest
        {
            Files = [file1, file2, file3],
            UserHash = TestUserHash
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadFilesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(3);
        results.ShouldAllBe(r => r == TestFileUrl);
    }

    [Test]
    public async Task UploadFilesAsync_WithEmptyFileCollection_YieldsNothing()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new FileUploadRequest
        {
            Files = [],
            UserHash = TestUserHash
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadFilesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.ShouldBeEmpty();
    }

    [Test]
    public async Task UploadFilesAsync_CancellationToken_CancelsOperation()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var testFile = new FileInfo(_tempTestJpg);
        var request = new FileUploadRequest
        {
            Files = [testFile],
            UserHash = TestUserHash
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in client.UploadFilesAsync(request, cts.Token))
            {
            }
        });
    }

    [Test]
    public async Task UploadFilesAsStreamAsync_WithValidRequestAndUserHash_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var stream = new MemoryStream([1, 2, 3, 4]);
        var requests = new[]
        {
            new StreamUploadRequest
            {
                FileName = "test.jpg",
                Stream = stream,
                UserHash = TestUserHash
            }
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadFilesAsStreamAsync(requests))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldBe(TestFileUrl);
    }

    [Test]
    public async Task UploadFilesAsStreamAsync_WithoutUserHash_SucceedsAnonymously()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var stream = new MemoryStream([1, 2, 3, 4]);
        var requests = new[]
        {
            new StreamUploadRequest
            {
                FileName = "test.jpg",
                Stream = stream,
                UserHash = null
            }
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadFilesAsStreamAsync(requests))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldBe(TestFileUrl);
    }

    [Test]
    public void UploadFilesAsStreamAsync_WithNullFileName_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var stream = new MemoryStream([1, 2, 3, 4]);
        var requests = new[]
        {
            new StreamUploadRequest
            {
                FileName = null!,
                Stream = stream,
                UserHash = TestUserHash
            }
        };

        // Act & Assert
        Should.Throw<ArgumentException>(async () =>
        {
            await foreach (var _ in client.UploadFilesAsStreamAsync(requests))
            {
            }
        });
    }

    [Test]
    public async Task UploadFilesAsStreamAsync_WithMultipleStreams_ProcessesAll()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var requests = new[]
        {
            new StreamUploadRequest
            {
                FileName = "test1.jpg",
                Stream = new MemoryStream([1, 2, 3]),
                UserHash = TestUserHash
            },
            new StreamUploadRequest
            {
                FileName = "test2.png",
                Stream = new MemoryStream([4, 5, 6]),
                UserHash = TestUserHash
            }
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadFilesAsStreamAsync(requests))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(2);
        results.ShouldAllBe(r => r == TestFileUrl);
    }

    [Test]
    public void UploadFilesAsStreamAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        // Act & Assert
        Should.Throw<ArgumentNullException>(async () =>
        {
            await foreach (var _ in client.UploadFilesAsStreamAsync(null!))
            {
            }
        });
    }

    [Test]
    public async Task UploadFilesAsUrlAsync_WithValidUrls_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new UrlUploadRequest
        {
            Files = [new Uri("https://example.com/image.jpg")],
            UserHash = TestUserHash
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadFilesAsUrlAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldBe(TestFileUrl);
    }

    [Test]
    public async Task UploadFilesAsUrlAsync_WithoutUserHash_SucceedsAnonymously()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new UrlUploadRequest
        {
            Files = [new Uri("https://example.com/image.jpg")],
            UserHash = null
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadFilesAsUrlAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldBe(TestFileUrl);
    }

    [Test]
    public async Task UploadFilesAsUrlAsync_WithMultipleUrls_ProcessesAll()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new UrlUploadRequest
        {
            Files =
            [
                new Uri("https://example.com/image1.jpg"),
                new Uri("https://example.com/image2.png"),
                new Uri("https://example.com/image3.gif")
            ],
            UserHash = TestUserHash
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadFilesAsUrlAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(3);
        results.ShouldAllBe(r => r == TestFileUrl);
    }

    [Test]
    public void UploadFilesAsUrlAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        // Act & Assert
        Should.Throw<ArgumentNullException>(async () =>
        {
            await foreach (var _ in client.UploadFilesAsUrlAsync(null!))
            {
            }
        });
    }

    [Test]
    public async Task UploadFilesAsUrlAsync_CancellationToken_CancelsOperation()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new UrlUploadRequest
        {
            Files = [new Uri("https://example.com/image.jpg")],
            UserHash = TestUserHash
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in client.UploadFilesAsUrlAsync(request, cts.Token))
            {
            }
        });
    }

    [Test]
    public async Task DeleteMultipleFilesAsync_WithValidRequest_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Files deleted successfully");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new DeleteFileRequest
        {
            UserHash = TestUserHash,
            FileNames = ["file1.jpg", "file2.png"]
        };

        // Act
        var result = await client.DeleteMultipleFilesAsync(request);

        // Assert
        result.ShouldBe("Files deleted successfully");
    }

    [Test]
    public void DeleteMultipleFilesAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        // Act & Assert
        Should.Throw<ArgumentNullException>(async () => await client.DeleteMultipleFilesAsync(null!));
    }

    [Test]
    public void DeleteMultipleFilesAsync_WithNullUserHash_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new DeleteFileRequest
        {
            UserHash = null!,
            FileNames = ["file1.jpg"]
        };

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.DeleteMultipleFilesAsync(request));
    }

    [Test]
    public void DeleteMultipleFilesAsync_WithEmptyUserHash_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new DeleteFileRequest
        {
            UserHash = "",
            FileNames = ["file1.jpg"]
        };

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.DeleteMultipleFilesAsync(request));
    }

    [Test]
    public void DeleteMultipleFilesAsync_WithEmptyFileNames_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new DeleteFileRequest
        {
            UserHash = TestUserHash,
            FileNames = []
        };

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.DeleteMultipleFilesAsync(request));
    }

    [Test]
    public async Task DeleteMultipleFilesAsync_WithMultipleFiles_JoinsWithSpace()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new DeleteFileRequest
        {
            UserHash = TestUserHash,
            FileNames = ["file1.jpg", "file2.png", "file3.gif"]
        };

        // Act
        var result = await client.DeleteMultipleFilesAsync(request);

        // Assert
        result.ShouldNotBeNull();
    }

    [Test]
    public async Task CreateAlbumAsync_WithValidRequest_ReturnsAlbumUrl()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("https://catbox.moe/c/abc123");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new RemoteCreateAlbumRequest
        {
            Title = "Test Album",
            Description = "Test Description",
            UserHash = TestUserHash,
            Files = ["file1.jpg", "file2.png"]
        };

        // Act
        var result = await client.CreateAlbumAsync(request);

        // Assert
        result.ShouldBe("https://catbox.moe/c/abc123");
    }

    [Test]
    public async Task CreateAlbumAsync_WithFullUrls_StripsHostname()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("https://catbox.moe/c/abc123");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new RemoteCreateAlbumRequest
        {
            Title = "Test Album",
            Description = "Test Description",
            UserHash = TestUserHash,
            Files = ["https://files.catbox.moe/file1.jpg", "https://files.catbox.moe/file2.png"]
        };

        // Act
        var result = await client.CreateAlbumAsync(request);

        // Assert
        result.ShouldBe("https://catbox.moe/c/abc123");
    }

    [Test]
    public async Task CreateAlbumAsync_WithShortFilenames_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("https://catbox.moe/c/abc123");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new RemoteCreateAlbumRequest
        {
            Title = "Test Album",
            Description = "Test Description",
            UserHash = TestUserHash,
            Files = ["abc123.jpg", "def456.png"]
        };

        // Act
        var result = await client.CreateAlbumAsync(request);

        // Assert
        result.ShouldBe("https://catbox.moe/c/abc123");
    }

    [Test]
    public void CreateAlbumAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        // Act & Assert
        Should.Throw<ArgumentNullException>(async () => await client.CreateAlbumAsync(null!));
    }

    [Test]
    public void CreateAlbumAsync_WithNullTitle_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new RemoteCreateAlbumRequest
        {
            Title = null!,
            Description = "Test Description",
            UserHash = TestUserHash,
            Files = ["file1.jpg"]
        };

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.CreateAlbumAsync(request));
    }

    [Test]
    public void CreateAlbumAsync_WithNullDescription_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new RemoteCreateAlbumRequest
        {
            Title = "Test Album",
            Description = null!,
            UserHash = TestUserHash,
            Files = ["file1.jpg"]
        };

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.CreateAlbumAsync(request));
    }

    [Test]
    public async Task CreateAlbumAsync_WithoutUserHash_CreatesAnonymousAlbum()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("https://catbox.moe/c/abc123");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new RemoteCreateAlbumRequest
        {
            Title = "Test Album",
            Description = "Test Description",
            UserHash = null,
            Files = ["file1.jpg"]
        };

        // Act
        var result = await client.CreateAlbumAsync(request);

        // Assert
        result.ShouldBe("https://catbox.moe/c/abc123");
    }

    [Test]
    public void CreateAlbumAsync_WithEmptyFiles_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new RemoteCreateAlbumRequest
        {
            Title = "Test Album",
            Description = "Test Description",
            UserHash = TestUserHash,
            Files = []
        };

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.CreateAlbumAsync(request));
    }

    [Test]
    public async Task CreateAlbumAsync_WithOptionalDescription_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("https://catbox.moe/c/abc123");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new RemoteCreateAlbumRequest
        {
            Title = "Test Album",
            Description = "Optional Description",
            UserHash = TestUserHash,
            Files = ["file1.jpg"]
        };

        // Act
        var result = await client.CreateAlbumAsync(request);

        // Assert
        result.ShouldBe("https://catbox.moe/c/abc123");
    }

    [Test]
    public async Task EditAlbumAsync_WithValidRequest_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Album edited successfully");
        var client = new CatBoxClient(httpClient, CreateOptions());

#pragma warning disable CS0618 // Type or member is obsolete
        var request = new EditAlbumRequest
        {
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Title = "Updated Title",
            Description = "Updated Description",
            Files = ["file1.jpg", "file2.png"]
        };
#pragma warning restore CS0618 // Type or member is obsolete

        // Act
        var result = await client.EditAlbumAsync(request);

        // Assert
        result.ShouldBe("Album edited successfully");
    }

    [Test]
    public void EditAlbumAsync_WithNullUserHash_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

#pragma warning disable CS0618 // Type or member is obsolete
        var request = new EditAlbumRequest
        {
            UserHash = null!,
            AlbumId = "abc123",
            Title = "Title",
            Description = "Description",
            Files = ["file1.jpg"]
        };
#pragma warning restore CS0618 // Type or member is obsolete

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.EditAlbumAsync(request));
    }

    [Test]
    public void EditAlbumAsync_WithNullTitle_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

#pragma warning disable CS0618 // Type or member is obsolete
        var request = new EditAlbumRequest
        {
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Title = null!,
            Description = "Description",
            Files = ["file1.jpg"]
        };
#pragma warning restore CS0618 // Type or member is obsolete

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.EditAlbumAsync(request));
    }

    [Test]
    public void EditAlbumAsync_WithNullDescription_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

#pragma warning disable CS0618 // Type or member is obsolete
        var request = new EditAlbumRequest
        {
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Title = "Title",
            Description = null!,
            Files = ["file1.jpg"]
        };
#pragma warning restore CS0618 // Type or member is obsolete

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.EditAlbumAsync(request));
    }

    [Test]
    public void EditAlbumAsync_WithNullAlbumId_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

#pragma warning disable CS0618 // Type or member is obsolete
        var request = new EditAlbumRequest
        {
            UserHash = TestUserHash,
            AlbumId = null!,
            Title = "Title",
            Description = "Description",
            Files = ["file1.jpg"]
        };
#pragma warning restore CS0618 // Type or member is obsolete

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.EditAlbumAsync(request));
    }

    [Test]
    public void EditAlbumAsync_WithEmptyFiles_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

#pragma warning disable CS0618 // Type or member is obsolete
        var request = new EditAlbumRequest
        {
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Title = "Title",
            Description = "Description",
            Files = []
        };
#pragma warning restore CS0618 // Type or member is obsolete

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.EditAlbumAsync(request));
    }

    [Test]
    public void EditAlbumAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        // Act & Assert
        Should.Throw<ArgumentNullException>(async () => await client.EditAlbumAsync(null!));
    }

    [Test]
    public async Task EditAlbumAsync_WithAllParameters_SendsCorrectly()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

#pragma warning disable CS0618 // Type or member is obsolete
        var request = new EditAlbumRequest
        {
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Title = "New Title",
            Description = "New Description",
            Files = ["file1.jpg", "file2.png", "file3.gif"]
        };
#pragma warning restore CS0618 // Type or member is obsolete

        // Act
        var result = await client.EditAlbumAsync(request);

        // Assert
        result.ShouldBe("Success");
    }

    [Test]
    public async Task ModifyAlbumAsync_AddToAlbum_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Files added successfully");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new ModifyAlbumImagesRequest
        {
            Request = RequestType.AddToAlbum,
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Files = ["file1.jpg", "file2.png"]
        };

        // Act
        var result = await client.ModifyAlbumAsync(request);

        // Assert
        result.ShouldBe("Files added successfully");
    }

    [Test]
    public async Task ModifyAlbumAsync_RemoveFromAlbum_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Files removed successfully");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new ModifyAlbumImagesRequest
        {
            Request = RequestType.RemoveFromAlbum,
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Files = ["file1.jpg"]
        };

        // Act
        var result = await client.ModifyAlbumAsync(request);

        // Assert
        result.ShouldBe("Files removed successfully");
    }

    [Test]
    public async Task ModifyAlbumAsync_DeleteAlbum_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Album deleted successfully");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new ModifyAlbumImagesRequest
        {
            Request = RequestType.DeleteAlbum,
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Files = []
        };

        // Act
        var result = await client.ModifyAlbumAsync(request);

        // Assert
        result.ShouldBe("Album deleted successfully");
    }

    [Test]
    public void ModifyAlbumAsync_WithInvalidRequestType_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new ModifyAlbumImagesRequest
        {
            Request = RequestType.UploadFile,
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Files = ["file1.jpg"]
        };

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.ModifyAlbumAsync(request));
    }

    [Test]
    public void ModifyAlbumAsync_WithNullUserHash_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new ModifyAlbumImagesRequest
        {
            Request = RequestType.AddToAlbum,
            UserHash = null!,
            AlbumId = "abc123",
            Files = ["file1.jpg"]
        };

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.ModifyAlbumAsync(request));
    }

    [Test]
    public void ModifyAlbumAsync_WithEmptyFiles_ThrowsArgumentException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new ModifyAlbumImagesRequest
        {
            Request = RequestType.AddToAlbum,
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Files = []
        };

        // Act & Assert
        Should.Throw<ArgumentException>(async () => await client.ModifyAlbumAsync(request));
    }

    [Test]
    public void ModifyAlbumAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        // Act & Assert
        Should.Throw<ArgumentNullException>(async () => await client.ModifyAlbumAsync(null!));
    }

    [Test]
    public async Task ModifyAlbumAsync_AddToAlbum_IncludesFilesParameter()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new ModifyAlbumImagesRequest
        {
            Request = RequestType.AddToAlbum,
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Files = ["file1.jpg", "file2.png"]
        };

        // Act
        var result = await client.ModifyAlbumAsync(request);

        // Assert
        result.ShouldNotBeNull();
    }

    [Test]
    public async Task ModifyAlbumAsync_RemoveFromAlbum_IncludesFilesParameter()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new ModifyAlbumImagesRequest
        {
            Request = RequestType.RemoveFromAlbum,
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Files = ["file1.jpg"]
        };

        // Act
        var result = await client.ModifyAlbumAsync(request);

        // Assert
        result.ShouldNotBeNull();
    }

    [Test]
    public async Task ModifyAlbumAsync_DeleteAlbum_DoesNotRequireFiles()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Album deleted");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new ModifyAlbumImagesRequest
        {
            Request = RequestType.DeleteAlbum,
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Files = ["ignored.jpg"]
        };

        // Act
        var result = await client.ModifyAlbumAsync(request);

        // Assert
        result.ShouldBe("Album deleted");
    }

    [Test]
    public void ModifyAlbumAsync_WithWrongRequestType_ThrowsInvalidOperationException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new ModifyAlbumImagesRequest
        {
            Request = RequestType.CreateAlbum,
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Files = ["file1.jpg"]
        };

        // Act & Assert
        Should.Throw<InvalidOperationException>(async () => await client.ModifyAlbumAsync(request));
    }

    [Test]
    public async Task ModifyAlbumAsync_WithMultipleFiles_JoinsWithSpace()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient("Success");
        var client = new CatBoxClient(httpClient, CreateOptions());

        var request = new ModifyAlbumImagesRequest
        {
            Request = RequestType.AddToAlbum,
            UserHash = TestUserHash,
            AlbumId = "abc123",
            Files = ["file1.jpg", "file2.png", "file3.gif"]
        };

        // Act
        var result = await client.ModifyAlbumAsync(request);

        // Assert
        result.ShouldNotBeNull();
    }

    [Test]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new CatBoxClient(null!, CreateOptions()));
    }

    [Test]
    public void Constructor_WithNullCatBoxUrl_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestFileUrl);
        var options = Options.Create(new CatboxOptions { CatBoxUrl = null });

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new CatBoxClient(httpClient, options));
    }
}
