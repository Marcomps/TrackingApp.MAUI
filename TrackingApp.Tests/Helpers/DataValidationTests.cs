using Xunit;
using FluentAssertions;
using TrackingApp.Models;

namespace TrackingApp.Tests.Helpers;

public class DataValidationTests
{
    [Fact]
    public void Medication_NameShouldNotBeEmpty()
    {
        // Arrange
        var medication = new Medication
        {
            Name = "Ibuprofeno",
            Dose = "200mg"
        };
        
        // Assert
        medication.Name.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public void Medication_FrequencyShouldBePositive()
    {
        // Arrange
        var medication = new Medication
        {
            FrequencyHours = 8,
            FrequencyMinutes = 0
        };
        
        // Assert
        medication.TotalFrequencyInMinutes.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public void FoodEntry_AmountShouldBePositive()
    {
        // Arrange
        var entry = new FoodEntry
        {
            Amount = 150
        };
        
        // Assert
        entry.Amount.Should().BeGreaterThan(0);
    }
    
    [Fact]
    public void FoodEntry_FoodTypeShouldNotBeEmpty()
    {
        // Arrange
        var entry = new FoodEntry
        {
            FoodType = "Leche"
        };
        
        // Assert
        entry.FoodType.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public void MedicationDose_ScheduledTimeShouldBeValid()
    {
        // Arrange
        var dose = new MedicationDose
        {
            ScheduledTime = new DateTime(2024, 1, 1, 10, 0, 0)
        };
        
        // Assert
        dose.ScheduledTime.Should().NotBe(default(DateTime));
        dose.ScheduledTime.Year.Should().BeGreaterThan(2000);
    }
    
    [Fact]
    public void MedicationDose_ActualTimeShouldBeAfterScheduledTime_WhenLate()
    {
        // Arrange
        var dose = new MedicationDose
        {
            ScheduledTime = new DateTime(2024, 1, 1, 10, 0, 0),
            ActualTime = new DateTime(2024, 1, 1, 10, 15, 0)
        };
        
        // Assert
        dose.ActualTime.Should().BeAfter(dose.ScheduledTime);
    }
    
    [Fact]
    public void Medication_TreatmentStartDateShouldBeValid()
    {
        // Arrange
        var medication = new Medication
        {
            TreatmentStartDate = new DateTime(2024, 1, 1)
        };
        
        // Assert
        medication.TreatmentStartDate.Should().NotBe(default(DateTime));
    }
    
    [Fact]
    public void Medication_TreatmentEndDateShouldBeAfterStartDate_WhenSet()
    {
        // Arrange
        var medication = new Medication
        {
            TreatmentStartDate = new DateTime(2024, 1, 1),
            TreatmentEndDate = new DateTime(2024, 1, 10)
        };
        
        // Assert
        medication.TreatmentEndDate.Should().BeAfter(medication.TreatmentStartDate);
    }
    
    [Fact]
    public void FoodEntry_EndTimeShouldBeAfterStartTime_WhenBothSet()
    {
        // Arrange
        var entry = new FoodEntry
        {
            StartTime = new DateTime(2024, 1, 1, 10, 0, 0),
            EndTime = new DateTime(2024, 1, 1, 10, 30, 0)
        };
        
        // Assert
        entry.EndTime.HasValue.Should().BeTrue();
        entry.EndTime.Value.Should().BeAfter(entry.StartTime.Value);
    }
    
    [Fact]
    public void Medication_UserTypeShouldBeValidOption()
    {
        // Arrange
        var validUserTypes = new[] { "Bebé", "Adulto", "Animal" };
        var medication = new Medication
        {
            UserType = "Bebé"
        };
        
        // Assert
        validUserTypes.Should().Contain(medication.UserType);
    }
}
