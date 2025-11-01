using Xunit;
using FluentAssertions;
using TrackingApp.Models;
using Moq;
using System.Threading.Tasks;
using TrackingApp.Services;

namespace TrackingApp.Tests.Models;

public class MedicationDoseTests
{
    [Fact]
    public void MedicationDose_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var dose = new MedicationDose();

        // Assert
        dose.Id.Should().Be(0);
        dose.MedicationId.Should().Be(0);
        dose.IsConfirmed.Should().BeFalse();
        dose.IsEdited.Should().BeFalse();
        dose.ActualTime.Should().BeNull();
        dose.Medication.Should().BeNull();
    }

    [Fact]
    public void MedicationDose_PropertiesSetCorrectly()
    {
        // Arrange & Act
        var dose = new MedicationDose
        {
            Id = 1,
            MedicationId = 5,
            ScheduledTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ActualTime = new DateTime(2024, 1, 1, 10, 5, 0),
            IsConfirmed = true,
            IsEdited = false
        };

        // Assert
        dose.Id.Should().Be(1);
        dose.MedicationId.Should().Be(5);
        dose.IsConfirmed.Should().BeTrue();
        dose.IsEdited.Should().BeFalse();
        dose.ActualTime.HasValue.Should().BeTrue();
    }

    [Fact]
    public void Status_ReturnsConfirmed_WhenIsConfirmedTrue()
    {
        // Arrange
        var dose = new MedicationDose
        {
            ScheduledTime = DateTime.Now,
            IsConfirmed = true
        };

        // Act
        var status = dose.Status;

        // Assert
        status.Should().Be("Confirmado");
    }

    [Fact]
    public void Status_ReturnsAtrasado_WhenMoreThan30MinutesLate()
    {
        // Arrange
        var dose = new MedicationDose
        {
            ScheduledTime = DateTime.Now.AddMinutes(-60),
            IsConfirmed = false
        };

        // Act
        var status = dose.Status;

        // Assert
        status.Should().Be("Atrasado");
    }

    [Fact]
    public void Status_ReturnsProximo_WhenLessThan30MinutesAway()
    {
        // Arrange
        var dose = new MedicationDose
        {
            ScheduledTime = DateTime.Now.AddMinutes(15),
            IsConfirmed = false
        };

        // Act
        var status = dose.Status;

        // Assert
        status.Should().Be("Próximo");
    }

    [Fact]
    public void Status_ReturnsProgramado_WhenMoreThan30MinutesAway()
    {
        // Arrange
        var dose = new MedicationDose
        {
            ScheduledTime = DateTime.Now.AddMinutes(90),
            IsConfirmed = false
        };

        // Act
        var status = dose.Status;

        // Assert
        status.Should().Be("Programado");
    }

    [Fact]
    public void DisplayTime_UsesActualTime_WhenAvailable()
    {
        // Arrange
        var actualTime = new DateTime(2024, 1, 1, 15, 30, 0);
        var dose = new MedicationDose
        {
            ScheduledTime = new DateTime(2024, 1, 1, 15, 0, 0),
            ActualTime = actualTime
        };

        // Act
        var displayTime = dose.DisplayTime;

        // Assert
        displayTime.Should().Contain("03:30");
    }

    [Fact]
    public void DisplayTime_UsesScheduledTime_WhenActualTimeNotAvailable()
    {
        // Arrange
        var scheduledTime = new DateTime(2024, 1, 1, 14, 0, 0);
        var dose = new MedicationDose
        {
            ScheduledTime = scheduledTime,
            ActualTime = null
        };

        // Act
        var displayTime = dose.DisplayTime;

        // Assert
        displayTime.Should().Contain("02:00");
    }

    [Fact]
    public void DisplayText_IncludesMedicationInfo_WhenMedicationSet()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Ibuprofeno",
            Dose = "200mg"
        };

        var dose = new MedicationDose
        {
            ScheduledTime = new DateTime(2024, 1, 1, 10, 0, 0),
            Medication = medication
        };

        // Act
        var displayText = dose.DisplayText;

        // Assert
        displayText.Should().Contain("Ibuprofeno").And.Contain("200mg");
    }

    [Fact]
    public void GetNextDoses_ShouldReturnCorrectDoses_ForGivenFrequencyAndDays()
    {
        // Arrange
        var scheduledTime = new DateTime(2025, 10, 24, 3, 0, 0); // 3:00 AM
        var medicationDose = new MedicationDose
        {
            ScheduledTime = scheduledTime
        };

        int frequencyInHours = 6;
        int days = 1;

        // Act
        var nextDoses = medicationDose.GetNextDoses(frequencyInHours, days).ToList();

        // Assert
        nextDoses.Should().NotBeEmpty();
        nextDoses.Should().HaveCount(4); // 4 doses in 24 hours: 9 AM, 3 PM, 9 PM, 3 AM (next day)   
        nextDoses[0].Should().Be(new DateTime(2025, 10, 24, 9, 0, 0));
        nextDoses[1].Should().Be(new DateTime(2025, 10, 24, 15, 0, 0));
        nextDoses[2].Should().Be(new DateTime(2025, 10, 24, 21, 0, 0));
        nextDoses[3].Should().Be(new DateTime(2025, 10, 25, 3, 0, 0));
    }

    [Fact]
    public async Task GetNextDoses_ShouldQueryDatabaseAndCalculateNextDoses()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();
        var lastConfirmedDose = new MedicationDose
        {
            ScheduledTime = new DateTime(2025, 10, 24, 3, 0, 0), // 3:00 AM
            ActualTime = new DateTime(2025, 10, 24, 3, 0, 0),
            IsConfirmed = true
        };

        mockDataService.Setup(ds => ds.GetLastConfirmedDoseAsync(It.IsAny<int>()))
            .ReturnsAsync(lastConfirmedDose);

        var medicationDose = new MedicationDose();
        int medicationId = 1;
        int frequencyInHours = 6;
        int days = 1;

        // Act
        var nextDoses = (await medicationDose.GetNextDosesFromDatabaseAsync(mockDataService.Object, medicationId, frequencyInHours, days)).ToList();

        // Assert
        nextDoses.Should().NotBeEmpty();
        nextDoses.Should().HaveCount(4); // 4 doses in 24 hours: 9 AM, 3 PM, 9 PM, 3 AM (next day)   
        nextDoses[0].Should().Be(new DateTime(2025, 10, 24, 9, 0, 0));
        nextDoses[1].Should().Be(new DateTime(2025, 10, 24, 15, 0, 0));
        nextDoses[2].Should().Be(new DateTime(2025, 10, 24, 21, 0, 0));
        nextDoses[3].Should().Be(new DateTime(2025, 10, 25, 3, 0, 0));
    }

    [Fact]
    public async Task GetNextDoses_ShouldHandleNoConfirmedDosesInDatabase()
    {
        // Arrange
        var mockDataService = new Mock<IDataService>();

        mockDataService.Setup(ds => ds.GetLastConfirmedDoseAsync(It.IsAny<int>()))
            .ReturnsAsync(() => null);

        var medicationDose = new MedicationDose();
        int medicationId = 1;
        int frequencyInHours = 6;
        int days = 1;

        // Act
        var nextDoses = await medicationDose.GetNextDosesFromDatabaseAsync(mockDataService.Object, medicationId, frequencyInHours, days);
        // Assert
        nextDoses.Should().BeEmpty();
    }
}