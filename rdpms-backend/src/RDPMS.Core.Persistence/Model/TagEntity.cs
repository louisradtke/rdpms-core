namespace RDPMS.Core.Persistence.Model;

public record TagEntity(string Name)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = Name;
}
