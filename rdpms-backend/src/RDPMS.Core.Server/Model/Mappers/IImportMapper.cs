using RDPMS.Core.Server.Util;

namespace RDPMS.Core.Server.Model.Mappers;

public interface IImportMapper<out TDomain, TDTO>
{
    TDomain Import(TDTO foreign);
    IEnumerable<CheckSet<TDTO>> ImportChecks();
}

public interface IImportMapper<out TDomain, TDTO, in TArg1>
{
    TDomain Import(TDTO foreign, TArg1 arg1);
    IEnumerable<CheckSet<TDTO>> ImportChecks();
}

public interface IImportMapper<out TDomain, TDTO, in TArg1, in TArg2>
{
    TDomain Import(TDTO foreign, TArg1 arg1, TArg2 arg2);
    IEnumerable<CheckSet<TDTO>> ImportChecks();
}
