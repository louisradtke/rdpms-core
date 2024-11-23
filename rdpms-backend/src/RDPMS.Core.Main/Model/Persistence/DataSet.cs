using Newtonsoft.Json.Linq;

namespace RDPMS.Core.Main.Model.Persistence;

public record DataSet()
{
    public required string Name { get; init; }
    public required List<DataFile> Files { get; set; } = new();
    public required List<Tag> AssignedTags { get; set; } = new();
    public required Dictionary<string, string> Metadata { get; set; } = new();
}
