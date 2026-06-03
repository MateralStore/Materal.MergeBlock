namespace Materal.MergeBlock.AI.Tools;

/// <summary>
/// AI工具扫描器
/// </summary>
public class AIToolScanner
{
    /// <summary>
    /// 扫描程序集
    /// </summary>
    /// <param name="assemblies">程序集</param>
    /// <returns>工具描述列表</returns>
    public IReadOnlyList<AIToolDescriptor> Scan(params Assembly[] assemblies)
    {
        List<AIToolDescriptor> result = [];
        foreach (Assembly assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                MergeBlockAIToolAttribute? attribute = type.GetCustomAttribute<MergeBlockAIToolAttribute>();
                if (attribute is not null)
                {
                    result.Add(new AIToolDescriptor
                    {
                        Name = string.IsNullOrWhiteSpace(attribute.Name) ? type.Name : attribute.Name,
                        Description = attribute.Description,
                        ExecutionMode = attribute.ExecutionMode,
                        RequiredPermission = attribute.RequiredPermission,
                        ImplementationType = type
                    });
                }
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    MergeBlockAIToolAttribute? methodAttribute = method.GetCustomAttribute<MergeBlockAIToolAttribute>();
                    if (methodAttribute is null) continue;
                    result.Add(new AIToolDescriptor
                    {
                        Name = string.IsNullOrWhiteSpace(methodAttribute.Name) ? method.Name : methodAttribute.Name,
                        Description = methodAttribute.Description,
                        ExecutionMode = methodAttribute.ExecutionMode,
                        RequiredPermission = methodAttribute.RequiredPermission,
                        ImplementationType = type
                    });
                }
            }
        }
        return result;
    }
}
