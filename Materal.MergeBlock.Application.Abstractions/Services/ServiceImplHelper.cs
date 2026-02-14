namespace Materal.MergeBlock.Application.Abstractions.Services;

/// <summary>
/// 服务实现帮助
/// </summary>
public static partial class ServiceImplHelper
{
    /// <summary>
    /// 更改附件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TRepository"></typeparam>
    /// <param name="fileIDs"></param>
    /// <param name="repository"></param>
    /// <param name="unitOfWork"></param>
    /// <param name="targetName"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public static async Task ChangeAdjunctsAsync<T, TRepository>(Guid[] fileIDs, TRepository repository, IMergeBlockUnitOfWork unitOfWork, string targetName, Guid? id = null)
        where T : class, IAdjunctDomain, new()
        where TRepository : IEFRepository<T, Guid>
    {
        // 通过反射获取目标关联属性(例如某个业务实体的ID属性)，用于关联附件与业务对象
        Type tType = typeof(T);
        PropertyInfo propertyInfo = tType.GetProperty(targetName) ?? throw new MergeBlockModuleException("操作附件失败");
        ICollection<Guid> addIDs;
        if (id == null)
        {
            // id为空表示是新增场景，所有传入的fileIDs都需要新增
            addIDs = fileIDs;
        }
        else
        {
            // id不为空表示是编辑场景，需要做增量更新(对比新旧附件列表，计算需要新增和删除的部分)
            // 动态构建查询表达式: m => m.[targetName] == id，查询该业务对象下的所有现有附件
            ParameterExpression mParameterExpression = Expression.Parameter(tType, "m");
            MemberExpression leftExpression = Expression.Property(mParameterExpression, targetName);
            ConstantExpression rightExpression = Expression.Constant(id, propertyInfo.PropertyType);
            BinaryExpression expression = Expression.Equal(leftExpression, rightExpression);
            Expression<Func<T, bool>> searchExpression = Expression.Lambda<Func<T, bool>>(expression, mParameterExpression);
            // 查询当前业务对象已关联的所有附件
            List<T> allAdjunctInfos = await repository.FindAsync(searchExpression);
            List<Guid> allAdjunctIDs = [.. allAdjunctInfos.Select(m => m.UploadFileID)];
            // 对比新传入的fileIDs和已有的附件ID列表，得出需要新增的和需要删除的
            (addIDs, ICollection<Guid> removeIDs) = fileIDs.GetAddArrayAndRemoveArray(allAdjunctIDs);
            // 将需要删除的附件注册为删除状态
            List<T> removeAdjunctInfos = [.. allAdjunctInfos.Where(m => removeIDs.Contains(m.UploadFileID))];
            foreach (T adjunct in removeAdjunctInfos)
            {
                unitOfWork.RegisterDelete(adjunct);
            }
        }
        // 遍历需要新增的附件ID，创建附件领域对象并设置关联属性，注册为新增状态
        foreach (Guid adjunctID in addIDs)
        {
            T t = new()
            {
                UploadFileID = adjunctID
            };
            // 通过反射将业务对象ID设置到附件的关联属性上
            propertyInfo.SetValue(t, id);
            unitOfWork.RegisterAdd(t);
        }
    }
}
