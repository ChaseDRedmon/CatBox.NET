using System.Net;
using NSubstitute;

namespace CatBox.Tests.Helpers;

/// <summary>
/// Helper class for creating HttpClient instances with mocked responses for testing
/// </summary>
public static class HttpClientTestHelper
{
    /// <summary>
    /// Creates an HttpClient with a mocked handler that returns the specified response
    /// </summary>
    /// <param name="responseContent">The content to return in the response</param>
    /// <param name="statusCode">The HTTP status code to return (default: OK)</param>
    /// <returns>A configured HttpClient with mocked responses</returns>
    public static HttpClient CreateMockHttpClient(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var mockHandler = Substitute.ForPartsOf<MockableHttpMessageHandler>();

        mockHandler.PublicSendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseContent)
            }));

        return new HttpClient(mockHandler);
    }

    /// <summary>
    /// Creates an HttpClient with a mocked handler that throws an exception
    /// </summary>
    /// <param name="exception">The exception to throw</param>
    /// <returns>A configured HttpClient that throws the specified exception</returns>
    public static HttpClient CreateMockHttpClientWithException(Exception exception)
    {
        var mockHandler = Substitute.ForPartsOf<MockableHttpMessageHandler>();

        mockHandler.PublicSendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns<Task<HttpResponseMessage>>(_ => throw exception);

        return new HttpClient(mockHandler);
    }

    /// <summary>
    /// Creates an HttpClient with a mocked handler that captures the requestBase for inspection
    /// </summary>
    /// <param name="responseContent">The content to return in the response</param>
    /// <param name="capturedRequest">Out parameter that will contain the captured requestBase</param>
    /// <param name="statusCode">The HTTP status code to return (default: OK)</param>
    /// <returns>A configured HttpClient with mocked responses</returns>
    public static HttpClient CreateMockHttpClientWithRequestCapture(
        string responseContent,
        out HttpRequestMessage? capturedRequest,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        HttpRequestMessage? localCapturedRequest = null;
        var mockHandler = Substitute.ForPartsOf<MockableHttpMessageHandler>();

        mockHandler.PublicSendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                localCapturedRequest = callInfo.Arg<HttpRequestMessage>();
                var response = new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(responseContent)
                };
                return Task.FromResult(response);
            });

        capturedRequest = localCapturedRequest;
        return new HttpClient(mockHandler);
    }
}

/// <summary>
/// Mockable wrapper for HttpMessageHandler that exposes the protected SendAsync method
/// This is necessary because NSubstitute cannot mock protected members directly
/// </summary>
public abstract class MockableHttpMessageHandler : HttpMessageHandler
{
    public abstract Task<HttpResponseMessage> PublicSendAsync(HttpRequestMessage request, CancellationToken cancellationToken);

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return PublicSendAsync(request, cancellationToken);
    }
}
