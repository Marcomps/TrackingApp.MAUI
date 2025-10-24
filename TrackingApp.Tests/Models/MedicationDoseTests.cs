using Xunit;
using FluentAssertions;
using TrackingApp.Models;

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
}
