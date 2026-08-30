using Roastery.Api.Data;
using OpenApiExamples;
using OpenApiExamples.Abstractions;

namespace Roastery.Api.Invoices.Contracts;

public class InvoiceExample : ISingleOpenApiExamplesProvider<Invoice>
{
    public IOpenApiExample<Invoice> GetExample() =>
        OpenApiExample.Create("default", MockData.Invoices[0]);
}

public class InvoiceListExamples : IMultipleOpenApiExamplesProvider<Invoice[]>
{
    public IEnumerable<IOpenApiExample<Invoice[]>> GetExamples() =>
    [
        OpenApiExample.Create(
            key: "ledger",
            summary: "Everything on the books",
            value: MockData.Invoices
        ),
        OpenApiExample.Create(
            key: "overdue",
            summary: "Filtered by status",
            description: "What GET /api/invoices?status=Overdue comes back with.",
            value: MockData.Invoices.Where(invoice => invoice.Status == InvoiceStatus.Overdue).ToArray()
        ),
    ];
}

public class IssueInvoiceRequestExamples : IMultipleOpenApiExamplesProvider<IssueInvoiceRequest>
{
    public IEnumerable<IOpenApiExample<IssueInvoiceRequest>> GetExamples() =>
    [
        OpenApiExample.Create(
            key: "singleLine",
            summary: "One line, standard terms",
            description: "Leave dueOn off and the accounts system applies thirty days.",
            value: new IssueInvoiceRequest
            {
                Customer = "Northgate Deli",
                Lines =
                [
                    new InvoiceLine
                    {
                        BeanId = "house-blend",
                        Bags = 5,
                        PricePerBag = 28.00m,
                        LineTotal = 140.00m,
                    },
                ],
            }
        ),
        OpenApiExample.Create(
            key: "multiLine",
            summary: "Two lines with an agreed due date",
            value: new IssueInvoiceRequest
            {
                Customer = "Harbour Cafe",
                DueOn = new DateTime(2026, 4, 30),
                Lines =
                [
                    new InvoiceLine
                    {
                        BeanId = "house-blend",
                        Bags = 10,
                        PricePerBag = 26.50m,
                        LineTotal = 265.00m,
                    },
                    new InvoiceLine
                    {
                        BeanId = "kenya-nyeri-aa",
                        Bags = 4,
                        PricePerBag = 12.50m,
                        LineTotal = 50.00m,
                    },
                ],
            }
        ),
    ];
}

public class RecordPaymentRequestExample : ISingleOpenApiExamplesProvider<RecordPaymentRequest>
{
    public IOpenApiExample<RecordPaymentRequest> GetExample() =>
        OpenApiExample.Create(
            key: "bankTransfer",
            summary: "Settled by transfer",
            value: new RecordPaymentRequest
            {
                PaidOn = new DateTime(2026, 4, 2),
                Reference = "FT26092K7QW1",
            }
        );
}
