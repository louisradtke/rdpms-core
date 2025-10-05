using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Persistence;

public static class SlugUtil
{
    public static bool IsQualifiedForSlug(Type type)
    {
        return type == typeof(Project) ||
               type == typeof(DataCollectionEntity) ||
               type == typeof(DataSet) ||
               type == typeof(DataStore);
    }

    public static Type MapSlugToType(SlugType slugType)
    {
        return slugType switch
        {
            SlugType.Project => typeof(Project),
            SlugType.DataCollection => typeof(DataCollectionEntity),
            SlugType.DataSet => typeof(DataSet),
            SlugType.DataStore => typeof(DataStore),
            _ => throw new ArgumentOutOfRangeException(nameof(slugType), slugType, null)
        };
    }

    public static SlugType MapTypeToSlug<T>() => MapTypeToSlug(typeof(T));

    public static SlugType MapTypeToSlug(Type type)
    {
        if (type == typeof(Project))
            return SlugType.Project;
        if (type == typeof(DataCollectionEntity))
            return SlugType.DataCollection;
        if (type == typeof(DataSet))
            return SlugType.DataSet;
        if (type == typeof(DataStore))
            return SlugType.DataStore;
        throw new ArgumentOutOfRangeException(nameof(type), type, null);
    }
}