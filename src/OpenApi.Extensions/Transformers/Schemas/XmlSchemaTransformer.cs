using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace OpenApi.Extensions.Transformers.Schemas;

public class XmlSchemaTransformer<T> : IOpenApiSchemaTransformer
    where T : class, new()
{
    private readonly ConcurrentDictionary<string, string?> _descriptions = [];
    private readonly Func<JsonTypeInfo, JsonPropertyInfo?, string?, ValueTask<string?>>? _getDescription;
    private readonly Lazy<XPathNavigator?> _navigator;

    public XmlSchemaTransformer(Func<JsonTypeInfo, JsonPropertyInfo?, string?, ValueTask<string?>>? getDescription = null)
    {
        _getDescription = getDescription;
        _navigator = new Lazy<XPathNavigator?>(CreateNavigator);
    }

    public async Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var memberInfo = context.JsonPropertyInfo?.DeclaringType;
        if (schema.Description is null &&
            memberInfo is not null &&
            GetMemberName(context.JsonTypeInfo, context.JsonPropertyInfo) is { Length: > 0 } memberName)
        {
            schema.Description = await GetDescription(memberName, context.JsonTypeInfo, context.JsonPropertyInfo);
        }
    }

    private async ValueTask<string?> GetDescription(string memberName, JsonTypeInfo typeInfo, JsonPropertyInfo? propertyInfo)
    {
        if (_descriptions.TryGetValue(memberName, out var description))
        {
            return description;
        }

        var navigator = _navigator.Value;

        var xpath = $"/doc/members/member[@name='{memberName}']/summary";
        var summaryNode = navigator?.SelectSingleNode(xpath);
        description = summaryNode?.Value.Trim();
        if (_getDescription is not null)
        {
            description = await _getDescription(typeInfo, propertyInfo, description);
        }

        _descriptions[memberName] = description;

        return description;
    }

    private static XPathNavigator? CreateNavigator()
    {
        var assemblyPath = typeof(T).Assembly.Location;
        var xmlPath = Path.ChangeExtension(assemblyPath, ".xml");

        if (!File.Exists(xmlPath))
        {
            return null;
        }

        using var reader = XmlReader.Create(xmlPath);
        return new XPathDocument(reader).CreateNavigator();
    }

    private static string? GetMemberName(JsonTypeInfo typeInfo, JsonPropertyInfo? propertyInfo)
    {
        if (propertyInfo is null)
        {
            return $"T:{typeInfo.Type.FullName}";
        }

        var declaringType = propertyInfo.DeclaringType.FullName;
        if (declaringType is null)
        {
            return null;
        }

        var memberName = propertyInfo.AttributeProvider switch
        {
            MemberInfo member => member.Name,
            _ => $"{char.ToUpperInvariant(propertyInfo.Name[0])}{propertyInfo.Name[1..]}"
        };

        // Determine member type prefix: Property (P) or Field (F)
        var memberType = propertyInfo.AttributeProvider switch
        {
            PropertyInfo => "P",
            FieldInfo => "F",
            _ => null
        };

        return memberType is null
            ? null
            : $"{memberType}:{declaringType}{Type.Delimiter}{memberName}";
    }
}
