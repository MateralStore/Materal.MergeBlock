using Materal.MergeBlock.AI.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Materal.MergeBlock.AI.Web.Test.Controllers;

[TestClass]
public class AIAgentControllerTest
{
    [TestMethod]
    public void Controller_ShouldNotAllowAnonymousAccess()
    {
        object[] attributes = typeof(AIAgentController).GetCustomAttributes(typeof(AllowAnonymousAttribute), false);

        Assert.AreEqual(0, attributes.Length);
    }

    [TestMethod]
    public void Controller_ShouldExposeCompatibleAgentRoutes()
    {
        RouteAttribute controllerRoute = typeof(AIAgentController).GetCustomAttribute<RouteAttribute>()!;
        MethodInfo streamMethod = typeof(AIAgentController).GetMethod(nameof(AIAgentController.StreamAsync))!;
        MethodInfo resumeMethod = typeof(AIAgentController).GetMethod(nameof(AIAgentController.ResumeAsync))!;
        MethodInfo cancelMethod = typeof(AIAgentController).GetMethod(nameof(AIAgentController.CancelAsync))!;
        MethodInfo sessionMethod = typeof(AIAgentController).GetMethod(nameof(AIAgentController.GetSessionAsync))!;
        MethodInfo runMethod = typeof(AIAgentController).GetMethod(nameof(AIAgentController.GetRunAsync))!;
        MethodInfo debugTracesMethod = typeof(AIAgentController).GetMethod(nameof(AIAgentController.GetDebugTracesAsync))!;
        MethodInfo debugTraceMethod = typeof(AIAgentController).GetMethod(nameof(AIAgentController.GetDebugTraceAsync))!;
        MethodInfo skillsMethod = typeof(AIAgentController).GetMethod(nameof(AIAgentController.GetSkills))!;

        string streamRoute = CombineRoute(controllerRoute.Template, streamMethod.GetCustomAttribute<HttpPostAttribute>()!.Template);
        string resumeRoute = CombineRoute(controllerRoute.Template, resumeMethod.GetCustomAttribute<HttpPostAttribute>()!.Template);
        string cancelRoute = CombineRoute(controllerRoute.Template, cancelMethod.GetCustomAttribute<HttpPostAttribute>()!.Template);
        string sessionRoute = CombineRoute(controllerRoute.Template, sessionMethod.GetCustomAttribute<HttpGetAttribute>()!.Template);
        string runRoute = CombineRoute(controllerRoute.Template, runMethod.GetCustomAttribute<HttpGetAttribute>()!.Template);
        string debugTracesRoute = CombineRoute(controllerRoute.Template, debugTracesMethod.GetCustomAttribute<HttpGetAttribute>()!.Template);
        string debugTraceRoute = CombineRoute(controllerRoute.Template, debugTraceMethod.GetCustomAttribute<HttpGetAttribute>()!.Template);
        string skillsRoute = CombineRoute(controllerRoute.Template, skillsMethod.GetCustomAttribute<HttpGetAttribute>()!.Template);

        Assert.AreEqual("agent/chat/stream", streamRoute);
        Assert.AreEqual("agent/chat/resume/stream", resumeRoute);
        Assert.AreEqual("agent/runs/{runId}/cancel", cancelRoute);
        Assert.AreEqual("agent/sessions/{threadId}", sessionRoute);
        Assert.AreEqual("agent/runs/{runId}", runRoute);
        Assert.AreEqual("agent/debug-traces", debugTracesRoute);
        Assert.AreEqual("agent/debug-traces/{traceId}", debugTraceRoute);
        Assert.AreEqual("agent/skills", skillsRoute);
    }

    [TestMethod]
    public void GetSkills_ShouldReturnRegisteredCatalogItems()
    {
        ServiceCollection services = new();
        services.AddSingleton<IAIAgentSkillCatalogProvider>(new StubSkillCatalogProvider());
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        AIAgentController controller = new(
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            [],
            serviceProvider);

        AgentSkillCatalogResponse response = controller.GetSkills();

        Assert.AreEqual("agent-skill-catalog-v1", response.SchemaVersion);
        Assert.AreEqual(1, response.Skills.Count);
        Assert.AreEqual("word-agent", response.Skills[0].Id);
        Assert.AreEqual("Word Agent", response.Skills[0].Name);
        Assert.AreEqual("Word automation skill", response.Skills[0].Description);
    }

    private static string CombineRoute(string? controllerTemplate, string? actionTemplate)
    {
        return string.Join("/", new[] { controllerTemplate, actionTemplate }
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Select(m => m!.Trim('/')));
    }

    private sealed class StubSkillCatalogProvider : IAIAgentSkillCatalogProvider
    {
        public IReadOnlyList<AgentSkillCatalogItem> GetSkills() =>
        [
            new AgentSkillCatalogItem
            {
                Id = "word-agent",
                Name = "Word Agent",
                Description = "Word automation skill"
            }
        ];
    }
}
