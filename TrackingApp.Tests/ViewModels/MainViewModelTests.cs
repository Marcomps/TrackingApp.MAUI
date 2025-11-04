using Xunit;
using FluentAssertions;
using System.Globalization;

namespace TrackingApp.Tests.Helpers;

public class DecimalInputValidationTests
{
    [Theory]
    [InlineData("1.5", true, 1.5)]
    [InlineData("2.0", true, 2.0)]
    [InlineData("10.25", true, 10.25)]
    [InlineData("0.5", true, 0.5)]
    [InlineData("100.99", true, 100.99)]
    [InlineData("1.8", true, 1.8)]  // Real-world example from user requirements
    public void DecimalInput_WithPeriodSeparator_ParsesCorrectly(string input, bool expectedSuccess, double expectedValue)
    {
        // Arrange & Act
        var result = double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedValue);

        // Assert
        result.Should().Be(expectedSuccess);
        if (expectedSuccess)
        {
            parsedValue.Should().BeApproximately(expectedValue, 0.001);
        }
    }

    [Theory]
    [InlineData("1,5")]  // Comma separator should not parse with InvariantCulture
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("10.25.5")]
    [InlineData("2,5")]  // Another comma test
    public void DecimalInput_WithInvalidFormat_FailsToParse(string input)
    {
        // Arrange & Act
        var result = double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsedValue);

        // Assert
        result.Should().BeFalse();
        parsedValue.Should().Be(0);
    }

    [Theory]
    [InlineData("1.8")]
    [InlineData("2.5")]
    [InlineData("120.5")]
    [InlineData("0.75")]
    public void FoodAmount_WithDecimalInput_AcceptsPeriodSeparator(string amountInput)
    {
        // Arrange & Act
        var result = double.TryParse(amountInput, NumberStyles.Float, CultureInfo.InvariantCulture, out double amount);

        // Assert
        result.Should().BeTrue("because {0} should be a valid decimal", amountInput);
        amount.Should().BeGreaterThan(0, "because amount must be positive");
    }

    [Fact]
    public void InvariantCulture_EnsuresPeriodAsDecimalSeparator()
    {
        // Arrange
        var culture = CultureInfo.InvariantCulture;

        // Assert
        culture.NumberFormat.NumberDecimalSeparator.Should().Be(".");
        culture.NumberFormat.NumberGroupSeparator.Should().Be(",");
    }

    [Theory]
    [InlineData("3.14159", 3.14159)]
    [InlineData("0.001", 0.001)]
    [InlineData("999.999", 999.999)]
    public void DecimalInput_WithHighPrecision_ParsesAccurately(string input, double expected)
    {
        // Arrange & Act
        var result = double.TryParse(input, NumberStyles.Float, CultureInfo.InvariantCulture, out double value);

        // Assert
        result.Should().BeTrue();
        value.Should().BeApproximately(expected, 0.000001);
    }
}
