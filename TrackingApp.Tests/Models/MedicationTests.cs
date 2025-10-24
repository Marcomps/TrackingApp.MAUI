using Xunit;
using FluentAssertions;
using TrackingApp.Models;

namespace TrackingApp.Tests.Models;

public class MedicationTests
{
    [Fact]
    public void Medication_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var medication = new Medication();
        
        // Assert
        medication.Id.Should().Be(0);
        medication.Name.Should().Be(string.Empty);
        medication.Dose.Should().Be(string.Empty);
        medication.FrequencyHours.Should().Be(0);
        medication.FrequencyMinutes.Should().Be(0);
        medication.UserType.Should().Be(string.Empty);
    }
    
    [Fact]
    public void Medication_PropertiesSetCorrectly()
    {
        // Arrange & Act
        var medication = new Medication
        {
            Id = 1,
            Name = "Ibuprofeno",
            Dose = "200mg",
            FrequencyHours = 8,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2024, 1, 1, 9, 0, 0),
            UserType = "Adulto",
            TreatmentStartDate = new DateTime(2024, 1, 1),
            TreatmentEndDate = new DateTime(2024, 1, 10)
        };
        
        // Assert
        medication.Id.Should().Be(1);
        medication.Name.Should().Be("Ibuprofeno");
        medication.Dose.Should().Be("200mg");
        medication.FrequencyHours.Should().Be(8);
        medication.FrequencyMinutes.Should().Be(0);
        medication.UserType.Should().Be("Adulto");
    }
    
    [Fact]
    public void TotalFrequencyInMinutes_CalculatesCorrectly_OnlyHours()
    {
        // Arrange
        var medication = new Medication
        {
            FrequencyHours = 6,
            FrequencyMinutes = 0
        };
        
        // Act
        var totalMinutes = medication.TotalFrequencyInMinutes;
        
        // Assert
        totalMinutes.Should().Be(360); // 6 hours * 60
    }
    
    [Fact]
    public void TotalFrequencyInMinutes_CalculatesCorrectly_HoursAndMinutes()
    {
        // Arrange
        var medication = new Medication
        {
            FrequencyHours = 2,
            FrequencyMinutes = 30
        };
        
        // Act
        var totalMinutes = medication.TotalFrequencyInMinutes;
        
        // Assert
        totalMinutes.Should().Be(150); // 2*60 + 30 = 150
    }
    
    [Fact]
    public void TotalFrequencyInMinutes_CalculatesCorrectly_OnlyMinutes()
    {
        // Arrange
        var medication = new Medication
        {
            FrequencyHours = 0,
            FrequencyMinutes = 45
        };
        
        // Act
        var totalMinutes = medication.TotalFrequencyInMinutes;
        
        // Assert
        totalMinutes.Should().Be(45);
    }
    
    [Fact]
    public void DisplayText_FormatsCorrectly_WithHoursAndMinutes()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Paracetamol",
            Dose = "500mg",
            FrequencyHours = 4,
            FrequencyMinutes = 30
        };
        
        // Act
        var displayText = medication.DisplayText;
        
        // Assert
        displayText.Should().Be("Paracetamol (500mg) cada 4h 30min");
    }
    
    [Fact]
    public void DisplayText_FormatsCorrectly_WithOnlyHours()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Aspirina",
            Dose = "100mg",
            FrequencyHours = 12,
            FrequencyMinutes = 0
        };
        
        // Act
        var displayText = medication.DisplayText;
        
        // Assert
        displayText.Should().Be("Aspirina (100mg) cada 12h");
    }
    
    [Fact]
    public void DisplayText_FormatsCorrectly_WithOnlyMinutes()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Gotas",
            Dose = "5ml",
            FrequencyHours = 0,
            FrequencyMinutes = 30
        };
        
        // Act
        var displayText = medication.DisplayText;
        
        // Assert
        displayText.Should().Be("Gotas (5ml) cada 30min");
    }
}
