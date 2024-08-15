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


        public TDestination Map<TSource, TDestination>(TSource source, string configName = "Default") where TDestination : new()
        {
            return _mapper.Map<TSource, TDestination>(source, configName);
        }


        public void MapExistingDestination<TSource, TDestination>(TSource source, TDestination destination, string configName)
        {
            _mapper.MapExistingDestination<TSource, TDestination>(source, destination, configName);
        }
    }
}
