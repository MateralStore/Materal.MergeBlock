using Materal.MergeBlock.AI.Abstractions.Auditing;

namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// AI服务端工具结果
/// </summary>
public class AIServerToolResult
{
    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; init; } = AIToolCallStatus.Completed;
    /// <summary>
    /// 结果
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Result { get; init; }
    /// <summary>
    /// 错误
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Error { get; init; }

    /// <summary>
    /// 创建完成结果
    /// </summary>
    public static AIServerToolResult Completed(IReadOnlyDictionary<string, object?> result) => new()
    {
        Status = AIToolCallStatus.Completed,
        Result = result
    };

    /// <summary>
    /// 创建失败结果
    /// </summary>
    public static AIServerToolResult Failed(string code, string message) => new()
    {
        Status = AIToolCallStatus.Failed,
        Error = new Dictionary<string, object?>
        {
            ["code"] = code,
            ["message"] = message
        }
    };
}
