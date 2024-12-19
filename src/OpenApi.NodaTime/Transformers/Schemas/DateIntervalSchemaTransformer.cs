using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using NodaTime;

namespace OpenApi.NodaTime.Transformers.Schemas;

public class DateIntervalSchemaTransformer : IOpenApiSchemaTransformer
{
    private readonly DateTimeZone _dateTimeZone;
    private readonly Instant _instant;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public DateIntervalSchemaTransformer(Instant instant, DateTimeZone dateTimeZone, JsonSerializerOptions jsonSerializerOptions)
    {
        _instant = instant;
        _dateTimeZone = dateTimeZone ?? throw new ArgumentNullException(nameof(dateTimeZone));
        _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
    }

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type != typeof(Interval))
        {
            return Task.CompletedTask;
        }

        var zoned = _instant.InZone(_dateTimeZone);
        var dateInterval = new DateInterval(zoned.Date, zoned.Date.PlusDays(1));

        schema.Type = "object";
        schema.Properties = new Dictionary<string, OpenApiSchema>
                            {
                                {
                                    nameof(DateInterval.Start),
                                    new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "date",
                                        Example = new OpenApiString(JsonSerializer.Serialize(dateInterval.Start, _jsonSerializerOptions))
                                    }
                                },
                                {
                                    nameof(DateInterval.End),
                                    new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "date",
                                        Example = new OpenApiString(JsonSerializer.Serialize(dateInterval.End, _jsonSerializerOptions))
                                    }
                                }
                            };

        return Task.CompletedTask;
    }
}
