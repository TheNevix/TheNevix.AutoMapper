using System.Reflection;
using TheNevix.AutoMapper.Configurations;

namespace TheNevix.AutoMapper
{
    public class AutoMapper
    {
        public class Mapper
        {

            private readonly AutoMapperConfiguration _configuration;

            public Mapper(AutoMapperConfiguration configuration)
            {
                _configuration = configuration;
            }

            public TDestination Map<TSource, TDestination>(TSource source, string configName = "Default") where TDestination : new()
            {
                var destination = new TDestination();
                AutoMapProperties(source, destination);

                var configs = _configuration.GetMappingConfigs(configName);
                if (configs != null)
                {
                    foreach (var config in configs)
                    {
                        // Ensure that the config is for the correct types
                        if (config.CustomMapping is Action<TSource, TDestination> mappingAction)
                        {
                            mappingAction.Invoke(source, destination);
                            config.UsageCount++;
                        }
                    }
                }

                return destination;
            }

            public void MapExistingDestination<TSource, TDestination>(TSource source, TDestination destination, string configName)
            {
                var configs = _configuration.GetMappingConfigs(configName);
                if (configs != null)
                {
                    foreach (var config in configs)
                    {
                        // Ensure that the config is for the correct types
                        if (config.CustomMapping is Action<TSource, TDestination> mappingAction)
                        {
                            mappingAction.Invoke(source, destination);
                            config.UsageCount++;
                        }
                    }
                }
            }


            private static void AutoMapProperties<TSource, TDestination>(TSource source, TDestination destination)
            {
                if (source == null || destination == null) return;

                var sourceProps = source.GetType().GetProperties();
                var destProps = destination.GetType().GetProperties();

                foreach (var sourceProp in sourceProps)
                {
                    var destProp = destProps.FirstOrDefault(p => p.Name == sourceProp.Name && p.CanWrite);
                    if (destProp != null)
                    {
                        var sourceValue = sourceProp.GetValue(source, null); // This should handle regular properties

                        if (sourceValue != null)
                        {
                            if (sourceProp.PropertyType.IsClass && sourceProp.PropertyType != typeof(string))
                            {
                                if (typeof(System.Collections.IEnumerable).IsAssignableFrom(sourceProp.PropertyType))
                                {
                                    // Handle collections (e.g., List<string>)
                                    var elementType = sourceProp.PropertyType.GetGenericArguments().FirstOrDefault();
                                    if (elementType != null)
                                    {
                                        var genericListType = typeof(List<>).MakeGenericType(elementType);
                                        var destList = Activator.CreateInstance(genericListType);

                                        foreach (var item in (System.Collections.IEnumerable)sourceValue)
                                        {
                                            ((System.Collections.IList)destList).Add(item);
                                        }

                                        destProp.SetValue(destination, destList);
                                    }
                                }
                                else
                                {
                                    // Handle nested custom objects recursively
                                    object destValue = destProp.GetValue(destination);
                                    if (destValue == null)
                                    {
                                        destValue = Activator.CreateInstance(destProp.PropertyType);
                                        destProp.SetValue(destination, destValue);
                                    }
                                    AutoMapProperties(sourceValue, destValue);
                                }
                            }
                            else
                            {
                                // Handle primitive types and strings
                                destProp.SetValue(destination, sourceValue);
                            }
                        }
                        else
                        {
                            // Handle null values
                            destProp.SetValue(destination, null);
                        }
                    }
                }
            }
        }
    }
}