namespace TheNevix.AutoMapper
{
    public interface IMappingService
    {
        TDestination Map<TSource, TDestination>(TSource source, string configName = "Default") where TDestination : new();
        void MapExistingDestination<TSource, TDestination>(TSource source, TDestination destination, string configName);
    }
}
