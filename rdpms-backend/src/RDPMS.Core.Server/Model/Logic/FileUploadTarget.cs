namespace RDPMS.Core.Server.Model.Logic;

public class FileUploadTarget(Uri uri)
{
    public Uri Uri { get; set; } = uri;
    public Guid FileId { get; set; } = Guid.Empty;
}