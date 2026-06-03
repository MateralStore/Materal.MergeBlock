namespace Materal.MergeBlock.AI.Test;

[TestClass]
public class AIModuleTest
{
    [TestMethod]
    public void AddMergeBlockAI_ShouldRegisterCoreServices()
    {
        ServiceCollection services = new();

        services.AddMergeBlockAI();
        ServiceProvider provider = services.BuildServiceProvider();

        Assert.IsNotNull(provider.GetRequiredService<AIToolRegistry>());
        Assert.IsNotNull(provider.GetRequiredService<AIPromptBuilder>());
        Assert.AreEqual(1, provider.GetServices<IAIToolCallAuditor>().Count());
    }

    [TestMethod]
    public void OnApplicationInitialization_ShouldRegisterScannedAndProvidedTools()
    {
        ServiceCollection services = new();
        services.AddSingleton(new MergeBlockContext
        {
            MergeBlockAssemblies = [typeof(AIModuleApplicationTool).Assembly]
        });
        services.AddSingleton<IAIToolMetadataProvider, TestToolMetadataProvider>();
        AIModule module = new();
        module.OnConfigureServices(new ServiceConfigurationContext(services, null));
        ServiceProvider provider = services.BuildServiceProvider();

        module.OnApplicationInitialization(new ApplicationInitializationContext(provider));

        AIToolRegistry registry = provider.GetRequiredService<AIToolRegistry>();
        Assert.IsNotNull(registry.GetRequired("applicationTool"));
        Assert.IsNotNull(registry.GetRequired("providedTool"));
    }

    [MergeBlockAITool("应用工具", Name = "applicationTool")]
    private sealed class AIModuleApplicationTool;

    private sealed class TestToolMetadataProvider : IAIToolMetadataProvider
    {
        public IEnumerable<AIToolDescriptor> GetToolDescriptors()
        {
            yield return new AIToolDescriptor
            {
                Name = "providedTool",
                Description = "提供器工具",
                ExecutionMode = AIToolExecutionMode.Remote
            };
        }
    }
}
