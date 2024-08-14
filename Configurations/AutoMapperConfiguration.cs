namespace TheNevix.AutoMapper.Configurations
{
    public class AutoMapperConfiguration
    {
        private readonly Dictionary<string, List<MappingConfig>> _namedMappings = new();

        public AutoMapperConfiguration CreateMap<TSource, TDestination>(string configName = "Default", Action<TSource, TDestination> customMapping = null)
        {
            if (!_namedMappings.ContainsKey(configName))
            {
                _namedMappings[configName] = new List<MappingConfig>();
            }

            _namedMappings[configName].Add(new MappingConfig
            {
                CustomMapping = customMapping,
                UsageCount = 0
            });

            return this;
        }

        internal List<MappingConfig> GetMappingConfigs(string configName)
        {
            if (_namedMappings.TryGetValue(configName, out var configs))
            {
                return configs;
            }
            return null;
        }
    }

    internal class MappingConfig
    {
        public Delegate CustomMapping { get; set; }
        public int UsageCount { get; set; }
    }
}
