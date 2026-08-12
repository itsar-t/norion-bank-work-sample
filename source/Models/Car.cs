using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TollFeeCalculator.Interfaces;
using TollFeeCalculator.Enums;

namespace TollFeeCalculator.Models
{
    public class Car : IVehicle
    {
        public VehicleType Type => VehicleType.Car;
    }
}