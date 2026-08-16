using System;
using System.Globalization;
using TollFeeCalculator;
using TollFeeCalculator.Interfaces;
using TollFeeCalculator.Enums;
using System.Linq;
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

        // To be used in GetTollFee function
        private const int MaximumDailyFee = 60;
        private const int SingleChargePeriodMinutes = 60;

        // ** Adding Hashset to be used in IsTollFreeDate(DateTime date) **

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
            // If vehicle or dates is null -> Exception
            ArgumentNullException.ThrowIfNull(vehicle);
            ArgumentNullException.ThrowIfNull(dates);

            if (dates.Length == 0)
            {
                return 0;
            }

            // Make sure earliest time first
            DateTime[] sortedDates = dates
                .OrderBy(date => date)
                .ToArray();

            DateTime intervalStart = sortedDates[0];

            /*
                The original implementation assumes that all passages occur on the same day,
                but it does not validate this requirement. The Core layer throws an
                ArgumentException when passages have different dates, while the API returns
                HTTP 400 for invalid client input. A built-in exception is sufficient because
                a custom exception would not add meaningful information here.
            */

            bool containsMultipleDates = sortedDates
                .Any(date => date.Date != intervalStart.Date);

            if (containsMultipleDates)
            {
                throw new ArgumentException(
                    "All passages must occur on the same day",
                    nameof(dates));
            }

            int highestFeeInterval = GetTollFee(intervalStart, vehicle);
            int totalFee = 0;
            
            // First intervall is already handled so skip it

            foreach (DateTime date in sortedDates.Skip(1))
            {
                int currentFee = GetTollFee(date, vehicle);

                double minutesSinceIntervalStart =
                    (date - intervalStart).TotalMinutes;

                // If fee changed inside of 60 minute time period totalFee should not increase but highest Fee during this time should in the end be added to totalFee

                if (minutesSinceIntervalStart <= SingleChargePeriodMinutes)
                {
                    highestFeeInterval =
                        Math.Max(highestFeeInterval, currentFee);
                }
                else
                {
                    // add the Fee when interval is more then 60 min
                    totalFee += highestFeeInterval;

                    //Set the new time to compare nextFee with, to first time that occurs after 60 min
                    intervalStart = date;
                    
                    //This time should also have a new higestFeeInterval which is the currentFee of current time 
                    highestFeeInterval = currentFee;
                }
            }

            totalFee += highestFeeInterval;

            //If totalFee is more than 60:- then no more fee should be added 

            return Math.Min(totalFee, MaximumDailyFee);
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

        private static bool IsTollFreeDate(DateTime date)
        {
            bool isWeekend = date.DayOfWeek is
                DayOfWeek.Saturday or
                DayOfWeek.Sunday;

            bool isTollFreeJuly =
                date.Year == 2013 &&
                date.Month == 7;

            // ** Making it less messy**
            //-------------------------------------------------------------------
            // if (year == 2013)
            // {
            //     if (month == 1 && day == 1 ||
            //         month == 3 && (day == 28 || day == 29) ||
            //         month == 4 && (day == 1 || day == 30) ||
            //         month == 5 && (day == 1 || day == 8 || day == 9) ||
            //         month == 6 && (day == 5 || day == 6 || day == 21) ||
            //         month == 7 ||
            //         month == 11 && day == 1 ||
            //         month == 12 && (day == 24 || day == 25 || day == 26 || day == 31))
            //     {
            //         return true;
            //     }
            // }
            // return false;
            //---------------------------------------------------------------------

            bool isTollfreeDate =
                TollFreeDates.Contains(DateOnly.FromDateTime(date));

            return isWeekend || isTollFreeJuly || isTollfreeDate;

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