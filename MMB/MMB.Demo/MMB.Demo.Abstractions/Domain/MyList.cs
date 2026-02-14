
namespace MMB.Demo.Abstractions.Domain;

public class MyList : BaseDomain, IDomain, IIndexDomain
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [NotAdd, NotEdit, NotQuery]
    public int Index { get; set; }
}