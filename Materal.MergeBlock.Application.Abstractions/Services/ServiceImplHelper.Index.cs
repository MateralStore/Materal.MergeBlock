namespace Materal.MergeBlock.Application.Abstractions.Services;

public static partial class ServiceImplHelper
{
    /// <summary>
    /// 更改位序
    /// </summary>
    /// <typeparam name="TRepository"></typeparam>
    /// <typeparam name="TDomain"></typeparam>
    /// <param name="model"></param>
    /// <param name="repository"></param>
    /// <param name="groupProperties"></param>
    /// <returns></returns>
    /// <exception cref="MergeBlockModuleException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<List<TDomain>> MoveAsync<TRepository, TDomain>(MoveIndexModel model, TRepository repository, params string[] groupProperties)
        where TRepository : IRepository<TDomain, Guid>
        where TDomain : class, IIndexDomain, new()
    {
        // 源对象和目标对象不能是同一个
        if (model.SourceID == model.TargetID) throw new MergeBlockModuleException("不能以自己为排序对象");
        Expression<Func<TDomain, bool>> searchExpression = m => true;
        if (groupProperties.Length > 0) // 有分组
        {
            List<TDomain> tesmpDomains = await repository.FindAsync(m => m.ID == model.SourceID || m.ID == model.TargetID);
            if (tesmpDomains.Count < 2) throw new MergeBlockModuleException("源对象或目标对象不存在");
            searchExpression = GetGroupExpression(tesmpDomains, groupProperties);
        }
        List<TDomain> domains = await repository.FindAsync(searchExpression, m => m.Index);
        return MoveIndex(model, domains);
    }

    /// <summary>
    /// 获得分组表达式
    /// </summary>
    /// <typeparam name="TDomain"></typeparam>
    /// <param name="domains"></param>
    /// <param name="groupProperties"></param>
    /// <returns></returns>
    /// <exception cref="MergeBlockModuleException"></exception>
    /// <exception cref="ArgumentException"></exception>
    private static Expression<Func<TDomain, bool>> GetGroupExpression<TDomain>(List<TDomain> domains, params string[] groupProperties)
    {
        if (groupProperties.Length <= 0) throw new MergeBlockModuleException("没有分组属性");
        Type domainType = typeof(TDomain);
        ParameterExpression mValue = Expression.Parameter(domainType, "m");
        Expression? expression = null;
        foreach (string groupProperty in groupProperties)
        {
            PropertyInfo propertyInfo = domainType.GetProperty(groupProperty) ?? throw new ArgumentException($"属性名称{groupProperty}不存在");
            // 获取两个领域对象在该分组属性上的值
            object? value1 = propertyInfo.GetValue(domains[0]);
            object? value2 = propertyInfo.GetValue(domains[1]);
            // 校验两个对象的分组属性值是否一致，不一致则不允许移动位序
            if (value1 == null || value2 == null)
            {
                if (value1 != null || value2 != null) throw new MergeBlockModuleException("不是同一组数据不能更改位序");
            }
            else
            {
                if (!value1.Equals(value2)) throw new MergeBlockModuleException("不是同一组数据不能更改位序");
            }
            // 追加分组过滤条件: m.[groupProperty] == value1
            Expression equalExpression = Expression.Equal(Expression.PropertyOrField(mValue, propertyInfo.Name), Expression.Constant(value1, propertyInfo.PropertyType));
            if (expression is null)
            {
                expression = equalExpression;
            }
            else
            {
                expression = Expression.And(expression, equalExpression);
            }
        }
        return Expression.Lambda<Func<TDomain, bool>>(expression!, mValue);
    }

    /// <summary>
    /// 更改位序
    /// </summary>
    /// <typeparam name="TDomain"></typeparam>
    /// <param name="model"></param>
    /// <param name="domains"></param>
    /// <returns></returns>
    /// <exception cref="MergeBlockModuleException"></exception>
    private static List<TDomain> MoveIndex<TDomain>(MoveIndexModel model, List<TDomain> domains)
        where TDomain : class, IIndexDomain, new()
    {
        // 找到源对象和目标对象的位置
        int sourceIndex = -1;
        int targetIndex = -1;
        for (int i = 0; i < domains.Count; i++)
        {
            if (domains[i].ID == model.SourceID)
            {
                sourceIndex = i;
            }
            else if (domains[i].ID == model.TargetID)
            {
                targetIndex = i;
            }
        }
        if (sourceIndex == -1 || targetIndex == -1) throw new MergeBlockModuleException("源对象或目标对象不存在");
        // 移除源对象
        TDomain sourceDomain = domains[sourceIndex];
        domains.RemoveAt(sourceIndex);
        // 移除后目标位置可能需要调整
        if (sourceIndex < targetIndex)
        {
            targetIndex--;
        }
        // 计算目标插入位置：Before=true放到目标之前，Before=false放到目标之后
        int insertIndex = model.Before ? targetIndex : targetIndex + 1;
        // 执行移动
        domains.Insert(insertIndex, sourceDomain);
        // 重新设置所有实体的 Index
        List<TDomain> editDomains = [];
        for (int i = 0; i < domains.Count; i++)
        {
            if (domains[i].Index == i) continue;
            domains[i].Index = i;
            editDomains.Add(domains[i]);
        }
        return editDomains;
    }
}
