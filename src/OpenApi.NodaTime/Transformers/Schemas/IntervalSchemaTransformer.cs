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

public class IntervalSchemaTransformer : IOpenApiSchemaTransformer
{
    private readonly Instant _instant;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public IntervalSchemaTransformer(Instant instant, JsonSerializerOptions jsonSerializerOptions)
    {
        _instant = instant;
        _jsonSerializerOptions = jsonSerializerOptions ?? throw new ArgumentNullException(nameof(jsonSerializerOptions));
    }

    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.JsonTypeInfo.Type != typeof(Interval))
        {
            return Task.CompletedTask;
        }

        var interval = new Interval(_instant,
                                    _instant.PlusTicks(TimeSpan.TicksPerDay)
                                            .PlusTicks(TimeSpan.TicksPerHour)
                                            .PlusTicks(TimeSpan.TicksPerMinute)
                                            .PlusTicks(TimeSpan.TicksPerSecond)
                                            .PlusTicks(TimeSpan.TicksPerMillisecond));

        schema.Type = "object";
        schema.Properties = new Dictionary<string, OpenApiSchema>
                            {
                                {
                                    nameof(Interval.Start),
                                    new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "date-time",
                                        Example = new OpenApiString(JsonSerializer.Serialize(interval.Start, _jsonSerializerOptions))
                                    }
                                },
                                {
                                    nameof(Interval.End),
                                    new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "date-time",
                                        Example = new OpenApiString(JsonSerializer.Serialize(interval.End, _jsonSerializerOptions))
                                    }
                                }
                            };

        return Task.CompletedTask;
    }
}
