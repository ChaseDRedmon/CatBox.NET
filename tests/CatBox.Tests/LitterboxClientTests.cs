using CatBox.NET;
using CatBox.NET.Client;
using CatBox.NET.Enums;
using CatBox.NET.Requests.Litterbox;
using CatBox.Tests.Helpers;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;

namespace CatBox.Tests;

[TestFixture]
public class LitterboxClientTests
{
    private const string TestLitterboxUrl = "https://litterbox.catbox.moe/resources/internals/api.php";
    private const string TestTempFileUrl = "https://litter.catbox.moe/abc123.jpg";

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
        _tempTestJpg = Path.Combine(Path.GetTempPath(), $"litterbox_test_{Guid.NewGuid()}.jpg");
        _tempTestPng = Path.Combine(Path.GetTempPath(), $"litterbox_test_{Guid.NewGuid()}.png");
        _tempTest1Jpg = Path.Combine(Path.GetTempPath(), $"litterbox_test1_{Guid.NewGuid()}.jpg");
        _tempTest2Png = Path.Combine(Path.GetTempPath(), $"litterbox_test2_{Guid.NewGuid()}.png");
        _tempTest3Gif = Path.Combine(Path.GetTempPath(), $"litterbox_test3_{Guid.NewGuid()}.gif");
        _tempMalwareExe = Path.Combine(Path.GetTempPath(), $"litterbox_malware_{Guid.NewGuid()}.exe");

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
            LitterboxUrl = new Uri(TestLitterboxUrl)
        };
        return Options.Create(options);
    }

    [Test]
    public async Task UploadMultipleImagesAsync_WithOneHourExpiry_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        var testFile = new FileInfo(_tempTestJpg);
        var request = new TemporaryFileUploadRequest
        {
            Files = [testFile],
            Expiry = ExpireAfter.OneHour
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadMultipleImagesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldBe(TestTempFileUrl);
    }

    [Test]
    public async Task UploadMultipleImagesAsync_WithTwelveHourExpiry_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        var testFile = new FileInfo(_tempTestJpg);
        var request = new TemporaryFileUploadRequest
        {
            Files = [testFile],
            Expiry = ExpireAfter.TwelveHours
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadMultipleImagesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldBe(TestTempFileUrl);
    }

    [Test]
    public async Task UploadMultipleImagesAsync_WithOneDayExpiry_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        var testFile = new FileInfo(_tempTestJpg);
        var request = new TemporaryFileUploadRequest
        {
            Files = [testFile],
            Expiry = ExpireAfter.OneDay
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadMultipleImagesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldBe(TestTempFileUrl);
    }

    [Test]
    public async Task UploadMultipleImagesAsync_WithThreeDaysExpiry_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        var testFile = new FileInfo(_tempTestJpg);
        var request = new TemporaryFileUploadRequest
        {
            Files = [testFile],
            Expiry = ExpireAfter.ThreeDays
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadMultipleImagesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldBe(TestTempFileUrl);
    }

    [Test]
    public async Task UploadMultipleImagesAsync_WithInvalidFileExtension_FiltersOutInvalidFiles()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        var validFile = new FileInfo(_tempTestJpg);
        var invalidFile = new FileInfo(_tempMalwareExe);
        var request = new TemporaryFileUploadRequest
        {
            Files = [validFile, invalidFile],
            Expiry = ExpireAfter.OneHour
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadMultipleImagesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        // Only the valid file should be uploaded
        results.Count.ShouldBe(1);
    }

    [Test]
    public void UploadMultipleImagesAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        // Act & Assert
        Should.Throw<ArgumentNullException>(async () =>
        {
            await foreach (var _ in client.UploadMultipleImagesAsync(null!))
            {
            }
        });
    }

    [Test]
    public async Task UploadMultipleImagesAsync_WithMultipleFiles_YieldsMultipleResponses()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        var file1 = new FileInfo(_tempTest1Jpg);
        var file2 = new FileInfo(_tempTest2Png);
        var file3 = new FileInfo(_tempTest3Gif);
        var request = new TemporaryFileUploadRequest
        {
            Files = [file1, file2, file3],
            Expiry = ExpireAfter.OneDay
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in client.UploadMultipleImagesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(3);
        results.ShouldAllBe(r => r == TestTempFileUrl);
    }

    [Test]
    public async Task UploadMultipleImagesAsync_CancellationToken_CancelsOperation()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        var testFile = new FileInfo(_tempTestJpg);
        var request = new TemporaryFileUploadRequest
        {
            Files = [testFile],
            Expiry = ExpireAfter.OneHour
        };

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await foreach (var _ in client.UploadMultipleImagesAsync(request, cts.Token))
            {
            }
        });
    }

    [Test]
    public async Task UploadImageAsync_WithValidRequest_Succeeds()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        var stream = new MemoryStream([1, 2, 3, 4]);
        var request = new TemporaryStreamUploadRequest
        {
            FileName = "test.jpg",
            Stream = stream,
            Expiry = ExpireAfter.OneHour
        };

        // Act
        var result = await client.UploadImageAsync(request);

        // Assert
        result.ShouldBe(TestTempFileUrl);
    }

    [Test]
    public void UploadImageAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        // Act & Assert
        Should.Throw<ArgumentNullException>(async () => await client.UploadImageAsync(null!));
    }

    [Test]
    public void UploadImageAsync_WithNullFileName_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        var stream = new MemoryStream([1, 2, 3, 4]);
        var request = new TemporaryStreamUploadRequest
        {
            FileName = null!,
            Stream = stream,
            Expiry = ExpireAfter.OneHour
        };

        // Act & Assert
        Should.Throw<ArgumentNullException>(async () => await client.UploadImageAsync(request));
    }

    [Test]
    public async Task UploadImageAsync_WithDifferentExpiryTimes_SucceedsWith()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        var stream = new MemoryStream([1, 2, 3, 4]);
        var request = new TemporaryStreamUploadRequest
        {
            FileName = "test.jpg",
            Stream = stream,
            Expiry = ExpireAfter.ThreeDays
        };

        // Act
        var result = await client.UploadImageAsync(request);

        // Assert
        result.ShouldBe(TestTempFileUrl);
    }

    [Test]
    public async Task UploadImageAsync_CancellationToken_CancelsOperation()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var client = new LitterboxClient(httpClient, CreateOptions());

        var stream = new MemoryStream([1, 2, 3, 4]);
        var request = new TemporaryStreamUploadRequest
        {
            FileName = "test.jpg",
            Stream = stream,
            Expiry = ExpireAfter.OneHour
        };

        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () => await client.UploadImageAsync(request, cts.Token));
    }

    [Test]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new LitterboxClient(null!, CreateOptions()));
    }

    [Test]
    public void Constructor_WithNullLitterboxUrl_ThrowsArgumentNullException()
    {
        // Arrange
        var httpClient = HttpClientTestHelper.CreateMockHttpClient(TestTempFileUrl);
        var options = Options.Create(new CatboxOptions { LitterboxUrl = null });

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => new LitterboxClient(httpClient, options));
    }
}
