using TollFeeCalculator.Enums;
using TollFeeCalculator.Interfaces;
using TollFeeCalculator.Models;

namespace TollFeeCalculator.Services
{
    public class TollCalculator
    {
        private const int MinutesPerHour = 60;

        private const int SixOClock = 6 * MinutesPerHour;
        private const int SixThirty = 6 * MinutesPerHour + 30;
        private const int SevenOClock = 7 * MinutesPerHour;
        private const int EightOClock = 8 * MinutesPerHour;
        private const int EightThirty = 8 * MinutesPerHour + 30;
        private const int ThreePm = 15 * MinutesPerHour;
        private const int ThreeThirtyPm = 15 * MinutesPerHour + 30;
        private const int FivePm = 17 * MinutesPerHour;
        private const int SixPm = 18 * MinutesPerHour;
        private const int SixThirtyPm = 18 * MinutesPerHour + 30;

        // Maximum total toll fee in SEK for one vehicle and one day.
        private const int MaximumDailyFee = 60;

        // Duration in minutes for the single-charge period.
        private const int SingleChargePeriodMinutes =
            MinutesPerHour;

        /*
            A HashSet is used because the toll-free dates must be
            unique and are only checked for membership. Contains()
            has an average time complexity of O(1), compared with
            O(n) for a List.
         */
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

        /// <summary>
        /// Calculates the total toll fee for one vehicle and one day.
        /// </summary>
        /// <param name="vehicle">
        /// The vehicle passing the toll stations.
        /// </param>
        /// <param name="dates">
        /// The passage dates and times for one day.
        /// </param>
        /// <returns>The total toll fee in SEK.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="vehicle"/> or
        /// <paramref name="dates"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the passages occur on different calendar days.
        /// </exception>
        public int GetTollFee(
            IVehicle vehicle,
            DateTime[] dates)
        {
            return Calculate(vehicle, dates).TotalFee;
        }

        /// <summary>
        /// Calculates the total toll fee and detailed results
        /// for every passage.
        /// </summary>
        /// <param name="vehicle">
        /// The vehicle passing the toll stations.
        /// </param>
        /// <param name="dates">
        /// The passage dates and times for one day.
        /// </param>
        /// <returns>
        /// The total fee and the calculated fee progression
        /// for every passage.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="vehicle"/> or
        /// <paramref name="dates"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the passages occur on different calendar days.
        /// </exception>
        public TollCalculationResult Calculate(
            IVehicle vehicle,
            DateTime[] dates)
        {
            ArgumentNullException.ThrowIfNull(vehicle);
            ArgumentNullException.ThrowIfNull(dates);

            if (dates.Length == 0)
            {
                return new TollCalculationResult(0, []);
            }

            DateTime[] sortedDates = dates
                .OrderBy(date => date)
                .ToArray();

            DateTime firstPassageDate = sortedDates[0];

            /*
                The calculation applies to passages from one calendar
                day. Invalid input is rejected before any toll
                calculation begins.
             */
            bool containsMultipleDates = sortedDates
                .Any(date =>
                    date.Date != firstPassageDate.Date);

            if (containsMultipleDates)
            {
                throw new ArgumentException(
                    "All passages must occur on the same day.",
                    nameof(dates)
                );
            }

            List<TollPassageResult> passageResults = [];

            DateTime intervalStart = firstPassageDate;

            int highestFeeInInterval =
                GetTollFee(intervalStart, vehicle);

            int completedIntervalsFee = 0;

            passageResults.Add(
                new TollPassageResult(
                    intervalStart,
                    highestFeeInInterval,
                    Math.Min(
                        highestFeeInInterval,
                        MaximumDailyFee
                    )
                )
            );

            foreach (DateTime date in sortedDates.Skip(1))
            {
                int passageFee =
                    GetTollFee(date, vehicle);

                double minutesSinceIntervalStart =
                    (date - intervalStart).TotalMinutes;

                if (minutesSinceIntervalStart <=
                    SingleChargePeriodMinutes)
                {
                    highestFeeInInterval = Math.Max(
                        highestFeeInInterval,
                        passageFee
                    );
                }
                else
                {
                    completedIntervalsFee +=
                        highestFeeInInterval;

                    intervalStart = date;
                    highestFeeInInterval = passageFee;
                }

                int runningTotal = Math.Min(
                    completedIntervalsFee +
                    highestFeeInInterval,
                    MaximumDailyFee
                );

                passageResults.Add(
                    new TollPassageResult(
                        date,
                        passageFee,
                        runningTotal
                    )
                );
            }

            int totalFee = Math.Min(
                completedIntervalsFee +
                highestFeeInInterval,
                MaximumDailyFee
            );

            return new TollCalculationResult(
                totalFee,
                passageResults
            );
        }

        /// <summary>
        /// Determines whether a vehicle is exempt from toll fees.
        /// </summary>
        /// <param name="vehicle">The vehicle to check.</param>
        /// <returns>
        /// <see langword="true"/> if the vehicle is toll-free;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        private bool IsTollFreeVehicle(IVehicle vehicle)
        {
            return vehicle.Type is
                VehicleType.Motorbike or
                VehicleType.Tractor or
                VehicleType.Emergency or
                VehicleType.Diplomat or
                VehicleType.Foreign or
                VehicleType.Military;
        }

        /// <summary>
        /// Calculates the toll fee for a single passage.
        /// </summary>
        /// <param name="date">
        /// The date and time of the passage.
        /// </param>
        /// <param name="vehicle">
        /// The vehicle passing the toll station.
        /// </param>
        /// <returns>The toll fee for the passage in SEK.</returns>
        public int GetTollFee(
            DateTime date,
            IVehicle vehicle)
        {
            if (IsTollFreeDate(date) ||
                IsTollFreeVehicle(vehicle))
            {
                return 0;
            }

            int timeInMinutes =
                date.Hour * MinutesPerHour + date.Minute;

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

        /// <summary>
        /// Determines whether a date is exempt from toll fees.
        /// </summary>
        /// <param name="date">The date and time to check.</param>
        /// <returns>
        /// <see langword="true"/> if the date is toll-free;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        private static bool IsTollFreeDate(DateTime date)
        {
            bool isWeekend = date.DayOfWeek is
                DayOfWeek.Saturday or
                DayOfWeek.Sunday;

            bool isTollFreeJuly =
                date.Year == 2013 &&
                date.Month == 7;

            bool isTollFreeDate =
                TollFreeDates.Contains(
                    DateOnly.FromDateTime(date)
                );

            return isWeekend ||
                   isTollFreeJuly ||
                   isTollFreeDate;
        }
    }
}