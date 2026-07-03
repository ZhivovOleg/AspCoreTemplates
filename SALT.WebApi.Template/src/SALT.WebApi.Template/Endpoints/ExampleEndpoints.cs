using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi;
using SALT.WebApi.Template.Dto;
using SALT.WebApi.Template.Services;

namespace SALT.WebApi.Template.Endpoints;

/// <summary>
/// Minimal API examples.
/// </summary>
internal static class ExampleEndpoints
{
    /// <summary>
    /// Prepare and register endpoints.
    /// </summary>
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

        _ = group.MapGet("/{id:int}", GetExample)
            .AddGetExampleDocumentation();

        _ = group.MapPost(string.Empty, ProcessExample)
            .AddProcessExampleDocumentation();

        return group;
    }

    /// <summary>
    /// GET example.
    /// </summary>
    private static async Task<IResult> GetExample(
        [FromRoute] int id,
        [FromServices] IExampleDatabaseService service)
    {
        if (id <= 0)
            return CreateValidationProblem(nameof(id));

        ExampleResponseDto result = await service.GetData(id);
        return Results.Ok(result);
    }

    /// <summary>
    /// POST example.
    /// </summary>
    private static IResult ProcessExample(
        [FromBody] ExampleRequestDto request) =>
        request.Id <= 0
            ? CreateValidationProblem(nameof(request.Id))
            : Results.Ok(new ExampleResponseDto
            {
                Id = request.Id,
                Result = "Example request was processed. No data was persisted.",
            });

    private static IResult CreateValidationProblem(string fieldName) =>
        Results.ValidationProblem(
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                [fieldName] = ["Id must be greater than zero."],
            },
            statusCode: StatusCodes.Status422UnprocessableEntity);

    private static RouteHandlerBuilder AddGetExampleDocumentation(this RouteHandlerBuilder builder) =>
        builder
            .WithName("GetMinimalExample")
            .WithSummary("Returns example data")
            .WithDescription("Returns example data by identifier. This endpoint demonstrates GET documentation for minimal APIs.")
            .Produces<ExampleResponseDto>(
                StatusCodes.Status200OK,
                contentType: "application/json")
            .ProducesValidationProblem(
                StatusCodes.Status422UnprocessableEntity,
                contentType: "application/problem+json")
            .ProducesProblem(
                StatusCodes.Status500InternalServerError,
                contentType: "application/problem+json")
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Description = "Reads example data from the shared database. No data is modified.";
                AddJsonResponseExample(
                    operation,
                    "200",
                    "Example data was found.",
                    new JsonObject
                    {
                        ["id"] = 1,
                        ["result"] = "hello from postgres",
                    });

                return Task.CompletedTask;
            });

    private static RouteHandlerBuilder AddProcessExampleDocumentation(this RouteHandlerBuilder builder) =>
        builder
            .WithName("ProcessMinimalExample")
            .WithSummary("Processes example data")
            .WithDescription("Processes an example request without persisting anything. This endpoint demonstrates POST documentation for minimal APIs.")
            .Accepts<ExampleRequestDto>("application/json")
            .Produces<ExampleResponseDto>(
                StatusCodes.Status200OK,
                contentType: "application/json")
            .ProducesValidationProblem(
                StatusCodes.Status422UnprocessableEntity,
                contentType: "application/problem+json")
            .ProducesProblem(
                StatusCodes.Status500InternalServerError,
                contentType: "application/problem+json")
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Description = "Validates and processes the request body. The operation is intentionally side-effect free.";
                AddJsonRequestExample(
                    operation,
                    new JsonObject
                    {
                        ["id"] = 1,
                    });

                AddJsonResponseExample(
                    operation,
                    "200",
                    "Example request was processed.",
                    new JsonObject
                    {
                        ["id"] = 1,
                        ["result"] = "Example request was processed. No data was persisted.",
                    });

                return Task.CompletedTask;
            });

    private static void AddJsonRequestExample(OpenApiOperation operation, JsonNode example)
    {
        if (operation.RequestBody?.Content is null)
            return;

        operation.RequestBody.Description = "Example payload used to demonstrate POST request body documentation.";

        if (operation.RequestBody.Content.TryGetValue("application/json", out OpenApiMediaType? mediaType))
            mediaType.Example = example;
    }

    private static void AddJsonResponseExample(OpenApiOperation operation, string statusCode, string description, JsonNode example)
    {
        if (operation.Responses is null || !operation.Responses.TryGetValue(statusCode, out IOpenApiResponse? response))
            return;

        response.Description = description;

        if (response.Content is null)
            return;

        if (response.Content.TryGetValue("application/json", out OpenApiMediaType? mediaType))
            mediaType.Example = example;
    }
}