namespace OpenApiExamples.Models;

public class RequestExampleMetadata
{
    public RequestExampleMetadata(string contentType, Type providerType)
    {
        this.ContentType = contentType;
        this.ProviderType = providerType;
    }
    public string ContentType { get; }
    public Type ProviderType { get; }
}