using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;

namespace OpenApi.Extensions.Extensions;

public static class OpenApiOptionsExtensions
{
    public static OpenApiOptions AddType<TConcrete>(this OpenApiOptions options, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        jsonSerializerOptions ??= new JsonSerializerOptions();

        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type != typeof(TConcrete))
            {
                return Task.CompletedTask;
            }

            schema.Type = typeof(TConcrete).Name;
            schema.Example = new OpenApiString(JsonSerializer.Serialize(Activator.CreateInstance<TConcrete>(), jsonSerializerOptions));

            return Task.CompletedTask;
        });

        return options;
    }

    public static OpenApiOptions AddType<TConcrete, TType>(this OpenApiOptions options, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        jsonSerializerOptions ??= new JsonSerializerOptions();

        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type != typeof(TConcrete))
            {
                return Task.CompletedTask;
            }

            schema.Type = typeof(TType).Name;
            schema.Example = new OpenApiString(JsonSerializer.Serialize(Activator.CreateInstance<TConcrete>(), jsonSerializerOptions));

            return Task.CompletedTask;
        });

        return options;
    }

    public static OpenApiOptions AddType<TConcrete>(this OpenApiOptions options, string format, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        jsonSerializerOptions ??= new JsonSerializerOptions();

        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type != typeof(TConcrete))
            {
                return Task.CompletedTask;
            }

            schema.Type = typeof(TConcrete).Name;
            schema.Format = format;
            schema.Description = JsonSerializer.Serialize(Activator.CreateInstance<TConcrete>(), jsonSerializerOptions);

            return Task.CompletedTask;
        });

        return options;
    }

    public static OpenApiOptions AddType<TConcrete, TType>(this OpenApiOptions options, string format, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        jsonSerializerOptions ??= new JsonSerializerOptions();

        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type != typeof(TConcrete))
            {
                return Task.CompletedTask;
            }

            schema.Type = typeof(TType).Name;
            schema.Format = format;
            schema.Example = new OpenApiString(JsonSerializer.Serialize(Activator.CreateInstance<TConcrete>(), jsonSerializerOptions));

            return Task.CompletedTask;
        });

        return options;
    }

    public static OpenApiOptions AddType<TConcrete>(this OpenApiOptions options,
        string format,
        TConcrete example,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        jsonSerializerOptions ??= new JsonSerializerOptions();

        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type != typeof(TConcrete))
            {
                return Task.CompletedTask;
            }

            schema.Type = typeof(TConcrete).Name;
            schema.Format = format;
            schema.Example = new OpenApiString(JsonSerializer.Serialize(example, jsonSerializerOptions));

            return Task.CompletedTask;
        });

        return options;
    }

    public static OpenApiOptions AddType<TConcrete, TType>(this OpenApiOptions options,
        string format,
        TConcrete example,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        jsonSerializerOptions ??= new JsonSerializerOptions();

        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type != typeof(TConcrete))
            {
                return Task.CompletedTask;
            }

            schema.Type = typeof(TType).Name;
            schema.Format = format;
            schema.Example = new OpenApiString(JsonSerializer.Serialize(example, jsonSerializerOptions));

            return Task.CompletedTask;
        });

        return options;
    }

    public static OpenApiOptions AddType<TConcrete, TType>(this OpenApiOptions options,
        TConcrete example,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        jsonSerializerOptions ??= new JsonSerializerOptions();

        options.AddSchemaTransformer((schema, context, _) =>
        {
            if (context.JsonTypeInfo.Type != typeof(TConcrete))
            {
                return Task.CompletedTask;
            }

            schema.Type = typeof(TType).Name;
            schema.Example = new OpenApiString(JsonSerializer.Serialize(example, jsonSerializerOptions));

            return Task.CompletedTask;
        });

        return options;
    }
}
