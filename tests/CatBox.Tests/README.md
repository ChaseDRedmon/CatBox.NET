# CatBox.NET Test Suite

This directory contains the test suite for the CatBox.NET library, including both unit tests and integration tests for the CatBox.moe and Litterbox file hosting services.

## Test Categories

### Unit Tests (No API Calls Required)

These tests use mocked HTTP clients and don't require any configuration or API credentials:

- **CommonTests.cs** - Tests for validation logic, file extension checking, and request validation
- **CatBoxClientTests.cs** - Tests for CatBox client functionality with mocked HTTP responses
- **LitterboxClientTests.cs** - Tests for Litterbox client functionality with mocked HTTP responses

### Integration Tests (Real API Calls)

These tests make actual API calls to CatBox.moe and Litterbox services and require a valid CatBox user hash:

- **CatBoxClientIntegrationTests.cs** - Real API testing for:
  - File uploads (from disk, stream, and URL)
  - Album creation and management
  - File deletion
  - Automatic cleanup of test resources

- **LitterboxClientIntegrationTests.cs** - Real API testing for:
  - Temporary file uploads with various expiry times (1h, 12h, 1d, 3d)
  - Stream-based uploads
  - Multiple file uploads

## Prerequisites

- **.NET 9.0 SDK** or later
- **CatBox.moe account** (only for integration tests)

## Getting Your CatBox User Hash

Integration tests require a CatBox user hash to enable file and album deletion. Follow these steps:

1. **Create a CatBox Account**
   - Visit https://catbox.moe and create an account (free)

2. **Access Your User Management Page**
   - Navigate to https://catbox.moe/user/manage.php
   - Log in if not already logged in

3. **Locate Your User Hash**
   - On the management page, find the "User Hash" field
   - Copy the alphanumeric hash value (e.g., `1234567890abcdef1234567890abcdef`)

## Configuration for Integration Tests

Integration tests will automatically skip with an informative message if no credentials are configured. Configure using one of the following methods:

### Option A: User Secrets (Recommended for Local Development)

User Secrets store credentials outside your project directory, preventing accidental commits to source control.

```bash
# Navigate to the test project directory
cd tests/CatBox.Tests

# Set your user hash
dotnet user-secrets set "CatBox:UserHash" "your-user-hash-here"

# Verify it was set (optional)
dotnet user-secrets list
```

**Where are secrets stored?**
- Windows: `%APPDATA%\Microsoft\UserSecrets\f7c8b9e3-4a5d-4e2f-9b3c-1d8e7f6a5b4c\secrets.json`
- Linux/Mac: `~/.microsoft/usersecrets/f7c8b9e3-4a5d-4e2f-9b3c-1d8e7f6a5b4c/secrets.json`

### Option B: Environment Variables (CI/CD & Alternative)

Environment variables are useful for CI/CD pipelines and as an alternative configuration method.

**Windows PowerShell:**
```powershell
$env:CATBOX_USER_HASH="your-user-hash-here"
```

**Windows Command Prompt:**
```cmd
set CATBOX_USER_HASH=your-user-hash-here
```

**Linux / macOS:**
```bash
export CATBOX_USER_HASH=your-user-hash-here
```

**Permanent Configuration (Linux/macOS):**
```bash
# Add to ~/.bashrc or ~/.zshrc
echo 'export CATBOX_USER_HASH="your-user-hash-here"' >> ~/.bashrc
source ~/.bashrc
```

### Configuration Priority

When both are present, User Secrets take precedence over Environment Variables:
1. User Secrets (highest priority)
2. Environment Variables (fallback)

## Running Tests

### Run All Tests

```bash
dotnet test
```

### Run Only Unit Tests

Excludes integration tests, perfect for quick local development:

```bash
dotnet test --filter Category!=Integration
```

### Run Only Integration Tests

Runs only tests that make real API calls:

```bash
dotnet test --filter Category=Integration
```

### Run Specific Test Class

```bash
# Run only CatBox client tests
dotnet test --filter FullyQualifiedName~CatBoxClientTests

# Run only integration tests for CatBox
dotnet test --filter FullyQualifiedName~CatBoxClientIntegrationTests

# Run only Litterbox tests
dotnet test --filter FullyQualifiedName~LitterboxClientTests
```

### Run with Verbose Output

```bash
dotnet test --logger "console;verbosity=detailed"
```

## Test Behavior

### Unit Tests
- Always run regardless of configuration
- Complete instantly (no network I/O)
- Use mocked HTTP responses
- Test validation logic and code paths

### Integration Tests Without Configuration
If `CatBox:UserHash` is not configured, integration tests will:
- Skip gracefully with a message
- Display setup instructions in the output
- Not fail or error
- Not make any API calls

Example skip message:
```
Integration tests skipped: CatBox:UserHash not configured.
Set via: dotnet user-secrets set "CatBox:UserHash" "your-hash"
or environment variable: CATBOX_USER_HASH=your-hash
```

### Integration Tests With Configuration
- Make real HTTP requests to CatBox.moe/Litterbox
- Upload actual test files (PNG image)
- Create, modify, and delete albums
- **Automatically clean up** all uploaded resources in teardown
- May take several seconds to complete

## Project Structure

```
CatBox.Tests/
├── README.md                              # This file
├── CatBox.Tests.csproj                    # Project file with UserSecretsId
├── CommonTests.cs                         # Unit tests for validation logic
├── CatBoxClientTests.cs                   # Unit tests for CatBox client
├── LitterboxClientTests.cs                # Unit tests for Litterbox client
├── CatBoxClientIntegrationTests.cs        # Integration tests for CatBox
├── LitterboxClientIntegrationTests.cs     # Integration tests for Litterbox
├── Helpers/
│   ├── HttpClientTestHelper.cs            # Mock HTTP client helper
│   └── IntegrationTestConfig.cs           # Configuration for integration tests
└── Images/
    └── test-file.png                      # PNG test file for uploads
```

## Integration Test Cleanup

Integration tests automatically clean up all resources they create:

1. **File Tracking**: Every uploaded file URL is tracked in a static collection
2. **Album Tracking**: Every created album ID is tracked separately
3. **Cleanup Order**:
   - Albums are deleted first (they reference files)
   - Individual files are deleted second
4. **Teardown Execution**: Cleanup runs even if tests fail via `[OneTimeTearDown]`

## Contributing

### Adding New Unit Tests

1. Create test methods in the appropriate test class
2. Use `[Test]` or `[TestCase]` attributes
3. Follow Arrange-Act-Assert pattern
4. Use Shouldly for assertions

Example:
```csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var input = "test";

    // Act
    var result = MethodUnderTest(input);

    // Assert
    result.ShouldBe("expected");
}
```

### Adding New Integration Tests

1. Add tests to `*IntegrationTests.cs` classes
2. Mark with `[Category("Integration")]` attribute
3. Use `[Order(n)]` to control execution sequence if needed
4. Track uploaded resources for cleanup:
   ```csharp
   TrackUploadedFile(uploadedUrl);
   TrackCreatedAlbum(albumUrl);
   ```

## Troubleshooting

### Integration Tests Are Skipping

**Symptom**: Integration tests show as "Skipped" in test output

**Solution**: Configure your CatBox user hash using one of the methods in the Configuration section above

### How to Verify Configuration

```bash
# Check if user secrets are configured
cd tests/CatBox.Tests
dotnet user-secrets list

# Check environment variable (Windows PowerShell)
$env:CATBOX_USER_HASH

# Check environment variable (Linux/Mac)
echo $CATBOX_USER_HASH
```

### Integration Tests Are Failing

1. **Verify your user hash is correct**: Visit https://catbox.moe/user/manage.php and confirm the hash
2. **Check network connectivity**: Ensure you can access catbox.moe from your network
3. **Review test output**: Look for specific error messages about API responses
4. **Check API limits**: CatBox may have rate limits or temporary restrictions

### User Secrets Not Working

If user secrets aren't being recognized:

1. Verify you're in the correct directory: `tests/CatBox.Tests`
2. Check that `UserSecretsId` exists in `CatBox.Tests.csproj`
3. Verify the secrets file exists at the location shown above
4. Try setting an environment variable as a fallback

### Build Errors After Adding Packages

If you encounter package version conflicts:

```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

## Testing Framework Reference

- **NUnit**: Test framework - https://nunit.org/
- **Shouldly**: Assertion library - https://docs.shouldly.org/
- **NSubstitute**: Mocking library - https://nsubstitute.github.io/

## Additional Resources

- [CatBox.NET Main Documentation](../../README.md)
- [CatBox API Documentation](https://catbox.moe/api)
- [.NET User Secrets Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [NUnit Documentation](https://docs.nunit.org/)
