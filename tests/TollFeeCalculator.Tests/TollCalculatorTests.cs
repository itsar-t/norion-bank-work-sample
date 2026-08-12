using TollFeeCalculator.Models;
using TollFeeCalculator.Services;

namespace TollFeeCalculator.Tests
{

    public class TollCalculatorTests
    {
        private readonly TollCalculator _calculator = new();
        private readonly Car _car = new();

        [Fact]
        public void GetTollFee_WithEmptyPassages_ReturnsZero()
        {
            int result = _calculator.GetTollFee(_car, []);
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetTollFell_WithPassagesWithinSixtyMinutes_ReturnsHighestFee()
        {
            DateTime[] passages =
            [
                new(2013, 1, 2, 6, 10, 0),
                new(2013, 1, 2, 6, 40, 0),
                new(2013, 1, 2, 7, 5, 0),

            ];

            int result = _calculator.GetTollFee(_car, passages);
            Assert.Equal(18, result);
        }

        [Fact]
        public void GetTollFee_WithMultipleChargePeriods_ReturnsTheirCombinedFees()
        {
            DateTime[] passages =
            [
                new(2013, 1, 2, 6, 10, 0),
                new(2013, 1, 2, 6, 40, 0),
                new(2013, 1, 2, 7, 11, 0)
            ];

            int result = _calculator.GetTollFee(_car, passages);

            Assert.Equal(31, result);
        }

        [Fact]
        public void GetTollFee_WithUnsortedPassages_CalculatesChronologically()
        {
            DateTime[] passages =
            [
                new(2013, 1, 2, 7, 5, 0),
                new(2013, 1, 2, 6, 10, 0),
                new(2013, 1, 2, 6, 40, 0)
            ];

            int result = _calculator.GetTollFee(_car, passages);

            Assert.Equal(18, result);
        }


        [Fact]
        public void GetTollFee_WhenTotalExceedsMaximum_ReturnsMaximumDailyFee()
        {
            DateTime[] passages =
            [
                new(2013, 1, 2, 6, 0, 0),
                new(2013, 1, 2, 7, 1, 0),
                new(2013, 1, 2, 8, 2, 0),
                new(2013, 1, 2, 9, 3, 0),
                new(2013, 1, 2, 15, 4, 0),
                new(2013, 1, 2, 16, 5, 0)
            ];

            int result = _calculator.GetTollFee(_car, passages);

            Assert.Equal(60, result);
        }

        [Fact]
        public void GetTollFee_ForMotorbike_ReturnsZero()
        {
            Motorbike motorbike = new();

            int result = _calculator.GetTollFee(
                motorbike,
                [new DateTime(2013, 1, 2, 7, 30, 0)]);

            Assert.Equal(0, result);
        }

        [Theory]
        [InlineData(2013, 1, 1)]
        [InlineData(2013, 1, 5)]
        [InlineData(2013, 7, 1)]
        [InlineData(2013, 12, 25)]
        public void GetTollFee_OnTollFreeDate_ReturnsZero(
            int year,
            int month,
            int day)
        {
            DateTime passage = new(year, month, day, 7, 30, 0);

            int result = _calculator.GetTollFee(_car, [passage]);

            Assert.Equal(0, result);
        }
        
        [Theory]
        [InlineData(6, 0, 8)]
        [InlineData(6, 30, 13)]
        [InlineData(7, 0, 18)]
        [InlineData(8, 0, 13)]
        [InlineData(8, 30, 8)]
        [InlineData(15, 0, 13)]
        [InlineData(15, 30, 18)]
        [InlineData(17, 0, 13)]
        [InlineData(18, 0, 8)]
        [InlineData(18, 30, 0)]
        public void GetTollFee_AtTimeBoundary_ReturnsExpectedFee(
            int hour,
            int minute,
            int expectedFee)
        {
            DateTime passage = new(2013, 1, 2, hour, minute, 0);

            int result = _calculator.GetTollFee(passage, _car);

            Assert.Equal(expectedFee, result);
        }
    }
}