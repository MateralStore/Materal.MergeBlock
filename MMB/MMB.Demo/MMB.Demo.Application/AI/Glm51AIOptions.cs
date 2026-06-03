namespace MMB.Demo.Application.AI;

/// <summary>
/// GLM5.1 AI配置
/// </summary>
public class Glm51AIOptions
{
    /// <summary>
    /// 配置节点
    /// </summary>
    public const string ConfigKey = "MergeBlock:AI:GLM51";
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enable { get; set; } = true;
    /// <summary>
    /// API地址
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.z.ai/api/paas/v4/";
    /// <summary>
    /// API密钥
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    /// <summary>
    /// 模型名称
    /// </summary>
    public string Model { get; set; } = "glm-5.1";
    /// <summary>
    /// 温度
    /// </summary>
    public float? Temperature { get; set; } = 0.6f;
    /// <summary>
    /// 最大输出Token数
    /// </summary>
    public int? MaxOutputTokens { get; set; } = 2048;
}
