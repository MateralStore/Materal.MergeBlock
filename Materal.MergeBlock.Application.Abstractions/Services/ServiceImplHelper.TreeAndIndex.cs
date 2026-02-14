using Materal.MergeBlock.Repository.Abstractions.Repositories;

namespace Materal.MergeBlock.Application.Abstractions.Services;

public static partial class ServiceImplHelper
{
    /// <summary>
    /// 移动树形节点并排序
    /// </summary>
    /// <typeparam name="TRepository"></typeparam>
    /// <typeparam name="TDomain"></typeparam>
    /// <param name="model"></param>
    /// <param name="repository"></param>
    /// <param name="groupProperties"></param>
    /// <returns></returns>
    /// <exception cref="MergeBlockModuleException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<List<TDomain>> MoveAsync<TRepository, TDomain>(MoveTreeNodeAndIndexModel model, TRepository repository, params string[] groupProperties)
        where TRepository : IRepository<TDomain, Guid>, ITreeRepository<TDomain>
        where TDomain : class, IIndexDomain, ITreeDomain, new()
    {
        // 源对象和目标对象不能是同一个
        if (model.SourceID == model.TargetID) throw new MergeBlockModuleException("不能以自己为排序对象");

        // 获取源对象
        TDomain? sourceDomain = await repository.FirstOrDefaultAsync(model.SourceID) ?? throw new MergeBlockModuleException("源对象不存在");

        // 确定目标父级（优先级：Target的父级 > 传入的父级 > Source的父级）
        Guid? originalParentID = sourceDomain.ParentID;
        Guid? targetParentID;
        if (model.TargetID.HasValue)
        {
            // 优先使用 TargetID 的父级
            TDomain? targetDomain = await repository.FirstOrDefaultAsync(model.TargetID.Value);
            if (targetDomain is not null)
            {
                targetParentID = targetDomain.ParentID;
            }
            else
            {
                targetParentID = model.ParentID ?? originalParentID;
            }
        }
        else
        {
            // 没有 TargetID 时，使用传入的 ParentID 或保持原父级
            targetParentID = model.ParentID ?? originalParentID;
        }

        // 验证循环引用和分组属性
        if (targetParentID.HasValue && targetParentID != originalParentID)
        {
            // 验证不能将父级设置为自己的子节点
            if (IsChildNode<TRepository, TDomain>(repository, model.SourceID, targetParentID.Value)) throw new MergeBlockModuleException("不能将父级设置为自己的子节点");
            // 验证分组属性
            if (groupProperties.Length > 0)
            {
                TDomain? targetParentDomain = await repository.FirstOrDefaultAsync(targetParentID.Value);
                if (targetParentDomain != null)
                {
                    ValidateGroupPropertiesForTreeIndex(sourceDomain, targetParentDomain, groupProperties);
                }
            }
        }

        // 执行移动
        List<TDomain> editDomains;
        if (originalParentID == targetParentID)
        {
            // 同一父级下移动
            List<TDomain> siblings = await repository.FindAsync(m => m.ParentID == originalParentID, m => m.Index);
            editDomains = MoveIndex(model, siblings);
        }
        else
        {
            // 移动到不同父级
            editDomains = [];

            // 1. 从原父级移除
            sourceDomain.ParentID = targetParentID;
            editDomains.Add(sourceDomain);

            // 2. 重新排列原父级的子节点
            List<TDomain> originalSiblings = await repository.FindAsync(m => m.ParentID == originalParentID, m => m.Index);
            originalSiblings.RemoveAll(m => m.ID == model.SourceID);
            for (int i = 0; i < originalSiblings.Count; i++)
            {
                if (originalSiblings[i].Index == i) continue;
                originalSiblings[i].Index = i;
                editDomains.Add(originalSiblings[i]);
            }

            // 3. 插入到新父级并重新排列
            List<TDomain> targetSiblings = await repository.FindAsync(m => m.ParentID == targetParentID, m => m.Index);
            // 排除源对象（EF Core change tracking 可能导致修改 ParentID 后的源对象被查询出来）
            targetSiblings.RemoveAll(m => m.ID == model.SourceID);

            // 确定插入位置
            int insertIndex = targetSiblings.Count;
            if (model.TargetID.HasValue)
            {
                int targetIndex = targetSiblings.FindIndex(m => m.ID == model.TargetID.Value);
                if (targetIndex >= 0)
                {
                    insertIndex = model.Before ? targetIndex : targetIndex + 1;
                }
            }

            // 插入并重新排列
            targetSiblings.Insert(insertIndex, sourceDomain);
            for (int i = 0; i < targetSiblings.Count; i++)
            {
                if (targetSiblings[i].Index == i) continue;
                targetSiblings[i].Index = i;
                if (!editDomains.Contains(targetSiblings[i]))
                {
                    editDomains.Add(targetSiblings[i]);
                }
            }
        }

        return editDomains;
    }

    /// <summary>
    /// 更改位序（在同一父级内）
    /// </summary>
    /// <typeparam name="TDomain"></typeparam>
    /// <param name="model"></param>
    /// <param name="domains"></param>
    /// <returns></returns>
    /// <exception cref="MergeBlockModuleException"></exception>
    private static List<TDomain> MoveIndex<TDomain>(MoveTreeNodeAndIndexModel model, List<TDomain> domains)
        where TDomain : class, IIndexDomain, ITreeDomain
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
            else if (model.TargetID.HasValue && domains[i].ID == model.TargetID.Value)
            {
                targetIndex = i;
            }
        }

        if (sourceIndex == -1) throw new MergeBlockModuleException("源对象不存在");

        // 移除源对象
        TDomain sourceDomain = domains[sourceIndex];
        domains.RemoveAt(sourceIndex);

        int insertIndex;
        if (targetIndex == -1)
        {
            // 没有指定目标或目标不在列表中，直接放到末尾
            insertIndex = domains.Count;
        }
        else
        {
            // 移除后目标位置可能需要调整
            if (sourceIndex < targetIndex)
            {
                targetIndex--;
            }

            // 计算目标插入位置：Before=true放到目标之前，Before=false放到目标之后
            insertIndex = model.Before ? targetIndex : targetIndex + 1;
        }

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

    /// <summary>
    /// 验证分组属性（用于树形位序移动）
    /// </summary>
    /// <typeparam name="TDomain"></typeparam>
    /// <param name="sourceDomain">源对象</param>
    /// <param name="targetParentDomain">目标父级对象</param>
    /// <param name="groupProperties">分组属性名称</param>
    /// <exception cref="MergeBlockModuleException"></exception>
    /// <exception cref="ArgumentException"></exception>
    private static void ValidateGroupPropertiesForTreeIndex<TDomain>(TDomain sourceDomain, TDomain targetParentDomain, params string[] groupProperties)
        where TDomain : class, ITreeDomain, new()
    {
        if (groupProperties.Length <= 0) return;
        Type domainType = typeof(TDomain);
        foreach (string groupProperty in groupProperties)
        {
            // ParentID 是树形结构属性，跨父级移动时天然不同，不参与分组验证
            if (groupProperty == nameof(ITreeDomain.ParentID)) continue;
            PropertyInfo? propertyInfo = domainType.GetProperty(groupProperty) ?? throw new ArgumentException($"属性名称{groupProperty}不存在");
            object? sourceValue = propertyInfo.GetValue(sourceDomain);
            object? targetValue = propertyInfo.GetValue(targetParentDomain);
            // 校验两个对象的分组属性值是否一致
            if (sourceValue == null || targetValue == null)
            {
                if (sourceValue != null || targetValue != null) throw new MergeBlockModuleException("不是同一组数据不能更改父级");
            }
            else
            {
                if (!sourceValue.Equals(targetValue)) throw new MergeBlockModuleException("不是同一组数据不能更改父级");
            }
        }
    }
}