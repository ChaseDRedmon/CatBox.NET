using System.Net;
using Microsoft.Extensions.Logging;

namespace CatBox.NET.Logging;

public static partial class LoggerExtensions
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Error, Message = "HttpStatus: {StatusCode} - {Message}")]
    public static partial void LogCatBoxAPIException(this ILogger logger, HttpStatusCode statusCode, string message);
}