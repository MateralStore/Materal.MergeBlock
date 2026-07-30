using Materal.MergeBlock.AI.Abstractions.Runtime;
using Materal.MergeBlock.AI.Context;
using Materal.MergeBlock.AI.Prompts;
using Materal.MergeBlock.AI.Web.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace Materal.MergeBlock.AI.Web.Test.Runtime;

[TestClass]
public class AIAgentRuntimeRequestFactoryTest
{
    [TestMethod]
    public async Task CreateRunRequestAsync_ShouldCarryModelSettingsAndContext()
    {
        ServiceCollection services = new();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        AIAgentRuntimeRequestFactory factory = new(
            new AIContextBuilder(serviceProvider, []),
            new AIPromptBuilder([]));
        AgentChatRequest request = new()
        {
            ThreadId = "thread_001",
            RunId = "run_001",
            Message = "hello",
            ModelConfig = new AIAgentModelConfig
            {
                Provider = "openai",
                Model = "gpt-test",
                ApiKey = "secret"
            },
            SkillRequest = new AIAgentSkillRequest
            {
                Name = "analysis",
                Description = "Use analysis capability"
            },
            PreExecutionReview = new AIAgentPreExecutionReviewConfig
            {
                Enabled = true
            }
        };

        AIAgentRunRequest runtimeRequest = await factory.CreateRunRequestAsync(
            request,
            "thread_001",
            "run_001",
            CancellationToken.None);

        Assert.AreEqual("thread_001", runtimeRequest.ThreadId);
        Assert.AreEqual("run_001", runtimeRequest.RunId);
        Assert.AreEqual("hello", runtimeRequest.Message);
        Assert.AreEqual("openai", runtimeRequest.ModelConfig.Provider);
        Assert.AreEqual("analysis", runtimeRequest.SkillRequest!.Name);
        Assert.IsTrue(runtimeRequest.PreExecutionReview.Enabled);
    }
}
