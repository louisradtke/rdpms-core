namespace RDPMS.Core.Server.Model.Repositories.Infra;

public class GenericLambdaIncludeConfiguration<T> : IIncludeConfiguration<T> where T : class
{
    private readonly Func<IQueryable<T>, IQueryable<T>> _includeFunc;

    private GenericLambdaIncludeConfiguration(
        Func<IQueryable<T>, IQueryable<T>>? includeFunc = null)
    {
        _includeFunc = includeFunc ?? (q => q);
    }

    public static GenericLambdaIncludeConfiguration<T> Create(
        Func<IQueryable<T>, IQueryable<T>> includeFunc)
    {
        return new GenericLambdaIncludeConfiguration<T>(includeFunc);
    }

    public IQueryable<T> ConfigureIncludes(IQueryable<T> query)
    {
        return _includeFunc(query);
    }
}
