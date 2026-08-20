using TollFeeCalculator.Api.Contracts;
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
            .WithSummary(
                "Calculates the total toll fee for one day."
            );

        return endpoints;
    }

    private static IResult CalculateTollFee(
        CalculateTollRequest request,
        TollCalculator calculator)
    {
        if (request.Passages is null ||
            request.Passages.Length == 0)
        {
            return Results.BadRequest(new
            {
                error = "At least one passage is required."
            });
        }

        /*
            The endpoint validates client input before calling Core so
            that passages from different days produce HTTP 400 instead
            of an unhandled exception and HTTP 500.
         */
        DateTime passageDate =
            request.Passages[0].Date;

        bool containsMultipleDates = request.Passages
            .Any(passage =>
                passage.Date != passageDate);

        if (containsMultipleDates)
        {
            return Results.BadRequest(new
            {
                error =
                    "All passages must occur on the same day."
            });
        }

        Vehicle vehicle =
            new(request.VehicleType);

        TollCalculationResult calculation =
            calculator.Calculate(
                vehicle,
                request.Passages
            );

        TollPassageResponse[] passageResponses =
            calculation.Passages
                .Select(passage =>
                    new TollPassageResponse(
                        passage.PassageTime,
                        passage.PassageFee,
                        passage.RunningTotal,
                        passage.ChargePeriodNumber,
                        passage.StartsNewChargePeriod,
                        passage.DailyCapReached
                    )
                )
                .ToArray();

        CalculateTollResponse response = new(
            calculation.TotalFee,
            calculation.MaximumDailyFee,
            calculation.SingleChargePeriodMinutes,
            passageResponses
        );

        return Results.Ok(response);
    }
}