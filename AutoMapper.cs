using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using TheNevix.AutoMapper.Configurations;

namespace TheNevix
{
    public class Mapper
    {
        private readonly AutoMapperConfiguration _configuration;

        public Mapper(AutoMapperConfiguration configuration)
        {
            _configuration = configuration;
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
            var destination = AutoMapProperties<TSource, TDestination>(source);


            var configs = _configuration.GetMappingConfigs(configName);
            if (configs != null)
            {
                foreach (var config in configs)
                {
                    //Ensure that the config is for the correct types
                    if (config.CustomMapping is Action<TSource, TDestination> mappingAction)
                    {
                        mappingAction.Invoke(source, destination);
                    }
                }
            }

            return destination;
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
            var configs = _configuration.GetMappingConfigs(configName);
            if (configs != null)
            {
                foreach (var config in configs)
                {
                    //Ensure that the config is for the correct types
                    if (config.CustomMapping is Action<TSource, TDestination> mappingAction)
                    {
                        mappingAction.Invoke(source, destination);
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
                        Type destType = destProp.PropertyType;

                        // **Handle Nullable<T> types correctly**
                        bool isNullable = Nullable.GetUnderlyingType(destType) != null;
                        if (isNullable)
                        {
                            destType = Nullable.GetUnderlyingType(destType); // Extract underlying type (e.g., int from int?)
                        }

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
                                // **Handle nested objects recursively**
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
                            try
                            {
                                object convertedValue = (destType == sourceProp.PropertyType)
                                    ? sourceValue
                                    : Convert.ChangeType(sourceValue, destType);

                                destProp.SetValue(destination, convertedValue);
                            }
                            catch (Exception)
                            {
                                // Ignore conversion errors to prevent runtime crashes
                            }
                        }
                    }
                    else
                    {
                        if (Nullable.GetUnderlyingType(destProp.PropertyType) != null || !destProp.PropertyType.IsValueType)
                        {
                            destProp.SetValue(destination, null);
                        }
                    }
                }
            }
        }

        private static readonly ConcurrentDictionary<(Type Source, Type Dest), Delegate> _mapCache = new();

        public static TDestination AutoMapProperties<TSource, TDestination>(TSource source)
            where TDestination : new()
        {
            if (source == null) return default;

            var key = (typeof(TSource), typeof(TDestination));
            if (!_mapCache.TryGetValue(key, out var cachedDelegate))
            {
                cachedDelegate = BuildMapFunc<TSource, TDestination>();
                _mapCache[key] = cachedDelegate;
            }

            var mapFunc = (Func<TSource, TDestination>)cachedDelegate;
            return mapFunc(source);
        }

        private static Func<TSource, TDestination> BuildMapFunc<TSource, TDestination>()
            where TDestination : new()
        {
            var sourceType = typeof(TSource);
            var destType = typeof(TDestination);

            var sourceParam = Expression.Parameter(sourceType, "src");
            var bindings = new List<MemberBinding>();

            foreach (var destProp in destType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!destProp.CanWrite) continue;

                var sourceProp = sourceType.GetProperty(destProp.Name, BindingFlags.Public | BindingFlags.Instance);
                if (sourceProp == null || !sourceProp.CanRead) continue;

                if (!destProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType)) continue;

                var sourceValue = Expression.Property(sourceParam, sourceProp);
                var binding = Expression.Bind(destProp, sourceValue);
                bindings.Add(binding);
            }

            var body = Expression.MemberInit(Expression.New(destType), bindings);
            var lambda = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam);
            return lambda.Compile();
        }
    }
}