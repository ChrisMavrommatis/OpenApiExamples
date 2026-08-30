using Roastery.Api.Shared;

namespace Roastery.Api.Invoices.Contracts;

public enum InvoiceStatus
{
    Issued,
    Paid,
    Overdue,
    Void,
}

/// <summary>
/// A wholesale invoice. DateTime rather than DateTimeOffset because the accounts system takes dates,
/// not instants.
/// </summary>
public record Invoice
{
    public required string Number { get; init; }
    public required string Customer { get; init; }
    public required DateTime IssuedOn { get; init; }
    public required DateTime DueOn { get; init; }
    public required InvoiceStatus Status { get; init; }
    public required InvoiceLine[] Lines { get; init; }
    public required decimal Net { get; init; }
    public required decimal Vat { get; init; }
    public required decimal Gross { get; init; }
}

public record InvoiceLine
{
    public required string BeanId { get; init; }
    public required int Bags { get; init; }
    public required decimal PricePerBag { get; init; }
    public required decimal LineTotal { get; init; }
}

public record IssueInvoiceRequest
{
    public required string Customer { get; init; }
    public required InvoiceLine[] Lines { get; init; }
    public DateTime? DueOn { get; init; }

    public static ValueTask<IssueInvoiceRequest?> BindAsync(HttpContext httpContext) =>
        RequestBody.ReadAsync<IssueInvoiceRequest>(httpContext);
}

public record RecordPaymentRequest
{
    public required DateTime PaidOn { get; init; }

    /// <summary>The bank reference. This is what the accounts system reconciles against.</summary>
    public required string Reference { get; init; }

    public static ValueTask<RecordPaymentRequest?> BindAsync(HttpContext httpContext) =>
        RequestBody.ReadAsync<RecordPaymentRequest>(httpContext);
}
