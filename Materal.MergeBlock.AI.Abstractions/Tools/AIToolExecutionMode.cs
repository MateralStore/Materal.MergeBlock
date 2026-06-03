namespace Materal.MergeBlock.AI.Abstractions.Tools;

/// <summary>
/// AI工具执行模式
/// </summary>
public enum AIToolExecutionMode
{
    /// <summary>
    /// 服务端本地执行
    /// </summary>
    Local,
    /// <summary>
    /// 前端或远端宿主执行
    /// </summary>
    Remote
}
