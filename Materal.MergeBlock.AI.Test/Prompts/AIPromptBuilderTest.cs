namespace Materal.MergeBlock.AI.Test.Prompts;

[TestClass]
public class AIPromptBuilderTest
{
    [TestMethod]
    public async Task BuildAsync_ShouldReadFrozenContext_AndAppendMessages()
    {
        ServiceCollection services = new();
        services.AddSingleton<IAIPromptContributor, PermissionPromptContributor>();
        ServiceProvider provider = services.BuildServiceProvider();
        AIContextSnapshot context = new(new Dictionary<string, object?>
        {
            ["permissions"] = new[] { "word.read" }
        });

        AIPromptBuilder builder = new(provider.GetServices<IAIPromptContributor>());
        IReadOnlyList<string> messages = await builder.BuildSystemMessagesAsync(context);

        Assert.AreEqual(1, messages.Count);
        StringAssert.Contains(messages[0], "只能读取文档");
    }

    private sealed class PermissionPromptContributor : IAIPromptContributor
    {
        public Task ContributeAsync(AIPromptContributionContext context)
        {
            string[] permissions = context.AIContext.GetRequired<string[]>("permissions");
            if (!permissions.Contains("word.edit"))
            {
                context.AddSystemMessage("当前用户只能读取文档，不能修改文档。");
            }
            return Task.CompletedTask;
        }
    }
}
