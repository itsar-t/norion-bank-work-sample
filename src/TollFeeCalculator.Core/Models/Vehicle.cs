/*
    Right now there exists real models for Car and Motorbike, 
    but the API need to be able to create all types from the Enum VehicleType.
*/

using TollFeeCalculator.Enums;
using TollFeeCalculator.Interfaces;

namespace TollFeeCalculator.Models;

public sealed class Vehicle : IVehicle
{
    public VehicleType Type { get; }

    public Vehicle(VehicleType type)
    {
        Type = type;
    }
}

/*
    This makes the models Car and Motorbike kind of redundant but let's keep them for now.
*/