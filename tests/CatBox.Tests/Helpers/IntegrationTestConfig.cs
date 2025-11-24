using Microsoft.Extensions.Configuration;

namespace CatBox.Tests.Helpers;

/// <summary>
/// Configuration helper for integration tests that make real API calls
/// Supports both User Secrets (for local development) and Environment Variables (for CI/CD)
/// </summary>
public static class IntegrationTestConfig
{
    /// <summary>
    /// Lazy-initialized configuration that reads from User Secrets and Environment Variables
    /// User Secrets override Environment Variables when both are present
    /// </summary>
    private static readonly Lazy<IConfiguration> _configuration = new(() =>
    {
        return new ConfigurationBuilder()
            .AddEnvironmentVariables()  // Base layer - for CI/CD pipelines
            .AddUserSecrets(typeof(IntegrationTestConfig).Assembly)  // Override layer - for local development
            .Build();
    });

    /// <summary>
    /// Gets the CatBox user hash from configuration
    /// Required for integration tests to enable file/album deletion
    /// Checks multiple sources in priority order:
    /// 1. User Secrets: "CatBox:UserHash" (preferred for local development)
    /// 2. Environment Variable: "CATBOX_USER_HASH" (backward compatibility and CI/CD)
    /// 3. Environment Variable: "CatBox__UserHash" (hierarchical format)
    /// </summary>
    public static string? UserHash =>
        _configuration.Value["CatBox:UserHash"] ??
        _configuration.Value["CATBOX_USER_HASH"];

    /// <summary>
    /// Gets whether integration tests should run (UserHash is configured)
    /// </summary>
    public static bool IsConfigured => !string.IsNullOrWhiteSpace(UserHash);

    /// <summary>
    /// Real CatBox API endpoint for integration tests
    /// </summary>
    public static Uri CatBoxUrl => new("https://catbox.moe/user/api.php");

    /// <summary>
    /// Real Litterbox API endpoint for integration tests
    /// </summary>
    public static Uri LitterboxUrl => new("https://litterbox.catbox.moe/resources/internals/api.php");

    /// <summary>
    /// Gets the path to the test PNG file
    /// </summary>
    public static string GetTestFilePath()
    {
        var testDirectory = NUnit.Framework.TestContext.CurrentContext.TestDirectory;
        return Path.Combine(testDirectory, "Images", "test-file.png");
    }
}
