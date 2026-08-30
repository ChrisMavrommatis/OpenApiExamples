using System.Net.Mime;
using Roastery.Api.Data;
using Roastery.Api.Invoices.Contracts;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Invoices.Endpoints;

internal static class List
{
    public static RouteGroupBuilder MapListInvoices(this RouteGroupBuilder group)
    {
        group.MapGet("", Handle)
            .WithName("invoices.list")
            .WithSummary("List invoices")
            .WithDescription("The open ledger, narrowed by status.")
            .Produces<Invoice[]>(StatusCodes.Status200OK, MediaTypeNames.Application.Xml)
            .ResponseExamples<InvoiceListExamples>(
                StatusCodes.Status200OK,
                MediaTypeNames.Application.Xml
            );

        return group;
    }

    private static IResult Handle(InvoiceStatus? status)
    {
        var invoices = MockData.Invoices
            .Where(invoice => status is null || invoice.Status == status)
            .ToArray();

        return ContentNegotiation.Xml(invoices);
    }
}
