using System.Net.Mime;
using Roastery.Api.Data;
using Roastery.Api.Invoices.Contracts;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Invoices.Endpoints;

internal static class MarkPaid
{
    public static RouteGroupBuilder MapRecordInvoicePayment(this RouteGroupBuilder group)
    {
        group.MapPost("{number}/payment", Handle)
            .WithName("invoices.recordPayment")
            .WithSummary("Record a payment")
            .WithDescription("Settles the invoice against a bank reference.")
            .Accepts<RecordPaymentRequest>(MediaTypeNames.Application.Xml)
            .RequestExample<RecordPaymentRequestExample>(MediaTypeNames.Application.Xml)
            .Produces<Invoice>(StatusCodes.Status200OK, MediaTypeNames.Application.Xml)
            .ResponseExample<InvoiceExample>(StatusCodes.Status200OK, MediaTypeNames.Application.Xml)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ResponseExample<NotFoundProblemExample>(
                StatusCodes.Status404NotFound,
                MediaTypeNames.Application.ProblemJson
            );

        return group;
    }

    private static IResult Handle(string number, RecordPaymentRequest request)
    {
        var invoice = MockData.Invoices.FirstOrDefault(invoice => invoice.Number == number);

        return invoice is null
            ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, detail: "No invoice is filed under that number.")
            : ContentNegotiation.Xml(invoice with { Status = InvoiceStatus.Paid });
    }
}
