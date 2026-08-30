using System.Net.Mime;
using Roastery.Api.Beans.Contracts;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Beans.Endpoints;

internal static class Create
{
    public static RouteGroupBuilder MapCreateBean(this RouteGroupBuilder group)
    {
        group.MapPost("", Handle)
            .WithName("beans.create")
            .WithSummary("Add a bean")
            .WithDescription("Puts a new bag on the shelf. The id is slugged from the name.")
            .RequestExamples<CreateBeanRequestExamples>(MediaTypeNames.Application.Json)
            .Produces<Bean>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .ResponseExample<BeanExample>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ResponseExample<ValidationProblemExample>(
                StatusCodes.Status400BadRequest,
                MediaTypeNames.Application.ProblemJson
            );

        return group;
    }

    private static IResult Handle(CreateBeanRequest request)
    {
        var id = request.Name.ToLowerInvariant().Replace(' ', '-');

        var bean = new Bean
        {
            Id = id,
            Name = request.Name,
            Origin = request.Origin,
            Roast = request.Roast,
            BagSizeGrams = request.BagSizeGrams,
            PricePerBag = request.PricePerBag,
            InStock = true,
            TastingNotes = request.TastingNotes,
        };

        return TypedResults.Created($"/api/beans/{id}", bean);
    }
}
