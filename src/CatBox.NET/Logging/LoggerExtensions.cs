using System.Net;
using Microsoft.Extensions.Logging;

namespace CatBox.NET.Logging;

public static class LoggerExtensions
{
    private static readonly Action<ILogger, HttpStatusCode, string, Exception> _logCatBoxException = LoggerMessage.Define<HttpStatusCode, string>(LogLevel.Error, new EventId(1000, "CatBox API"), "HttpStatus: {StatusCode} - {Message}");
    public static void LogCatBoxAPIException(this ILogger logger, HttpStatusCode code, string apiMessage) => _logCatBoxException(logger, code, apiMessage, default!);
}