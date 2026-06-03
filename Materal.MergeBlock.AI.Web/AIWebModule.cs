using Materal.MergeBlock.AI.Web.Cancellation;

namespace Materal.MergeBlock.AI.Web;

/// <summary>
/// AI Web模块
/// </summary>
[DependsOn(typeof(AIModule), typeof(WebModule))]
public class AIWebModule() : MergeBlockModule("AI Web模块")
{
    /// <inheritdoc />
    public override void OnConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<RemoteToolGateway>();
        context.Services.AddSingleton<AIAgentCancellationRegistry>();
        context.Services.AddSingleton<AIAgentStreamAdapter>();
        context.Services.TryAddSingleton<IAIAgentStateStore>(_ => new SqliteAIAgentStateStore("data/ai-agent.sqlite3"));
        base.OnConfigureServices(context);
    }
}
