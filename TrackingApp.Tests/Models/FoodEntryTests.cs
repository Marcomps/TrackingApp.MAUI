using Xunit;
using FluentAssertions;
using TrackingApp.Models;

namespace TrackingApp.Tests.Models;

public class FoodEntryTests
{
    [Fact]
    public void FoodEntry_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var entry = new FoodEntry();

        // Assert
        entry.Id.Should().Be(0);
        entry.FoodType.Should().Be(string.Empty);
        entry.Amount.Should().Be(0);
        entry.Unit.Should().Be(Unit.Gram);
        entry.UserType.Should().Be(string.Empty);
        entry.StartTime.Should().BeNull();
        entry.EndTime.Should().BeNull();
    }

    [Fact]
    public void FoodEntry_PropertiesSetCorrectly()
    {
        // Arrange & Act
        var entry = new FoodEntry
        {
            Id = 1,
            FoodType = "Leche materna",
            Amount = 120,
            Unit = Unit.Milliliter,
            Time = new DateTime(2024, 1, 1, 10, 0, 0),
            UserType = "Bebé"
        };

        // Assert
        entry.Id.Should().Be(1);
        entry.FoodType.Should().Be("Leche materna");
        entry.Amount.Should().Be(120);
        entry.Unit.Should().Be(Unit.Milliliter);
        entry.UserType.Should().Be("Bebé");
    }

    [Fact]
    public void DisplayAmount_FormatsCorrectly()
    {
        // Arrange
        var entry = new FoodEntry
        {
            Amount = 150,
            Unit = Unit.Milliliter
        };

        // Act
        var displayAmount = entry.DisplayAmount;

        // Assert
        displayAmount.Should().Be("150 ml");
    }

    [Fact]
    public void FormattedTime_FormatsCorrectly()
    {
        // Arrange
        var entry = new FoodEntry
        {
            Time = new DateTime(2024, 1, 1, 14, 30, 0)
        };

        // Act
        var formattedTime = entry.FormattedTime;

        // Assert
        formattedTime.Should().Contain("02:30");
    }

    [Fact]
    public void FormattedDate_FormatsCorrectly()
    {
        // Arrange
        var entry = new FoodEntry
        {
            Time = new DateTime(2024, 3, 15, 10, 0, 0)
        };

        // Act
        var formattedDate = entry.FormattedDate;

        // Assert
        formattedDate.Should().Be("15/03/2024");
    }

    [Fact]
    public void DurationText_ReturnsEmptyWhenNoStartEndTime()
    {
        // Arrange
        var entry = new FoodEntry
        {
            Time = DateTime.Now
        };

        // Act
        var durationText = entry.DurationText;

        // Assert
        durationText.Should().Be(string.Empty);
    }

    [Fact]
    public void DurationText_CalculatesCorrectly_WhenStartAndEndTimeSet()
    {
        // Arrange
        var entry = new FoodEntry
        {
            StartTime = new DateTime(2024, 1, 1, 10, 0, 0),
            EndTime = new DateTime(2024, 1, 1, 10, 25, 0)
        };

        // Act
        var durationText = entry.DurationText;

        // Assert
        durationText.Should().Be("25 min");
    }

    [Fact]
    public void TimeRangeText_ShowsTimeRange_WhenStartAndEndTimeSet()
    {
        // Arrange
        var entry = new FoodEntry
        {
            StartTime = new DateTime(2024, 1, 1, 10, 0, 0),
            EndTime = new DateTime(2024, 1, 1, 10, 30, 0),
            Time = new DateTime(2024, 1, 1, 10, 0, 0)
        };

        // Act
        var timeRangeText = entry.TimeRangeText;

        // Assert
        timeRangeText.Should().Contain("10:00").And.Contain("10:30");
    }

    [Fact]
    public void DisplayText_IncludesDuration_WhenStartAndEndTimeSet()
    {
        // Arrange
        var entry = new FoodEntry
        {
            Amount = 100,
            Unit = Unit.Milliliter,
            FoodType = "Leche",
            StartTime = new DateTime(2024, 1, 1, 10, 0, 0),
            EndTime = new DateTime(2024, 1, 1, 10, 20, 0)
        };

        // Act
        var displayText = entry.DisplayText;

        // Assert
        displayText.Should().Contain("100 ml").And.Contain("Leche").And.Contain("20 min");
    }
}