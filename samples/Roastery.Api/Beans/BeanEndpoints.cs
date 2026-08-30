using System.Net.Mime;
using Roastery.Api.Beans.Endpoints;
using Roastery.Api.Shared;
using OpenApiExamples.ExtensionMethods;

namespace Roastery.Api.Beans;

internal static class BeanEndpoints
{
    /// <summary>
    /// Everything under /api/beans. The 500 example is declared once on the group and every endpoint
    /// inside it picks it up.
    /// </summary>
    public static IEndpointRouteBuilder MapBeanEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/beans")
            .WithTags("Beans")
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ResponseExample<ServerErrorProblemExample>(
                StatusCodes.Status500InternalServerError,
                MediaTypeNames.Application.ProblemJson
            );

        group
            .MapListBeans()
            .MapGetBean()
            .MapCreateBean()
            .MapUpdateBean()
            .MapDeleteBean();

        return endpoints;
    }
}
