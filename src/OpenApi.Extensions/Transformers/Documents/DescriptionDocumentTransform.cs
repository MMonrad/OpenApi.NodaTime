using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace OpenApi.Extensions.Transformers.Documents;

public class DescriptionDocumentTransform : IOpenApiDocumentTransformer
{
    private readonly string _description;

    public DescriptionDocumentTransform(string description)
    {
        _description = description;
    }

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info.Description = _description;
        await Task.CompletedTask;
    }
}
