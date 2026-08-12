using System;
using System.Globalization;
using TollFeeCalculator;
using TollFeeCalculator.Interfaces;
using TollFeeCalculator.Enums;
namespace TollFeeCalculator.Services
{
    public class TollCalculator
    {

        /**
         * Calculate the total toll fee for one day
         *
         * @param vehicle - the vehicle
         * @param dates   - date and time of all passes on one day
         * @return - the total toll fee for that day
         */

        // ** Adding some constants to represent time - Will be used in GetTollFee function **

        private const int SixOClock = 6 * 60;
        private const int SixThirty = 6 * 60 + 30;
        private const int SevenOClock = 7 * 60;
        private const int EightOClock = 8 * 60;
        private const int EightThirty = 8 * 60 + 30;
        private const int ThreePm = 15 * 60;
        private const int ThreeThirtyPm = 15 * 60 + 30;
        private const int FivePm = 17 * 60;
        private const int SixPm = 18 * 60;
        private const int SixThirtyPm = 18 * 60 + 30;

        private static readonly HashSet<DateOnly> TollFreeDates =
        [
            new(2013, 1, 1),
            new(2013, 3, 28),
            new(2013, 3, 29),
            new(2013, 4, 1),
            new(2013, 4, 30),
            new(2013, 5, 1),
            new(2013, 5, 8),
            new(2013, 5, 9),
            new(2013, 6, 5),
            new(2013, 6, 6),
            new(2013, 6, 21),
            new(2013, 11, 1),
            new(2013, 12, 24),
            new(2013, 12, 25),
            new(2013, 12, 26),
            new(2013, 12, 31)
        ];

        public int GetTollFee(IVehicle vehicle, DateTime[] dates)
        {
            DateTime intervalStart = dates[0];
            int totalFee = 0;
            foreach (DateTime date in dates)
            {
                int nextFee = GetTollFee(date, vehicle);
                int tempFee = GetTollFee(intervalStart, vehicle);

                long diffInMillies = date.Millisecond - intervalStart.Millisecond;
                long minutes = diffInMillies / 1000 / 60;

                if (minutes <= 60)
                {
                    if (totalFee > 0) totalFee -= tempFee;
                    if (nextFee >= tempFee) tempFee = nextFee;
                    totalFee += tempFee;
                }
                else
                {
                    totalFee += nextFee;
                }
            }
            if (totalFee > 60) totalFee = 60;
            return totalFee;
        }

        private bool IsTollFreeVehicle(IVehicle vehicle)
        {
            // ** Changing the implementation to use the VehicleType enum instead of string comparison **
            // ------------------------------------------------------------------------------------------
            // if (vehicle == null) return false;  <-- Don´t need this should not be able to be null
            // String vehicleType = vehicle.GetVehicleType();
            // // return vehicleType.Equals(TollFreeVehicles.Motorbike.ToString()) ||
            // //        vehicleType.Equals(TollFreeVehicles.Tractor.ToString()) ||
            // //        vehicleType.Equals(TollFreeVehicles.Emergency.ToString()) ||
            // //        vehicleType.Equals(TollFreeVehicles.Diplomat.ToString()) ||
            // //        vehicleType.Equals(TollFreeVehicles.Foreign.ToString()) ||
            // //        vehicleType.Equals(TollFreeVehicles.Military.ToString());
            // ------------------------------------------------------------------------------------------

            return vehicle.Type is
                VehicleType.Motorbike or
                VehicleType.Tractor or
                VehicleType.Emergency or
                VehicleType.Diplomat or
                VehicleType.Foreign or
                VehicleType.Military;
        }

        public int GetTollFee(DateTime date, IVehicle vehicle)
        {
            if (IsTollFreeDate(date) || IsTollFreeVehicle(vehicle)) return 0;

            // ** Changing to time in minutes that I will use in switch expression instead **
            //-----------------------
            // int hour = date.Hour;
            // int minute = date.Minute;
            //-----------------------



            int timeInMinutes = date.Hour * 60 + date.Minute;

            // ** Changing to switch expression**
            //-------------------------------------------------------------
            // if (hour == 6 && minute >= 0 && minute <= 29) return 8;
            // else if (hour == 6 && minute >= 30 && minute <= 59) return 13;
            // else if (hour == 7 && minute >= 0 && minute <= 59) return 18;
            // else if (hour == 8 && minute >= 0 && minute <= 29) return 13;
            // else if (hour >= 8 && hour <= 14 && minute >= 30 && minute <= 59) return 8;
            // else if (hour == 15 && minute >= 0 && minute <= 29) return 13;
            // else if (hour == 15 && minute >= 0 || hour == 16 && minute <= 59) return 18;
            // else if (hour == 17 && minute >= 0 && minute <= 59) return 13;
            // else if (hour == 18 && minute >= 0 && minute <= 29) return 8;
            // else return 0;
            //--------------------------------------------------------------

            return timeInMinutes switch
            {
                (>= SixOClock and < SixThirty)
                or (>= EightThirty and < ThreePm)
                or (>= SixPm and < SixThirtyPm)
                    => 8,

                (>= SixThirty and < SevenOClock)
                or (>= EightOClock and < EightThirty)
                or (>= ThreePm and < ThreeThirtyPm)
                or (>= FivePm and < SixPm)
                    => 13,

                (>= SevenOClock and < EightOClock)
                or (>= ThreeThirtyPm and < FivePm)
                    => 18,


                _ => 0

            };
        }

        private Boolean IsTollFreeDate(DateTime date)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) return true;

            if (year == 2013)
            {
                if (month == 1 && day == 1 ||
                    month == 3 && (day == 28 || day == 29) ||
                    month == 4 && (day == 1 || day == 30) ||
                    month == 5 && (day == 1 || day == 8 || day == 9) ||
                    month == 6 && (day == 5 || day == 6 || day == 21) ||
                    month == 7 ||
                    month == 11 && day == 1 ||
                    month == 12 && (day == 24 || day == 25 || day == 26 || day == 31))
                {
                    return true;
                }
            }
            return false;
        }

        // ** Dont need this, created another ENUM in VehicleType.cs ** 
        // private enum TollFreeVehicles
        // {
        //     Motorbike = 0,
        //     Tractor = 1,
        //     Emergency = 2,
        //     Diplomat = 3,
        //     Foreign = 4,
        //     Military = 5
        // }
    }
}