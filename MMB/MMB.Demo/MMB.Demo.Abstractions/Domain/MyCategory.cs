
namespace MMB.Demo.Abstractions.Domain;

public class MyCategory : BaseDomain, IDomain, ITreeDomain, IIndexDomain
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [IndexGroup]
    [NotEdit]
    public Guid? ParentID { get; set; }
    [Required]
    [NotAdd, NotEdit, NotQuery]
    public int Index { get; set; }
}
