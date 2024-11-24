namespace RDPMS.Core.Persistence.Model;

public record Tag(string Name)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = Name;
}
