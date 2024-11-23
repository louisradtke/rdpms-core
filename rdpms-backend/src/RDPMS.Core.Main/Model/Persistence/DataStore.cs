namespace RDPMS.Core.Main.Model.Persistence;

public record DataStore()
{
    public required List<DataFile> DataFiles { get; init; } = new();
}