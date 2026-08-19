namespace TollFeeCalculator.Models;

public sealed record TollPassageResult(
    DateTime PassageTime,
    int PassageFee,
    int RunningTotal
);