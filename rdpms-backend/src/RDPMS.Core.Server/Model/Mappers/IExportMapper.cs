namespace RDPMS.Core.Server.Model.Mappers;

public interface IExportMapper<in TDomain, out TDTO>
{
    TDTO Export(TDomain domain);
}