# Roastery.Api

A sample minimal API wired up with [OpenApiExamples](../../README.md). It is a coffee roastery: bags of
beans on one side, customer orders in the middle, wholesale invoices at the back.

There is no database. Every handler answers from `Data/MockData.cs`, and the example providers read the
same arrays, so the documentation and the canned responses cannot drift apart.

## Run it

```text
dotnet run --project samples/Roastery.Api
```

Then open one of:

| Url | What it is |
|---|---|
| <http://localhost:5280/scalar> | Scalar |
| <http://localhost:5280/swagger> | Swagger UI |
| <http://localhost:5280/openapi/v1.json> | the document itself |

Both renderers read the same document. Worth opening both, since they show named examples differently.

## Three groups, three kinds of consumer

The point of the third group is that examples are per content type, so an API serving more than one has to
say so more than once.

| Group | Speaks | Because |
|---|---|---|
| `/api/beans` | JSON only | A modern first-party catalogue. Web and mobile clients, nothing older. |
| `/api/orders` | JSON **and** XML | Our own app posts JSON; a partner's till can only manage XML. Both get served. |
| `/api/invoices` | XML only | The accounts system on the other end is the finance world - ISO 20022, UBL, EDI. It reads XML and nothing else. |

Problem responses stay `application/problem+json` everywhere, including in the XML group.

Minimal APIs only speak JSON out of the box, so `Shared/ContentNegotiation.cs` carries the XML half: a
result that writes XML when the caller asks for it, and a body reader the request contracts call from
`BindAsync`. Both buffer through a `MemoryStream`, because `XmlSerializer` is synchronous and Kestrel
refuses synchronous reads and writes.

Try it:

```text
curl localhost:5280/api/orders/ORD-1042
curl -H 'Accept: application/xml' localhost:5280/api/orders/ORD-1042
curl -X POST localhost:5280/api/invoices -H 'Content-Type: application/xml' --data @invoice.xml
```

## Layout

One file per endpoint, three route groups, and every file is reached through an extension method.

```text
Shared/       problem contract and examples, plus the XML content negotiation
Data/         the mock shelf, order book and ledger
Beans/        BeanEndpoints, contracts, examples, Endpoints/List Get Create Update Delete
Orders/       OrderEndpoints, contracts, examples, Endpoints/List Get Place UpdateStatus Cancel
Invoices/     InvoiceEndpoints, contracts, examples, Endpoints/List Get Issue MarkPaid Void
```

`Program.cs` calls `app.MapBeanEndpoints()`, `app.MapOrderEndpoints()` and `app.MapInvoiceEndpoints()`. Each
builds its group, declares what every endpoint under it shares, then chains one `Map...` call per endpoint
file:

```csharp
group
    .MapListOrders()
    .MapGetOrder()
    .MapPlaceOrder()
    .MapUpdateOrderStatus()
    .MapCancelOrder();
```

## What each part of the library shows up as

| Feature | Where to look |
|---|---|
| `RequestExamples<T>` with a named dropdown | `Orders/Endpoints/Place.cs`, three shapes of order |
| `RequestExample<T>`, a single example | `Beans/Endpoints/Update.cs` |
| `ResponseExamples<T>`, named responses | `Beans/Endpoints/List.cs`, including an empty result |
| `ResponseExample<T>` per status code | any endpoint, 200 / 201 / 404 |
| The same example in two content types | `Orders/Endpoints/Get.cs`, one call each for JSON and XML |
| An XML-only operation | anything under `Invoices/Endpoints/` |
| Group level examples | `Beans/BeanEndpoints.cs`, the shared 500 |
| Examples inheriting the app's JSON settings | `Program.cs`, one `JsonStringEnumConverter` for both |
| Examples built from the same data as the handlers | `Data/MockData.cs` |

## Three things that bite

**`Produces` replaces, per status code.** A second call for the same status code drops the first call's
content types, and any example aimed at those is then skipped without a word. List them in one call:
`.Produces<Order>(200, "application/json", "application/xml")`.

**A contract with `BindAsync` leaves the request body.** ASP.NET Core stops treating it as a body parameter,
so the operation loses its `requestBody` and request examples have nowhere to land. `.Accepts<T>(...)` puts
it back. See `Orders/Endpoints/Place.cs`.

**Swagger UI writes its XML sample from the schema, not from your example.** It takes the root element name
off the `$ref`, so single-item endpoints are fine, but a list response is an inline array with no name and
it prints `XML example cannot be generated; root element name is undefined`. The schema transformer in
`Program.cs` names the arrays `ArrayOfOrder`, matching what `XmlSerializer` writes.

## Not done

No validation, no persistence, no auth. `Place.cs` charges a flat 3.20 a line and `Issue.cs` adds VAT at a
hard-coded 24%. The point is the document it generates, not the roastery.
