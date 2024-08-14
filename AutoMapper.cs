using System.Reflection;

namespace TheNevix.AutoMapper
{
    public static class AutoMapper
    {
        public static class Mapper
        {
            /// <summary>
            /// Maps properties from a source object of type TSource to a destination object of type TDestination.
            /// Additional custom mapping logic can be applied after the automatic property mapping.
            /// </summary>
            /// <typeparam name="TSource">The type of the source object from which properties are mapped.</typeparam>
            /// <typeparam name="TDestination">The type of the destination object to which properties are mapped.</typeparam>
            /// <param name="source">The source object from which property values are read. This object must be of type TSource.</param>
            /// <param name="customMappingConfig">An optional action to perform custom mapping. 
            /// This action receives the source and destination objects as parameters, allowing for additional custom mapping logic to be applied after automatic mapping.
            /// If null, only automatic mapping based on property names and types will be performed.</param>
            /// <returns>An object of type TDestination with properties mapped from the source object. 
            /// If customMapping is provided, the returned object will also reflect any modifications made by the custom mapping logic.</returns>
            public static TDestination Map<TSource, TDestination>(
                TSource source,
                Action<TSource, TDestination> customMappingConfig = null) where TDestination : new()
            {
                var destination = new TDestination();
                AutoMapProperties(source, destination); // Your existing auto-mapping logic

                // Apply custom mapping if provided
                customMappingConfig?.Invoke(source, destination);

                return destination;
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
                        var sourceValue = sourceProp.GetValue(source);
                        if (sourceValue != null)
                        {
                            if (sourceProp.PropertyType.IsClass && !sourceProp.PropertyType.Namespace.StartsWith("System"))
                            {
                                // The property is a custom object, so we need to handle it recursively.
                                object destValue = destProp.GetValue(destination);
                                if (destValue == null)
                                {
                                    // Create a new instance of the property's type if it's null.
                                    destValue = Activator.CreateInstance(destProp.PropertyType);
                                    destProp.SetValue(destination, destValue);
                                }
                                // Recursively map the properties of the nested objects.
                                AutoMapProperties(sourceValue, destValue);
                            }
                            else
                            {
                                // The property is not a custom object, so we can directly set its value.
                                destProp.SetValue(destination, sourceValue);
                            }
                        }
                    }
                }
            }
        }
    }
}