using Materal.MergeBlock.AI.Abstractions.Runtime;

namespace Materal.MergeBlock.AI.Test.Runtime;

[TestClass]
public class AIAgentModelConfigTest
{
    [TestMethod]
    public void NewModelConfig_ShouldKeepProviderNeutralFields()
    {
        AIAgentModelConfig config = new()
        {
            Provider = "openai_compatible",
            Adapter = "deepseek_openai",
            Model = "model-a",
            BaseUrl = "https://example.test/v1",
            ApiKey = "secret",
            Temperature = 0.2f,
            MaxTokens = 2048,
            Reasoning = new AIAgentReasoningConfig
            {
                Enabled = true,
                Effort = "high",
                BudgetTokens = 8192,
                Summary = "auto"
            },
            Thinking = new AIAgentThinkingConfig
            {
                Enabled = true,
                BudgetTokens = 4096
            }
        };

        Assert.AreEqual("openai_compatible", config.Provider);
        Assert.AreEqual("deepseek_openai", config.Adapter);
        Assert.AreEqual("model-a", config.Model);
        Assert.AreEqual("https://example.test/v1", config.BaseUrl);
        Assert.AreEqual("secret", config.ApiKey);
        Assert.AreEqual(0.2f, config.Temperature);
        Assert.AreEqual(2048, config.MaxTokens);
        Assert.IsNotNull(config.Reasoning);
        Assert.IsNotNull(config.Thinking);
    }

    [TestMethod]
    public void NewRunRequest_ShouldCarryModelAndReviewSettings()
    {
        AIAgentRunRequest request = new()
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

        Assert.AreEqual("openai", request.ModelConfig.Provider);
        Assert.AreEqual("analysis", request.SkillRequest!.Name);
        Assert.IsTrue(request.PreExecutionReview.Enabled);
    }
}
