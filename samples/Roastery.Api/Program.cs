using System.Text.Json.Serialization;
using Microsoft.OpenApi;
using OpenApiExamples.ExtensionMethods;
using Roastery.Api.Beans;
using Roastery.Api.Invoices;
using Roastery.Api.Orders;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Roast levels and order statuses as words, not numbers. The examples inherit this, so the schema and the
// example beside it say the same thing.
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApiExamples();
builder.Services.AddOpenApi(options =>
{
    options.AddExamples();

    // Swagger UI takes an XML root name off a $ref, so the single-item endpoints are fine on their own.
    // A list response is an inline array with nothing to take a name from, and it prints "XML example
    // cannot be generated" instead. ArrayOfOrder is what XmlSerializer actually writes.
    options.AddSchemaTransformer((schema, context, _) =>
    {
        var element = context.JsonTypeInfo.Type.GetElementType();

        if (schema.Type == JsonSchemaType.Array && element is not null)
        {
            schema.Xml ??= new OpenApiXml { Name = $"ArrayOf{element.Name}", Wrapped = true };
        }

        return Task.CompletedTask;
    });
});

builder.Services.AddProblemDetails();

var app = builder.Build();

app.MapOpenApi();

// Both renderers, off the one document, because the examples have to look right in either.
app.MapScalarApiReference();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "Roastery API"));

app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

app.MapBeanEndpoints();
app.MapOrderEndpoints();
app.MapInvoiceEndpoints();

app.Run();
