using System.Net.Mime;
using Roastery.Api.Invoices.Endpoints;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Invoices;

internal static class InvoiceEndpoints
{
    /// <summary>
    /// Everything under /api/invoices. The accounts system on the other end only reads XML, so nothing in
    /// this group declares JSON. Problems are the exception, they stay problem+json.
    /// </summary>
    public static IEndpointRouteBuilder MapInvoiceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/invoices")
            .WithTags("Invoices")
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ResponseExample<ServerErrorProblemExample>(
                StatusCodes.Status500InternalServerError,
                MediaTypeNames.Application.ProblemJson
            );

        group
            .MapListInvoices()
            .MapGetInvoice()
            .MapIssueInvoice()
            .MapRecordInvoicePayment()
            .MapVoidInvoice();

        return endpoints;
    }
}
