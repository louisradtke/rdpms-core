namespace RDPMS.Core.Persistence.Model;

public class Tag(string name)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = name;
}
