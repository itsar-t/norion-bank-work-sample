public sealed record TollPassageResult(
    DateTime PassageTime,
    int PassageFee,
    int RunningTotal,
    int ChargePeriodNumber,
    bool StartsNewChargePeriod,
    bool DailyCapReached
);