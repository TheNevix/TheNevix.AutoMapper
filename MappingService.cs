using static TheNevix.AutoMapper.AutoMapper;

namespace TheNevix.AutoMapper
{
    public class MappingService : IMappingService
    {
        private readonly Mapper _mapper;

        public MappingService(Mapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// Auto maps the properties of <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>. Optional to provide a custom configuration.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object to map from</typeparam>
        /// <typeparam name="TDestination">The type of the destination object to map to</typeparam>
        /// <param name="source">The source object to map the properties from</param>
        /// <param name="configName">OPTIONAL: The name of the mapping config to use. If not provided, it will just map the properties with no custom mapping </param>
        /// <returns>The destination object of type <typeparamref name="TDestination"/>.</returns>
        public TDestination Map<TSource, TDestination>(TSource source, string configName = "Default") where TDestination : new()
        {
            return _mapper.Map<TSource, TDestination>(source, configName);
        }

        /// <summary>
        /// Maps specific properties of <typeparamref name="TSource"/> to <typeparamref name="TDestination"/> from the provided configurqtion name.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object to map from</typeparam>
        /// <typeparam name="TDestination">The type of the destination object to map to</typeparam>
        /// <param name="source">The source object to map the properties from</param>
        /// <param name="destination">The destination object to map the properties to</param>
        /// <param name="configName">The name of the configuration to use</param>
        public void MapExistingDestination<TSource, TDestination>(TSource source, TDestination destination, string configName)
        {
            _mapper.MapExistingDestination<TSource, TDestination>(source, destination, configName);
        }
    }
}
