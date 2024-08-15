# TheNevix.AutoMapper

![NuGet](https://img.shields.io/nuget/v/TheNevix.AutoMapper) ![NuGet Downloads](https://img.shields.io/nuget/dt/TheNevix.AutoMapper)

A simple AutoMapper for your .NET applications with neat features.

## Features

- Quickly map two objects with no configuration
- Map two objects with a provided configuration
- Map specific properties from a source object to an existing destination object

## Installation

You can install the package via NuGet Package Manager, .NET CLI, or by adding it to your project file.

### NuGet Package Manager

```bash
Install-Package TheNevix.AutoMapper
```

## Configuration in program.cs

### Configuration without using project references

This means you will only use the mapper, for example, in an API project itself.

```csharp
builder.Services.AddSingleton(sp =>
{
    return MappingConfig.ConfigureMappings();
});
```

With this setup, you will be able to use the automapper with an option of providing a custom mapping configuration.

The ****'MappingConfig'**** in this case is a static class that you define.
The ****'ConfigureMappings'**** in this case is a static method where you can configure your custom mappings.

We will cover this in more detail in the [Configure Custom Mapping Configurations](#Configure-Custom-Mapping-Configurations) section.

### Configuration with using project references

```csharp
builder.Services.AddSingleton(sp =>
{
    return MappingConfig.ConfigureMappings();
});

builder.Services.AddTransient<IMappingService, MappingService>();
```

With this setup, you will be able to also use the mapping configurations in a class library.

> **Note:** The class library will also need to have the NuGet package installed.

## Configure Custom Mapping Configurations

If you want to simply map 2 objects, you won't need a configuration.

If you want to simply map two objects, you won't need a configuration.

However, if you want to map two objects where the destination property FullName is a combination of src.FirstName + " " + src.LastName, you can do the following:

```csharp
public static void ConfigurePersonNameMapping(this AutoMapperConfiguration config, Mapper mapper)
{
    config.CreateMap<Person, Person>("NameMapping", (src, dest) =>
    {
        dest.FullName = src.FirstName + " " + src.LastName;
    });
}
```

 this example, the TSource and TDestination are the same object. The object has a property called FullName. This configuration has a name, "NameMapping", meaning that you can configure multiple mapping configurations for the same two objects under different configuration names.

We discussed earlier how to use AddSingleton to return a class and a method to properly configure the mapping configurations. Here is an example of how you could do this:

```charp
public static class MappingConfig
{
    public static Mapper ConfigureMappings()
    {
        var config = new AutoMapperConfiguration();
        var mapper = new Mapper(config);

        config.ConfigurePlayerNameMapping(mapper);

        return mapper;
    }
}
```

You can name ****'MappingConfig'**** and ****'ConfigureMappings'**** however you see fit. Each mapping configuration needs to be added here for the mapper to find the custom configuration for certain objects.

## Usage

> **Note:** In the API project, you can simply inject Mapper. In a class library, you would need the IMapperService.

### Simple mapping without a configuration

```charp
public class PersonController : ControllerBase
{
    private readonly Mapper _mapper;

    public PersonController(Mapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet]
    public ActionResult<Person> GetPerson(PersonDTO personDto)
    {
        var mappedPerson = _mapper.Map<PersonDTO, Person>(personDto);

        return Ok(mappedPerson);
    }
}
```

In this case, we simply map a DTO object to a Domain model. Both models have a FirstName and LastName property so no additional mapping configurations are needed.

### Mapping with a custom configuration

```charp
public class PersonController : ControllerBase
{
    private readonly Mapper _mapper;

    public PersonController(Mapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet]
    public ActionResult<Person> GetPerson(Person person)
    {
        var mappedPerson = _mapper.Map<Person, Person>(person, "NameMapping");

        return Ok(mappedPerson);
    }
}
```

In this case, we are mapping Person to Person but we configured to also map the FullName property. You can add as much custom mapping in a config as you'd like.

### Mapping specific properties to an existing destination

```charp
public class PersonController : ControllerBase
{
    private readonly Mapper _mapper;

    public PersonController(Mapper mapper)
    {
        _mapper = mapper;
    }

    [HttpGet]
    public ActionResult<Person> UpdatePersonName(PersonDto item)
    {
        //Person from a database
        var dbPerson = new Person 
        {
            FirstName = "John",
            LastName = "Doe",
            Age = 35
        };

        _mapper.MapExistingDestination<PersonDto, Person>(item, dbPerson, "NameMapping");

        return Ok(dbPerson);
    }
}
```

In this case, we receive a PersonDto in our api and we only want to update the FirstName and LastName in dbPerson with the ones we received in our API. The PersonDto model also contains other properties which might be updated in other endpoints such as age for example.

If we were to use the normal .Map method, Age would be set to 0 because PersonDto has an Age property, but it might be 0 or null in this case since we didn't need it. This would result in updating dbPerson with a faulty value.

You could use .Map and specify something like ****dest.Age = dest.Age****. But this approach is redundant and messy, even though it would work.

Another common approach that people use is to skip mapping and do something like this:

```charp
dbPerson.FirstName = item.FirstName;
dbPerson.LastName = item.LastName;
```

This approach becomes cumbersome if you have 15 properties to update.

With the MapExistingDestination, you can just move all that to your mapping configurations.

## License

This project is licensed under the MIT License.
