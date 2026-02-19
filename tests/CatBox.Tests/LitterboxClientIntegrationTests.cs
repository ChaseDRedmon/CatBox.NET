using CatBox.NET;
using CatBox.NET.Client;
using CatBox.NET.Enums;
using CatBox.NET.Requests.Litterbox;
using CatBox.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;

namespace CatBox.Tests;

/// <summary>
/// Integration tests for LitterboxClient that make real API calls
/// Note: Litterbox uploads are temporary and auto-expire, so no cleanup is needed
/// Run with: dotnet test --filter Category=Integration
/// </summary>
[TestFixture]
[Category("Integration")]
public class LitterboxClientIntegrationTests
{
    private ILitterboxClient? _client;

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
            options.LitterboxUrl = IntegrationTestConfig.LitterboxUrl;
        });

        var serviceProvider = services.BuildServiceProvider();
        _client = serviceProvider.GetRequiredService<ILitterboxClient>();
    }

    [Test]
    public async Task UploadMultipleImagesAsync_WithOneHourExpiry_Succeeds()
    {
        // Arrange
        var testFilePath = IntegrationTestConfig.GetTestFilePath();
        File.Exists(testFilePath).ShouldBeTrue($"Test file not found: {testFilePath}");

        var request = new TemporaryFileUploadRequest
        {
            Files = [new FileInfo(testFilePath)],
            Expiry = ExpireAfter.OneHour
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in _client!.UploadMultipleImagesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldNotBeNullOrWhiteSpace();
        results[0].ShouldStartWith("https://litter.catbox.moe/");
        await TestContext.Out.WriteLineAsync($"Uploaded temporary file (1h expiry): {results[0]}");
    }

    [Test]
    public async Task UploadMultipleImagesAsync_WithTwelveHourExpiry_Succeeds()
    {
        // Arrange
        var testFilePath = IntegrationTestConfig.GetTestFilePath();
        var request = new TemporaryFileUploadRequest
        {
            Files = [new FileInfo(testFilePath)],
            Expiry = ExpireAfter.TwelveHours
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in _client!.UploadMultipleImagesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldNotBeNullOrWhiteSpace();
        results[0].ShouldStartWith("https://litter.catbox.moe/");
        await TestContext.Out.WriteLineAsync($"Uploaded temporary file (12h expiry): {results[0]}");
    }

    [Test]
    public async Task UploadMultipleImagesAsync_WithOneDayExpiry_Succeeds()
    {
        // Arrange
        var testFilePath = IntegrationTestConfig.GetTestFilePath();
        var request = new TemporaryFileUploadRequest
        {
            Files = [new FileInfo(testFilePath)],
            Expiry = ExpireAfter.OneDay
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in _client!.UploadMultipleImagesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldNotBeNullOrWhiteSpace();
        results[0].ShouldStartWith("https://litter.catbox.moe/");
        await TestContext.Out.WriteLineAsync($"Uploaded temporary file (1d expiry): {results[0]}");
    }

    [Test]
    public async Task UploadMultipleImagesAsync_WithThreeDaysExpiry_Succeeds()
    {
        // Arrange
        var testFilePath = IntegrationTestConfig.GetTestFilePath();
        var request = new TemporaryFileUploadRequest
        {
            Files = [new FileInfo(testFilePath)],
            Expiry = ExpireAfter.ThreeDays
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in _client!.UploadMultipleImagesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(1);
        results[0].ShouldNotBeNullOrWhiteSpace();
        results[0].ShouldStartWith("https://litter.catbox.moe/");
        await TestContext.Out.WriteLineAsync($"Uploaded temporary file (3d expiry): {results[0]}");
    }

    [Test]
    public async Task UploadImageAsync_WithMemoryStream_Succeeds()
    {
        // Arrange
        var testFilePath = IntegrationTestConfig.GetTestFilePath();
        var fileBytes = await File.ReadAllBytesAsync(testFilePath);
        var stream = new MemoryStream(fileBytes);

        var request = new TemporaryStreamUploadRequest
        {
            FileName = "test-temp-stream.png",
            Stream = stream,
            Expiry = ExpireAfter.OneHour
        };

        // Act
        var result = await _client!.UploadImageAsync(request);

        // Assert
        result.ShouldNotBeNullOrWhiteSpace();
        result.ShouldStartWith("https://litter.catbox.moe/");
        await TestContext.Out.WriteLineAsync($"Uploaded temporary stream: {result}");
    }

    [Test]
    public async Task UploadMultipleImagesAsync_WithMultipleFiles_YieldsMultipleUrls()
    {
        // Arrange
        var testFilePath = IntegrationTestConfig.GetTestFilePath();
        var request = new TemporaryFileUploadRequest
        {
            Files = [new FileInfo(testFilePath), new FileInfo(testFilePath), new FileInfo(testFilePath)],
            Expiry = ExpireAfter.OneHour
        };

        // Act
        var results = new List<string?>();
        await foreach (var result in _client!.UploadMultipleImagesAsync(request))
        {
            results.Add(result);
        }

        // Assert
        results.Count.ShouldBe(3);
        results.ShouldAllBe(r => !string.IsNullOrWhiteSpace(r) && r!.StartsWith("https://litter.catbox.moe/"));
        await TestContext.Out.WriteLineAsync($"Uploaded {results.Count} temporary files");
        foreach (var url in results)
        {
            await TestContext.Out.WriteLineAsync($"  - {url}");
        }
    }
}
