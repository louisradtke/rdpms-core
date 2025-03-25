using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Model.Mappers;

public interface IImportMapper<out TDomain, TDTO>
{
    TDomain Import(TDTO foreign);
    IEnumerable<CheckSet<TDTO>> ImportChecks();
}

public interface IImportMapper<out TDomain, TDTO, in TArg>
{
    TDomain Import(TDTO foreign, TArg arg);
    IEnumerable<CheckSet<TDTO>> ImportChecks();
}
