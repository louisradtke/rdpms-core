namespace RDPMS.Core.Server.Model.Repositories.Infra;

public interface IIncludeConfiguration<T> where T : class
{
    IQueryable<T> ConfigureIncludes(IQueryable<T> query);
}