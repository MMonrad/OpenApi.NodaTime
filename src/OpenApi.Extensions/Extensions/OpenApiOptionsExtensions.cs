using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using OpenApi.Extensions.Transformers.Documents;
using OpenApi.Extensions.Transformers.Operations;
using OpenApi.Extensions.Transformers.Schemas;

namespace OpenApi.Extensions.Extensions;

/// <summary>
/// Useful extension methods for <see cref="OpenApiOptions" />.
/// </summary>
public static class OpenApiOptionsExtensions
{
    /// <summary>
    /// Adds a document transformer to set a global description for the OpenAPI document.
    /// </summary>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="description">The description to apply to the OpenAPI document.</param>
    /// <returns>The updated <see cref="OpenApiOptions"/> instance.</returns>
    public static OpenApiOptions AddDescription(this OpenApiOptions options, string description)
    {
        options.AddDocumentTransformer(new DescriptionDocumentTransform(description));
        return options;
    }

    /// <summary>
    /// Adds the given status code and description to all operations
    /// </summary>
    /// <param name="options"></param>
    /// <param name="statusCode"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static OpenApiOptions AddResponseType(this OpenApiOptions options, HttpStatusCode statusCode, string? description = null)
    {
        options.AddOperationTransformer(new StatusCodeOperationTransform(statusCode, description));

        return options;
    }

    /// <summary>
    /// Adds the given status code and description to all operations
    /// </summary>
    /// <param name="options"></param>
    /// <param name="statusCode"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static OpenApiOptions AddResponseType<T>(this OpenApiOptions options, HttpStatusCode statusCode, string? description = null)
    {
        options.AddOperationTransformer(new StatusCodeOperationTransform<T>(statusCode, description));

        return options;
    }

    /// <summary>
    /// Adds the given security scheme to the current <see cref="OpenApiOptions" /> instance.
    /// </summary>
    /// <param name="options"><see cref="OpenApiOptions" />.</param>
    /// <param name="schemeName">The name of the scheme.</param>
    /// <param name="scheme">The <see cref="OpenApiSecurityScheme" />.</param>
    public static OpenApiOptions AddSecurityScheme(this OpenApiOptions options, string schemeName, OpenApiSecurityScheme scheme)
    {
        options.AddSecurityScheme(schemeName, (_, _, _) => Task.FromResult(scheme));
        return options;
    }

    /// <summary>
    /// Adds the given security scheme to the current <see cref="OpenApiOptions" /> instance.
    /// </summary>
    /// <param name="options"><see cref="OpenApiOptions" />.</param>
    /// <param name="schemeName">The name of the scheme.</param>
    /// <param name="scheme">An action to configure the <see cref="OpenApiSecurityScheme" />.</param>
    public static OpenApiOptions AddSecurityScheme(this OpenApiOptions options, string schemeName, Action<OpenApiSecurityScheme> scheme)
    {
        return options.AddSecurityScheme(schemeName,
            (s, _, _) =>
            {
                scheme(s);
                return Task.CompletedTask;
            });
    }

    /// <summary>
    /// Adds the given security scheme to the current <see cref="OpenApiOptions" /> instance.
    /// </summary>
    /// <param name="options"><see cref="OpenApiOptions" />.</param>
    /// <param name="schemeName">The name of the scheme.</param>
    /// <param name="factory">A factory to provide the <see cref="OpenApiSecurityScheme" />.</param>
    public static OpenApiOptions AddSecurityScheme(this OpenApiOptions options, string schemeName, Action<OpenApiSecurityScheme, IServiceProvider> factory)
    {
        return options.AddSecurityScheme(schemeName,
            (scheme, provider, _) =>
            {
                factory(scheme, provider);
                return Task.CompletedTask;
            });
    }

    /// <summary>
    /// Adds the given security scheme to the current <see cref="OpenApiOptions" /> instance.
    /// </summary>
    /// <param name="options"><see cref="OpenApiOptions" />.</param>
    /// <param name="schemeName">The name of the scheme.</param>
    /// <param name="asyncFactory">An async factory to provide the <see cref="OpenApiSecurityScheme" />.</param>
    public static OpenApiOptions AddSecurityScheme(this OpenApiOptions options,
        string schemeName,
        Func<OpenApiSecurityScheme, IServiceProvider, CancellationToken, Task> asyncFactory)
    {
        options.AddDocumentTransformer(async (document, context, cancellationToken) =>
        {
            var securityScheme = new OpenApiSecurityScheme();
            await asyncFactory(securityScheme, context.ApplicationServices, cancellationToken);
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes.Add(schemeName, securityScheme);
        });

        options.AddOperationTransformer(async (operation, context, _) =>
        {
            var hasAuthorization = await context.HasAuthorizationAsync();
            if (hasAuthorization)
            {
                var referenceScheme = new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                        { Id = schemeName, Type = ReferenceType.SecurityScheme }
                };
                operation.Security ??= [];
                operation.Security.Add(new OpenApiSecurityRequirement
                {
                    [referenceScheme] = []
                });
            }
        });
        return options;
    }

    /// <summary>
    /// Provide specific server information to include in the generated Swagger document
    /// </summary>
    /// <param name="options"></param>
    /// <param name="server">A url of the server</param>
    /// <param name="description">A description of the server</param>
    public static OpenApiOptions AddServer(this OpenApiOptions options, string server, string? description = null)
    {
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Servers.Add(new OpenApiServer
            {
                Url = server,
                Description = description
            });

            return Task.CompletedTask;
        });

        return options;
    }

    /// <summary>
    /// Provide specific server information to include in the generated Swagger document
    /// </summary>
    /// <param name="options"></param>
    /// <param name="server">A url of the server</param>
    /// <param name="description">A description of the server</param>
    public static OpenApiOptions AddServer(this OpenApiOptions options, Uri server, string? description = null)
    {
        return options.AddServer(server.ToString(), description);
    }

    /// <summary>
    /// Adds a schema transformer to configure OpenAPI schema for a specific type.
    /// </summary>
    /// <typeparam name="TConcrete">The type to be transformed in the OpenAPI schema.</typeparam>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serializer options for formatting the schema example.</param>
    /// <returns>The updated <see cref="OpenApiOptions"/> instance.</returns>
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
            schema.Example = new OpenApiString(FormatJson(Activator.CreateInstance<TConcrete>(), jsonSerializerOptions));
            schema.Annotations.Clear();

            return Task.CompletedTask;
        });

        return options;
    }

    /// <summary>
    /// Adds a schema transformer to configure OpenAPI schema for a specific type, mapping it to another type in the schema.
    /// </summary>
    /// <typeparam name="TConcrete">The type to be transformed in the OpenAPI schema.</typeparam>
    /// <typeparam name="TType">The type name to use in the schema.</typeparam>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serializer options for formatting the schema example.</param>
    /// <returns>The updated <see cref="OpenApiOptions"/> instance.</returns>
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
            schema.Example = new OpenApiString(FormatJson(Activator.CreateInstance<TConcrete>(), jsonSerializerOptions));
            schema.Annotations.Clear();

            return Task.CompletedTask;
        });

        return options;
    }

    /// <summary>
    /// Adds a schema transformer to configure OpenAPI schema for a specific type with a custom format.
    /// </summary>
    /// <typeparam name="TConcrete">The type to be transformed in the OpenAPI schema.</typeparam>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="format">The custom format to apply to the schema.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serializer options for formatting the schema example.</param>
    /// <returns>The updated <see cref="OpenApiOptions"/> instance.</returns>
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
            schema.Example = new OpenApiString(FormatJson(Activator.CreateInstance<TConcrete>(), jsonSerializerOptions));
            schema.Annotations.Clear();

            return Task.CompletedTask;
        });

        return options;
    }

    /// <summary>
    /// Adds a schema transformer to configure OpenAPI schema for a specific type with a custom format, mapping it to another type in the schema.
    /// </summary>
    /// <typeparam name="TConcrete">The type to be transformed in the OpenAPI schema.</typeparam>
    /// <typeparam name="TType">The type name to use in the schema.</typeparam>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="format">The custom format to apply to the schema.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serializer options for formatting the schema example.</param>
    /// <returns>The updated <see cref="OpenApiOptions"/> instance.</returns>
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
            schema.Example = new OpenApiString(FormatJson(Activator.CreateInstance<TConcrete>(), jsonSerializerOptions));
            schema.Annotations.Clear();

            return Task.CompletedTask;
        });

        return options;
    }

    /// <summary>
    /// Adds a schema transformer to configure OpenAPI schema for a specific type with a custom format and example.
    /// </summary>
    /// <typeparam name="TConcrete">The type to be transformed in the OpenAPI schema.</typeparam>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="format">The custom format to apply to the schema.</param>
    /// <param name="example">An example value for the type in the schema.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serializer options for formatting the schema example.</param>
    /// <returns>The updated <see cref="OpenApiOptions"/> instance.</returns>
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
            schema.Example = new OpenApiString(FormatJson(example, jsonSerializerOptions));
            schema.Annotations.Clear();

            return Task.CompletedTask;
        });

        return options;
    }

    /// <summary>
    /// Adds a schema transformer to configure OpenAPI schema for a specific type with a custom format, mapping it to another type in the schema and providing an example.
    /// </summary>
    /// <typeparam name="TConcrete">The type to be transformed in the OpenAPI schema.</typeparam>
    /// <typeparam name="TType">The type name to use in the schema.</typeparam>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="format">The custom format to apply to the schema.</param>
    /// <param name="example">An example value for the type in the schema.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serializer options for formatting the schema example.</param>
    /// <returns>The updated <see cref="OpenApiOptions"/> instance.</returns>
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
            schema.Example = new OpenApiString(FormatJson(example, jsonSerializerOptions));

            return Task.CompletedTask;
        });

        return options;
    }

    /// <summary>
    /// Adds a schema transformer to configure OpenAPI schema for a specific type, mapping it to another type in the schema and providing additional metadata.
    /// </summary>
    /// <typeparam name="TConcrete">The type to be transformed in the OpenAPI schema.</typeparam>
    /// <typeparam name="TType">The type name to use in the schema.</typeparam>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="format">The custom format to apply to the schema.</param>
    /// <param name="example">An example value for the type in the schema.</param>
    /// <param name="description">An optional description of the schema.</param>
    /// <param name="properties">Optional additional properties to include in the schema.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serializer options for formatting the schema example.</param>
    /// <returns>The updated <see cref="OpenApiOptions"/> instance.</returns>
    public static OpenApiOptions AddType<TConcrete, TType>(this OpenApiOptions options,
        string format,
        TConcrete example,
        string? description = null,
        IDictionary<string, OpenApiSchema>? properties = null,
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
            schema.Example = new OpenApiString(FormatJson(example, jsonSerializerOptions));
            schema.Description = description;
            schema.Properties = properties ?? new Dictionary<string, OpenApiSchema>();
            schema.Annotations.Clear();

            return Task.CompletedTask;
        });

        return options;
    }

    /// <summary>
    /// Adds a schema transformer to configure OpenAPI schema for a specific type, mapping it to another type in the schema.
    /// </summary>
    /// <typeparam name="TConcrete">The type to be transformed in the OpenAPI schema.</typeparam>
    /// <typeparam name="TType">The type name to use in the schema.</typeparam>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="example">An example value for the type in the schema.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serializer options for formatting the schema example.</param>
    /// <returns>The updated <see cref="OpenApiOptions"/> instance.</returns>
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
            schema.Example = new OpenApiString(FormatJson(example, jsonSerializerOptions));
            schema.Annotations.Clear();

            return Task.CompletedTask;
        });

        return options;
    }

    /// <summary>
    /// Adds a schema transformer to configure OpenAPI schema for a specific type, mapping it to another type in the schema, and including additional properties.
    /// </summary>
    /// <typeparam name="TConcrete">The type to be transformed in the OpenAPI schema.</typeparam>
    /// <typeparam name="TType">The type name to use in the schema.</typeparam>
    /// <param name="options">The OpenAPI options to configure.</param>
    /// <param name="example">An example value for the type in the schema.</param>
    /// <param name="properties">Optional additional properties to include in the schema.</param>
    /// <param name="jsonSerializerOptions">Optional JSON serializer options for formatting the schema example.</param>
    /// <returns>The updated <see cref="OpenApiOptions"/> instance.</returns>
    public static OpenApiOptions AddType<TConcrete, TType>(this OpenApiOptions options,
        TConcrete example,
        IDictionary<string, OpenApiSchema>? properties = null,
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
            schema.Example = new OpenApiString(FormatJson(example, jsonSerializerOptions));
            schema.Properties = properties ?? new Dictionary<string, OpenApiSchema>();
            schema.Annotations.Clear();

            return Task.CompletedTask;
        });

        return options;
    }

    /// <summary>
    /// Loads the assembly and adds XML documentation to the type.
    /// </summary>
    /// <typeparam name="T">The assembly type to add XML documentation for.</typeparam>
    /// <returns>The updated <see cref="OpenApiOptions"/> instance.</returns>
    public static OpenApiOptions AddXmlComments<T>(this OpenApiOptions options)
        where T : class, new()
    {
        options.AddSchemaTransformer<XmlSchemaTransformer<T>>();
        return options;
    }

    /// <summary>
    /// Loads the assembly and adds XML documentation to the type.
    /// </summary>
    /// <typeparam name="T">The assembly type to add XML documentation for.</typeparam>
    /// <returns>The updated <see cref="OpenApiOptions"/> instance.</returns>
    public static OpenApiOptions AddXmlComments<T>(this OpenApiOptions options,
        Func<JsonTypeInfo, JsonPropertyInfo?, string?, ValueTask<string?>> onChangeDescription)
        where T : class, new()
    {
        options.AddSchemaTransformer(new XmlSchemaTransformer<T>(onChangeDescription));
        return options;
    }

    private static string FormatJson<T>(T obj, JsonSerializerOptions? jsonSerializerOptions)
    {
        var formatToJson = JsonSerializer.Serialize(obj, jsonSerializerOptions);
        if (formatToJson.StartsWith('"') && formatToJson.EndsWith('"'))
        {
            formatToJson = formatToJson.Substring(1, formatToJson.Length - 2);
        }

        return formatToJson;
    }
}
