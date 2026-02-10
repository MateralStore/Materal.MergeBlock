using Scriban;
using Scriban.Runtime;

namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 枚举控制器代码生成插件
/// </summary>
public class EnumControllerGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    /// <summary>
    /// 枚举视图模型（用于模板渲染）
    /// </summary>
    private class EnumViewModel
    {
        /// <summary>
        /// 枚举名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 获取全部的注释说明
        /// </summary>
        public string GetAllSummary { get; set; } = string.Empty;
    }

    private static readonly string _enumControllerTemplate = @"
using Microsoft.AspNetCore.Authorization;
using Materal.Utils.Enums;
{{- if enums.size > 0 }}
using {{context.ProjectName}}.{{context.ModuleName}}.Abstractions.Enums;
{{- end }}

namespace {{context.ProjectName}}.{{context.ModuleName}}.Application.Controllers;

/// <summary>
/// 枚举控制器
/// </summary>
[AllowAnonymous]
public partial class EnumsController : {{context.ModuleName}}Controller
{
{{- for enumModel in enums }}
    /// <summary>
    /// {{enumModel.GetAllSummary}}
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ResultModel<List<KeyValueModel<{{enumModel.Name}}>>> GetAll{{enumModel.Name}}()
    {
        List<KeyValueModel<{{enumModel.Name}}>> result = KeyValueModel<{{enumModel.Name}}>.GetAllCode();
        return ResultModel<List<KeyValueModel<{{enumModel.Name}}>>>.Success(result, ""获取成功"");
    }
{{- end }}

    /// <summary>
    /// 获取所有返回对象类型枚举
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public ResultModel<List<KeyValueModel<ResultType>>> GetAllResultType()
    {
        List<KeyValueModel<ResultType>> result = KeyValueModel<ResultType>.GetAllCode();
        return ResultModel<List<KeyValueModel<ResultType>>>.Success(result, ""获取成功"");
    }
}
";

    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task ExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task AfterExcuteAsync(GeneratorCodeContext context)
    {
        Template template = Template.Parse(_enumControllerTemplate);
        List<EnumViewModel> enums = [.. context.Enums
            .Where(e => e.Name != "ResultType")
            .Select(e => new EnumViewModel
            {
                Name = e.Name,
                GetAllSummary = GetEnumGetAllSummary(e.Annotation)
            })];

        string codeContent = RenderTemplate(template, context, enums);
        context.SaveAs(new StringBuilder(codeContent), context.ModuleApplicationMGCPath, "Controllers", "EnumsController.cs");
        return Task.CompletedTask;
    }

    private static string GetEnumGetAllSummary(string? annotation)
    {
        string summary = $"获取所有{annotation}";
        if (!summary.EndsWith("枚举"))
        {
            summary += "枚举";
        }
        return summary;
    }

    /// <summary>
    /// 渲染模板
    /// </summary>
    private static string RenderTemplate(Template template, GeneratorCodeContext context, List<EnumViewModel> enums)
    {
        ScriptObject scriptObject = new()
        {
            { "context", context },
            { "enums", enums }
        };

        TemplateContext templateContext = new()
        {
            MemberRenamer = member => member.Name,
            LoopLimit = int.MaxValue,
            StrictVariables = false
        };
        templateContext.PushGlobal(scriptObject);

        return template.Render(templateContext);
    }
}
