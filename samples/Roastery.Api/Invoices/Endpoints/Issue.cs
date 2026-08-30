using System.Net.Mime;
using Roastery.Api.Invoices.Contracts;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Invoices.Endpoints;

internal static class Issue
{
    public static RouteGroupBuilder MapIssueInvoice(this RouteGroupBuilder group)
    {
        group.MapPost("", Handle)
            .WithName("invoices.issue")
            .WithSummary("Issue an invoice")
            .WithDescription("Raises an invoice against a wholesale customer. VAT is added at 24%.")
            .Accepts<IssueInvoiceRequest>(MediaTypeNames.Application.Xml)
            .RequestExamples<IssueInvoiceRequestExamples>(MediaTypeNames.Application.Xml)
            .Produces<Invoice>(StatusCodes.Status201Created, MediaTypeNames.Application.Xml)
            .ResponseExample<InvoiceExample>(StatusCodes.Status201Created, MediaTypeNames.Application.Xml)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ResponseExample<ValidationProblemExample>(
                StatusCodes.Status400BadRequest,
                MediaTypeNames.Application.ProblemJson
            );

        return group;
    }

    private static IResult Handle(IssueInvoiceRequest request)
    {
        var number = $"INV-2026-{Random.Shared.Next(1000, 9999)}";
        var issuedOn = DateTime.UtcNow.Date;
        var net = request.Lines.Sum(line => line.LineTotal);
        var vat = Math.Round(net * 0.24m, 2);

        var invoice = new Invoice
        {
            Number = number,
            Customer = request.Customer,
            IssuedOn = issuedOn,
            DueOn = request.DueOn ?? issuedOn.AddDays(30),
            Status = InvoiceStatus.Issued,
            Lines = request.Lines,
            Net = net,
            Vat = vat,
            Gross = net + vat,
        };

        return ContentNegotiation.XmlCreated($"/api/invoices/{number}", invoice);
    }
}
