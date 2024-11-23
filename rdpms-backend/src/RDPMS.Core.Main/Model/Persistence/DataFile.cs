namespace RDPMS.Core.Main.Model.Persistence;

public record DataFile()
{
    public required string Name { get; init; }
    public required Guid Id { get; init; }
}
