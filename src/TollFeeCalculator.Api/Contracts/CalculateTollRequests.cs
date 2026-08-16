using TollFeeCalculator.Enums;

namespace TollFeeCalculator.Api.Contracts;

/* The recor CalculateTollRequest describes what kind off JSON that frontend will pass to it */

public sealed record CalculateTollRequest(
    VehicleType VehicleType,
    DateTime[] Passages
);

/*
Example:

{
  "vehicleType": "Car",
  "passages": [
    "2013-01-02T06:10:00",
    "2013-01-02T06:40:00",
    "2013-01-02T07:05:00"
  ]
}

*/