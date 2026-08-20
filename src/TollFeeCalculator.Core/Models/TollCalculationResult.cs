namespace TollFeeCalculator.Models;

public sealed record TollCalculationResult(
    int TotalFee,
    int MaximumDailyFee,
    int SingleChargePeriodMinutes,
    IReadOnlyList<TollPassageResult> Passages
);