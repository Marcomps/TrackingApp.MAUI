using Xunit;
using FluentAssertions;
using TrackingApp.Models;

namespace TrackingApp.Tests.Services;

public class FilteringTests
{
    [Fact]
    public void FilterByUserType_Medications()
    {
        // Arrange
        var medications = new List<Medication>
        {
            new Medication { Name = "Med1", UserType = "Bebé" },
            new Medication { Name = "Med2", UserType = "Adulto" },
            new Medication { Name = "Med3", UserType = "Bebé" }
        };
        
        // Act
        var filtered = medications.Where(m => m.UserType == "Bebé").ToList();
        
        // Assert
        filtered.Count.Should().Be(2);
        filtered.All(m => m.UserType == "Bebé").Should().BeTrue();
    }
    
    [Fact]
    public void FilterByUserType_FoodEntries()
    {
        // Arrange
        var entries = new List<FoodEntry>
        {
            new FoodEntry { FoodType = "Leche", UserType = "Bebé" },
            new FoodEntry { FoodType = "Café", UserType = "Adulto" },
            new FoodEntry { FoodType = "Papilla", UserType = "Bebé" }
        };
        
        // Act
        var filtered = entries.Where(e => e.UserType == "Bebé").ToList();
        
        // Assert
        filtered.Count.Should().Be(2);
        filtered.All(e => e.UserType == "Bebé").Should().BeTrue();
    }
    
    [Fact]
    public void FilterByDateRange_FoodEntries()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 3);
        
        var entries = new List<FoodEntry>
        {
            new FoodEntry { FoodType = "Food1", Time = new DateTime(2023, 12, 31) },
            new FoodEntry { FoodType = "Food2", Time = new DateTime(2024, 1, 2) },
            new FoodEntry { FoodType = "Food3", Time = new DateTime(2024, 1, 5) }
        };
        
        // Act
        var filtered = entries.Where(e => e.Time >= startDate && e.Time <= endDate).ToList();
        
        // Assert
        filtered.Count.Should().Be(1);
        filtered[0].FoodType.Should().Be("Food2");
    }
    
    [Fact]
    public void FilterByDateRange_MedicationDoses()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2024, 1, 2);
        
        var doses = new List<MedicationDose>
        {
            new MedicationDose { ScheduledTime = new DateTime(2023, 12, 31, 10, 0, 0) },
            new MedicationDose { ScheduledTime = new DateTime(2024, 1, 1, 15, 0, 0) },
            new MedicationDose { ScheduledTime = new DateTime(2024, 1, 3, 8, 0, 0) }
        };
        
        // Act
        var filtered = doses.Where(d => d.ScheduledTime.Date >= startDate && d.ScheduledTime.Date <= endDate).ToList();
        
        // Assert
        filtered.Count.Should().Be(1);
    }
    
    [Fact]
    public void SortByTime_FoodEntries()
    {
        // Arrange
        var entries = new List<FoodEntry>
        {
            new FoodEntry { FoodType = "Food3", Time = new DateTime(2024, 1, 3) },
            new FoodEntry { FoodType = "Food1", Time = new DateTime(2024, 1, 1) },
            new FoodEntry { FoodType = "Food2", Time = new DateTime(2024, 1, 2) }
        };
        
        // Act
        var sorted = entries.OrderBy(e => e.Time).ToList();
        
        // Assert
        sorted[0].FoodType.Should().Be("Food1");
        sorted[1].FoodType.Should().Be("Food2");
        sorted[2].FoodType.Should().Be("Food3");
    }
    
    [Fact]
    public void SortByScheduledTime_MedicationDoses()
    {
        // Arrange
        var doses = new List<MedicationDose>
        {
            new MedicationDose { ScheduledTime = new DateTime(2024, 1, 1, 15, 0, 0) },
            new MedicationDose { ScheduledTime = new DateTime(2024, 1, 1, 8, 0, 0) },
            new MedicationDose { ScheduledTime = new DateTime(2024, 1, 1, 12, 0, 0) }
        };
        
        // Act
        var sorted = doses.OrderBy(d => d.ScheduledTime).ToList();
        
        // Assert
        sorted[0].ScheduledTime.Hour.Should().Be(8);
        sorted[1].ScheduledTime.Hour.Should().Be(12);
        sorted[2].ScheduledTime.Hour.Should().Be(15);
    }
    
    [Fact]
    public void SearchByName_Medications()
    {
        // Arrange
        var medications = new List<Medication>
        {
            new Medication { Name = "Ibuprofeno" },
            new Medication { Name = "Paracetamol" },
            new Medication { Name = "Aspirina" }
        };
        
        var searchTerm = "para";
        
        // Act
        var filtered = medications.Where(m => m.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        
        // Assert
        filtered.Count.Should().Be(1);
        filtered[0].Name.Should().Be("Paracetamol");
    }
    
    [Fact]
    public void SearchByFoodType_FoodEntries()
    {
        // Arrange
        var entries = new List<FoodEntry>
        {
            new FoodEntry { FoodType = "Leche materna" },
            new FoodEntry { FoodType = "Leche de fórmula" },
            new FoodEntry { FoodType = "Agua" }
        };
        
        var searchTerm = "leche";
        
        // Act
        var filtered = entries.Where(e => e.FoodType.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
        
        // Assert
        filtered.Count.Should().Be(2);
        filtered.All(e => e.FoodType.Contains("Leche")).Should().BeTrue();
    }
}
