namespace TollFeeCalculator.Models;

public sealed record TollCalculationResult(
    int TotalFee,
    IReadOnlyList<TollPassageResult> Passages
);