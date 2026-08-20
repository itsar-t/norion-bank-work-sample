
namespace TollFeeCalculator.Api.Contracts;

public sealed record CalculateTollResponse(
    int TotalFee,
    int MaximumDailyFee,
    int SingleChargePeriodMinutes,
    IReadOnlyList<TollPassageResponse> Passages
);
/*
Example:

{
    "totalFee": 28
}

*/