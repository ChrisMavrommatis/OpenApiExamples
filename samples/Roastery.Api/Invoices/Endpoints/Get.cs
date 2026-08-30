using System.Net.Mime;
using Roastery.Api.Data;
using Roastery.Api.Invoices.Contracts;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Invoices.Endpoints;

internal static class Get
{
    public static RouteGroupBuilder MapGetInvoice(this RouteGroupBuilder group)
    {
        group.MapGet("{number}", Handle)
            .WithName("invoices.get")
            .WithSummary("Get an invoice")
            .Produces<Invoice>(StatusCodes.Status200OK, MediaTypeNames.Application.Xml)
            .ResponseExample<InvoiceExample>(StatusCodes.Status200OK, MediaTypeNames.Application.Xml)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ResponseExample<NotFoundProblemExample>(
                StatusCodes.Status404NotFound,
                MediaTypeNames.Application.ProblemJson
            );

        return group;
    }

    private static IResult Handle(string number)
    {
        var invoice = MockData.Invoices.FirstOrDefault(invoice => invoice.Number == number);

        return invoice is null
            ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, detail: "No invoice is filed under that number.")
            : ContentNegotiation.Xml(invoice);
    }
}
