namespace Materal.MergeBlock.AI.Web.Persistence;

/// <summary>
/// Agent追踪脱敏器
/// </summary>
public static class AgentTraceRedactor
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "api_key",
        "apiKey",
        "authorization",
        "token",
        "password",
        "secret"
    };

    /// <summary>
    /// 脱敏
    /// </summary>
    public static IReadOnlyDictionary<string, object?> Redact(IReadOnlyDictionary<string, object?> payload, int maxTextLength = 1024)
    {
        Dictionary<string, object?> result = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, object?> item in payload)
        {
            if (SensitiveKeys.Contains(item.Key)) continue;
            result[item.Key] = RedactValue(item.Value, maxTextLength);
        }
        return result;
    }

    private static object? RedactValue(object? value, int maxTextLength)
    {
        return value switch
        {
            string text when text.Length > maxTextLength => text[..maxTextLength],
            IReadOnlyDictionary<string, object?> dictionary => Redact(dictionary, maxTextLength),
            IDictionary<string, object?> dictionary => Redact(new Dictionary<string, object?>(dictionary), maxTextLength),
            IEnumerable<object?> values => values.Select(m => RedactValue(m, maxTextLength)).ToArray(),
            _ => value
        };
    }
}
