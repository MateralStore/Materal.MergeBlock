namespace ProjectName.Core.Repository;

/// <summary>
/// ProjectName工作单元实现
/// </summary>
/// <param name="context"></param>
/// <param name="serviceProvider"></param>
public class ProjectNameUnitOfWorkImpl<T>(T context, IServiceProvider serviceProvider) : MergeBlockUnitOfWorkImpl<T>(context, serviceProvider), IProjectNameUnitOfWork
    where T : DbContext
{
}