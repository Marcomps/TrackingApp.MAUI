using Xunit;
using TrackingApp.Helpers;

namespace TrackingApp.Tests.Helpers
{
    public class NumericParserTests
    {
        #region Tests for Bug Fix: Allow Dot and Comma in Numeric Fields

        [Theory]
        [InlineData("12.5", 12.5)]
        [InlineData("12,5", 12.5)]
        [InlineData("10", 10.0)]
        [InlineData("0.5", 0.5)]
        [InlineData("0,5", 0.5)]
        public void TryParseDouble_ShouldParseValidNumbers_WithDotOrComma(string input, double expected)
        {
            // Act
            bool success = NumericParser.TryParseDouble(input, out double result);

            // Assert
            Assert.True(success);
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("abc")]
        [InlineData("12.5.5")]
        public void TryParseDouble_ShouldReturnFalse_ForInvalidInputs(string? input)
        {
            // Act
            bool success = NumericParser.TryParseDouble(input!, out double result);

            // Assert
            Assert.False(success);
            Assert.Equal(0, result);
        }

        #endregion
    }
}