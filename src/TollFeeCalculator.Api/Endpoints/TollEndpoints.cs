using TollFeeCalculator.Api.Contracts;
using TollFeeCalculator.Enums;
using TollFeeCalculator.Models;
using TollFeeCalculator.Services;

namespace TollFeeCalculator.Api.Endpoints;

public static class TollEndpoints
{
    public static IEndpointRouteBuilder MapTollEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/toll")
            .WithTags("Toll calculation");


        group.MapPost("/calculate", CalculateTollFee)
            .WithName("CalculateTollFee")
            .WithSummary("Calculates the total toll fee for one day.");

        return endpoints;
    }

    private static IResult CalculateTollFee(
        CalculateTollRequest request,
        TollCalculator calculator
    )
    {
        if (request.Passages is null || request.Passages.Length == 0)
        {
            return Results.BadRequest(new
            {
                error = "At least one passage is required."
            });
        }

        //Check that dates sent only conatin 1 day ie. one Date with different times

        DateTime passageDate = request.Passages[0].Date;

        bool containsMultipleDates = request.Passages
            .Any(passage => passage.Date != passageDate);

        if (containsMultipleDates)
        {
            return Results.BadRequest(new
            {
                error = "All passages must occur on the same day."
            });
        }

        Vehicle vehicle = new(request.VehicleType);

        int totalFee = calculator.GetTollFee(
            vehicle,
            request.Passages
        );

        CalculateTollResponse response = new(totalFee);

        return Results.Ok(response);
    }
}