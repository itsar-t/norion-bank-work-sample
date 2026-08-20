namespace TollFeeCalculator.Api.Contracts;

public sealed record TollPassageResponse(
    DateTime PassageTime,
    int PassageFee,
    int RunningTotal,
    int ChargePeriodNumber,
    bool StartsNewChargePeriod,
    bool DailyCapReached
);