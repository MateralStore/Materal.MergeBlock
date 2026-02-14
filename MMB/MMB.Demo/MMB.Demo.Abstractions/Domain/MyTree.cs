
namespace MMB.Demo.Abstractions.Domain;

public class MyTree : BaseDomain, IDomain, ITreeDomain
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [NotEdit]
    public Guid? ParentID { get; set; }
}
