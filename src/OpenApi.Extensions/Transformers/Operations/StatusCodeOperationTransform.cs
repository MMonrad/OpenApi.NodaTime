using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace OpenApi.Extensions.Transformers.Operations;

public class StatusCodeOperationTransform : IOpenApiOperationTransformer
{
    private readonly string? _description;
    private readonly HttpStatusCode _statusCode;

    public StatusCodeOperationTransform(HttpStatusCode statusCode, string? description)
    {
        _statusCode = statusCode;
        _description = description;
    }

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var statusCode = Convert.ToInt32(_statusCode);
        if (operation.Responses.ContainsKey(statusCode.ToString()))
        {
            return Task.CompletedTask;
        }

        operation.Responses.Add($"{statusCode}", new OpenApiResponse { Description = _description });
        return Task.CompletedTask;
    }
}

public class StatusCodeOperationTransform<T> : IOpenApiOperationTransformer
{
    private readonly string? _description;
    private readonly HttpStatusCode _statusCode;

    public StatusCodeOperationTransform(HttpStatusCode statusCode, string? description)
    {
        _statusCode = statusCode;
        _description = description;
    }

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var statusCode = Convert.ToInt32(_statusCode);
        if (operation.Responses.ContainsKey(statusCode.ToString()))
        {
            return Task.CompletedTask;
        }

        operation.Responses.Add($"{statusCode}",
            new OpenApiResponse
            {
                Description = _description,
                Content = new Dictionary<string, OpenApiMediaType>
                    { { "application/Json", new OpenApiMediaType { Schema = new OpenApiSchema { Type = typeof(T).Name } } } }
            });
        return Task.CompletedTask;
    }
}
