using System.Runtime.CompilerServices;

namespace CatBox.NET;

/// <summary>
/// This library targets .NET Standard 2.1 and thus, does not contain the newer ArgumentException family of guard clauses
/// This is a backport of those two methods.
/// </summary>
internal static class Throw
{
    public static void IfStringIsNullOrWhitespace(string? s, string exceptionMessage, [CallerArgumentExpression("s")] string memberName = "")
    {
        if (string.IsNullOrWhiteSpace(s)) throw new ArgumentNullException(memberName, exceptionMessage);
    }

    public static void IfNull(object? s, [CallerArgumentExpression("s")] string memberName = "")
    {
        if (s is null) throw new ArgumentNullException(memberName, "Argument cannot be null");
    }
}