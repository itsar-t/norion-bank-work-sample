using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TollFeeCalculator.Enums;

namespace TollFeeCalculator.Interfaces
{
    public interface IVehicle
    {
        VehicleType Type { get; }
    }
}