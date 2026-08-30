using System.Net.Mime;
using Roastery.Api.Data;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Invoices.Endpoints;

internal static class Void
{
    public static RouteGroupBuilder MapVoidInvoice(this RouteGroupBuilder group)
    {
        group.MapDelete("{number}", Handle)
            .WithName("invoices.void")
            .WithSummary("Void an invoice")
            .WithDescription("An issued invoice is never deleted, only voided.")
            .Produces(StatusCodes.Status204NoContent)
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
            : TypedResults.NoContent();
    }
}
