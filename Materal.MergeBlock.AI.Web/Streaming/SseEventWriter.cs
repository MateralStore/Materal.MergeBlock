namespace Materal.MergeBlock.AI.Web.Streaming;

/// <summary>
/// SSE事件写入器
/// </summary>
public static class SseEventWriter
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
    /// <summary>
    /// 格式化SSE事件
    /// </summary>
    /// <param name="streamEvent">流式事件</param>
    /// <returns>SSE文本</returns>
    public static string Format(AgentStreamEvent streamEvent)
    {
        string json = JsonSerializer.Serialize(streamEvent, _jsonSerializerOptions);
        return $"event: {streamEvent.Event}\r\ndata: {json}\r\n\r\n";
    }
}
