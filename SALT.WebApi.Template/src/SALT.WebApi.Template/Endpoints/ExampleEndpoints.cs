using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SALT.WebApi.Template.Dto;
using SALT.WebApi.Template.Services;

namespace SALT.WebApi.Template.Endpoints;

internal static class ExampleEndpoints
{
    internal static RouteGroupBuilder MapExampleMinimalEndpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet versions = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        RouteGroupBuilder group = app
            .MapGroup("/api/v{version:apiVersion}/minimal-example")
            .WithApiVersionSet(versions)
            .MapToApiVersion(new ApiVersion(1, 0))
            .WithTags("Minimal Example");

        _ = group.MapPost(
            "/get-example",
            async Task<IResult> (
                ExampleRequestDto request,
                IExampleDatabaseService service,
                CancellationToken cancellationToken) =>
                {
                    if (request.Id <= 0)
                        return Results.ValidationProblem(
                            new Dictionary<string, string[]>(StringComparer.Ordinal)
                            {
                                [nameof(request.Id)] = ["Id must be greater than zero."],
                            },
                            statusCode: StatusCodes.Status422UnprocessableEntity);

                    ExampleResponseDto result = await service.GetData(request.Id);
                    return Results.Ok(result);
                })
            .WithName("GetMinimalExample")
            .Accepts<ExampleRequestDto>("application/json")
            .Produces<ExampleResponseDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return group;
    }
}