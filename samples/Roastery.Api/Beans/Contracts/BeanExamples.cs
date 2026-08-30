using Roastery.Api.Data;
using OpenApiExamples;
using OpenApiExamples.Abstractions;

namespace Roastery.Api.Beans.Contracts;

public class BeanExample : ISingleOpenApiExamplesProvider<Bean>
{
    public IOpenApiExample<Bean> GetExample() =>
        OpenApiExample.Create("default", MockData.Beans[0]);
}

public class BeanListExamples : IMultipleOpenApiExamplesProvider<Bean[]>
{
    public IEnumerable<IOpenApiExample<Bean[]>> GetExamples() =>
    [
        OpenApiExample.Create(
            key: "wholeShelf",
            summary: "Everything we roast",
            value: MockData.Beans
        ),
        OpenApiExample.Create(
            key: "lightRoasts",
            summary: "Filtered by roast",
            description: "What GET /api/beans?roast=Light comes back with.",
            value: MockData.Beans.Where(b => b.Roast == RoastLevel.Light).ToArray()
        ),
        OpenApiExample.Create(
            key: "soldOut",
            summary: "Nothing matched",
            value: Array.Empty<Bean>()
        ),
    ];
}

public class CreateBeanRequestExamples : IMultipleOpenApiExamplesProvider<CreateBeanRequest>
{
    public IEnumerable<IOpenApiExample<CreateBeanRequest>> GetExamples() =>
    [
        OpenApiExample.Create(
            key: "singleOrigin",
            summary: "A single origin, light roast",
            value: new CreateBeanRequest
            {
                Name = "Huila Pink Bourbon",
                Origin = "Colombia",
                Roast = RoastLevel.Light,
                BagSizeGrams = 250,
                PricePerBag = 14.00m,
                TastingNotes = ["red apple", "honey"],
            }
        ),
        OpenApiExample.Create(
            key: "houseBlend",
            summary: "A kilo bag of blend",
            description: "Blends carry both origins in one string. Nobody has asked for a list yet.",
            value: new CreateBeanRequest
            {
                Name = "Winter Blend",
                Origin = "Brazil / Guatemala",
                Roast = RoastLevel.Dark,
                BagSizeGrams = 1000,
                PricePerBag = 26.00m,
                TastingNotes = ["cocoa", "walnut"],
            }
        ),
        OpenApiExample.Create(
            key: "bare",
            summary: "Only the required fields",
            value: new CreateBeanRequest
            {
                Name = "Guji Decaf",
                Origin = "Ethiopia",
                Roast = RoastLevel.Medium,
                BagSizeGrams = 250,
                PricePerBag = 13.00m,
            }
        ),
    ];
}

public class UpdateBeanRequestExample : ISingleOpenApiExamplesProvider<UpdateBeanRequest>
{
    public IOpenApiExample<UpdateBeanRequest> GetExample() =>
        OpenApiExample.Create(
            key: "priceRise",
            summary: "Put the price up and mark it back in stock",
            value: new UpdateBeanRequest
            {
                Name = "Nyeri AA",
                Roast = RoastLevel.Light,
                PricePerBag = 13.50m,
                InStock = true,
            }
        );
}
