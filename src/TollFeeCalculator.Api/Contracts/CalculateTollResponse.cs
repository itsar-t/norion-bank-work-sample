
namespace TollFeeCalculator.Api.Contracts;

/* The recor CalculateTollResponse describes what kind off JSON response will give when recieving CalculateTollRequest*/

public sealed record CalculateTollResponse(
    int TotalFee,
    IReadOnlyList<TollPassageResponse> Passages
);

/*
Example:

{
    "totalFee": 28
}

*/