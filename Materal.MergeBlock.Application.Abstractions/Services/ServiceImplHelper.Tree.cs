using Materal.MergeBlock.Repository.Abstractions.Repositories;

namespace Materal.MergeBlock.Application.Abstractions.Services;

public static partial class ServiceImplHelper
{
    /// <summary>
    /// 移动节点
    /// </summary>
    /// <param name="model"></param>
    /// <param name="repository"></param>
    /// <returns></returns>
    /// <exception cref="MergeBlockModuleException"></exception>
    public static async Task<TDomain> MoveAsync<TRepository, TDomain>(MoveTreeNodeModel model, TRepository repository)
        where TRepository : IRepository<TDomain, Guid>, ITreeRepository<TDomain>
        where TDomain : class, ITreeDomain, new()
    {
        // 不能将自己设置为自己的父级
        if (model.SourceID == model.TargetID) throw new MergeBlockModuleException("不能将自己设置为自己的父级");
        // 获取源对象
        TDomain? sourceDomain = await repository.FirstOrDefaultAsync(model.SourceID) ?? throw new MergeBlockModuleException("源对象不存在");
        // 如果目标父级不为空，需要验证目标父级是否存在以及不能将父级设置为自己的子节点
        if (model.TargetID.HasValue)
        {
            // 验证不能将父级设置为自己的子节点（避免循环引用）
            if (IsChildNode<TRepository, TDomain>(repository, model.SourceID, model.TargetID.Value)) throw new MergeBlockModuleException("不能将父级设置为自己的子节点");
        }
        // 更改父级
        sourceDomain.ParentID = model.TargetID;
        return sourceDomain;
    }

    /// <summary>
    /// 检查目标节点是否是源节点的子节点
    /// </summary>
    /// <typeparam name="TRepository"></typeparam>
    /// <typeparam name="TDomain"></typeparam>
    /// <param name="repository"></param>
    /// <param name="sourceID">源节点ID</param>
    /// <param name="targetID">目标节点ID</param>
    /// <returns>如果是子节点返回true，否则返回false</returns>
    private static bool IsChildNode<TRepository, TDomain>(TRepository repository, Guid sourceID, Guid targetID)
        where TRepository : ITreeRepository<TDomain>
        where TDomain : class, ITreeDomain, new()
    {
        // 获取源节点的所有递归子节点ID
        List<Guid> childIDs = repository.GetAllRecursiveChildrenID(sourceID);
        // 检查目标节点是否在子节点集合中
        return childIDs.Contains(targetID);
    }

    /// <summary>
    /// 获得查询树结构领域表达式
    /// </summary>
    /// <typeparam name="TDomain">领域类型</typeparam>
    /// <typeparam name="TQueryModel">查询模型类型</typeparam>
    /// <param name="expression">表达式</param>
    /// <param name="model">模型</param>
    /// <returns>表达式</returns>
    public static Expression<Func<TDomain, bool>> GetSearchTreeDomainExpression<TDomain, TQueryModel>(Expression<Func<TDomain, bool>> expression, TQueryModel model)
        where TQueryModel : notnull
    {
        if (!typeof(TDomain).IsAssignableTo<ITreeDomain>()) return expression;
        PropertyInfo? modelParentIDPropertyInfo = model.GetType().GetProperty(nameof(ITreeDomain.ParentID));
        if (modelParentIDPropertyInfo == null) return expression;
        object? modelParentID = modelParentIDPropertyInfo.GetValue(model);
        if (modelParentID != null) return expression;
        ParameterExpression parameterExpression = expression.Parameters[0];
        MemberExpression memberExpression = Expression.Property(parameterExpression, nameof(ITreeDomain.ParentID));
        BinaryExpression binaryExpression = Expression.Equal(memberExpression, Expression.Constant(null));
        Expression<Func<TDomain, bool>> newExpression = Expression.Lambda<Func<TDomain, bool>>(binaryExpression, parameterExpression);
        expression = expression.Compose(newExpression, new Func<Expression, Expression, Expression>(Expression.AndAlso));
        return expression;
    }
}
