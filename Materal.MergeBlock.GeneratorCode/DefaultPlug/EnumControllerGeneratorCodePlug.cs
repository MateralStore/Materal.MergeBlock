namespace Materal.MergeBlock.GeneratorCode.DefaultPlug;

/// <summary>
/// 枚举控制器代码生成插件
/// </summary>
public class EnumControllerGeneratorCodePlug : IMergeBlockGeneratorCodePlug
{
    /// <inheritdoc/>
    public Task BeforeExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public Task ExcuteAsync(GeneratorCodeContext context) => Task.CompletedTask;

    /// <inheritdoc/>
    public async Task AfterExcuteAsync(GeneratorCodeContext context)
    {
        if (context.Enums.Count <= 0) return;
        StringBuilder codeContent = new();
        codeContent.AppendLine($"using Microsoft.AspNetCore.Authorization;");
        codeContent.AppendLine($"using {context.ProjectName}.{context.ModuleName}.Abstractions.Enums;");
        codeContent.AppendLine($"");
        codeContent.AppendLine($"namespace {context.ProjectName}.{context.ModuleName}.Abstractions.HttpClient");
        codeContent.AppendLine($"{{");
        codeContent.AppendLine($"    /// <summary>");
        codeContent.AppendLine($"    /// 枚举控制器");
        codeContent.AppendLine($"    /// </summary>");
        codeContent.AppendLine($"    [AllowAnonymous]");
        codeContent.AppendLine($"    public partial class EnumsController : {context.ModuleName}Controller");
        codeContent.AppendLine($"    {{");
        foreach (EnumModel @enum in context.Enums)
        {
            if (@enum.HasAttribute<NotControllerAttribute>()) return;
            string annotation = $"        /// 获取所有{@enum.Annotation}";
            if (!annotation.EndsWith("枚举"))
            {
                annotation += "枚举";
            }
            codeContent.AppendLine($"        /// <summary>");
            codeContent.AppendLine(annotation);
            codeContent.AppendLine($"        /// </summary>");
            codeContent.AppendLine($"        /// <returns></returns>");
            codeContent.AppendLine($"        [HttpGet]");
            codeContent.AppendLine($"        public ResultModel<List<KeyValueModel<{@enum.Name}>>> GetAll{@enum.Name}()");
            codeContent.AppendLine($"        {{");
            codeContent.AppendLine($"            List<KeyValueModel<{@enum.Name}>> result = KeyValueModel<{@enum.Name}>.GetAllCode();");
            codeContent.AppendLine($"            return ResultModel<List<KeyValueModel<{@enum.Name}>>>.Success(result, \"获取成功\");");
            codeContent.AppendLine($"        }}");
        }
        codeContent.AppendLine($"    }}");
        codeContent.AppendLine($"}}");
        context.SaveAs(codeContent, context.ModuleApplicationMGCPath, "Controllers", $"EnumsController.cs");
    }
}