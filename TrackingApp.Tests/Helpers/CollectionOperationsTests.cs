using Xunit;
using FluentAssertions;
using TrackingApp.Models;
using System.Collections.ObjectModel;

namespace TrackingApp.Tests.Helpers;

public class CollectionOperationsTests
{
    [Fact]
    public void ObservableCollection_AddItem_IncreasesCount()
    {
        // Arrange
        var collection = new ObservableCollection<Medication>();
        var medication = new Medication { Name = "Test" };
        
        // Act
        collection.Add(medication);
        
        // Assert
        collection.Count.Should().Be(1);
        collection.Should().Contain(medication);
    }
    
    [Fact]
    public void ObservableCollection_RemoveItem_DecreasesCount()
    {
        // Arrange
        var medication = new Medication { Name = "Test" };
        var collection = new ObservableCollection<Medication> { medication };
        
        // Act
        collection.Remove(medication);
        
        // Assert
        collection.Count.Should().Be(0);
        collection.Should().NotContain(medication);
    }
    
    [Fact]
    public void ObservableCollection_Clear_RemovesAllItems()
    {
        // Arrange
        var collection = new ObservableCollection<Medication>
        {
            new Medication { Name = "Med1" },
            new Medication { Name = "Med2" },
            new Medication { Name = "Med3" }
        };
        
        // Act
        collection.Clear();
        
        // Assert
        collection.Count.Should().Be(0);
    }
    
    [Fact]
    public void List_GroupByUserType_GroupsCorrectly()
    {
        // Arrange
        var medications = new List<Medication>
        {
            new Medication { Name = "Med1", UserType = "Bebé" },
            new Medication { Name = "Med2", UserType = "Adulto" },
            new Medication { Name = "Med3", UserType = "Bebé" },
            new Medication { Name = "Med4", UserType = "Animal" }
        };
        
        // Act
        var grouped = medications.GroupBy(m => m.UserType).ToList();
        
        // Assert
        grouped.Count.Should().Be(3);
        grouped.First(g => g.Key == "Bebé").Count().Should().Be(2);
        grouped.First(g => g.Key == "Adulto").Count().Should().Be(1);
        grouped.First(g => g.Key == "Animal").Count().Should().Be(1);
    }
    
    [Fact]
    public void List_Distinct_RemovesDuplicates()
    {
        // Arrange
        var userTypes = new List<string> { "Bebé", "Adulto", "Bebé", "Animal", "Adulto" };
        
        // Act
        var distinct = userTypes.Distinct().ToList();
        
        // Assert
        distinct.Count.Should().Be(3);
        distinct.Should().Contain("Bebé").And.Contain("Adulto").And.Contain("Animal");
    }
    
    [Fact]
    public void List_Sum_CalculatesTotalAmount()
    {
        // Arrange
        var entries = new List<FoodEntry>
        {
            new FoodEntry { Amount = 100 },
            new FoodEntry { Amount = 150 },
            new FoodEntry { Amount = 200 }
        };
        
        // Act
        var total = entries.Sum(e => e.Amount);
        
        // Assert
        total.Should().Be(450);
    }
    
    [Fact]
    public void List_Average_CalculatesCorrectly()
    {
        // Arrange
        var entries = new List<FoodEntry>
        {
            new FoodEntry { Amount = 100 },
            new FoodEntry { Amount = 200 },
            new FoodEntry { Amount = 300 }
        };
        
        // Act
        var average = entries.Average(e => e.Amount);
        
        // Assert
        average.Should().Be(200);
    }
    
    [Fact]
    public void List_Count_WithPredicate_CountsMatchingItems()
    {
        // Arrange
        var doses = new List<MedicationDose>
        {
            new MedicationDose { IsConfirmed = true },
            new MedicationDose { IsConfirmed = false },
            new MedicationDose { IsConfirmed = true },
            new MedicationDose { IsConfirmed = true }
        };
        
        // Act
        var confirmedCount = doses.Count(d => d.IsConfirmed);
        
        // Assert
        confirmedCount.Should().Be(3);
    }
    
    [Fact]
    public void List_Any_ChecksForExistence()
    {
        // Arrange
        var medications = new List<Medication>
        {
            new Medication { Name = "Med1" },
            new Medication { Name = "Med2" }
        };
        
        // Act
        var hasItems = medications.Any();
        var hasMed1 = medications.Any(m => m.Name == "Med1");
        var hasMed3 = medications.Any(m => m.Name == "Med3");
        
        // Assert
        hasItems.Should().BeTrue();
        hasMed1.Should().BeTrue();
        hasMed3.Should().BeFalse();
    }
    
    [Fact]
    public void List_FirstOrDefault_ReturnsFirstMatchOrNull()
    {
        // Arrange
        var medications = new List<Medication>
        {
            new Medication { Id = 1, Name = "Med1" },
            new Medication { Id = 2, Name = "Med2" }
        };
        
        // Act
        var found = medications.FirstOrDefault(m => m.Id == 2);
        var notFound = medications.FirstOrDefault(m => m.Id == 99);
        
        // Assert
        found.Should().NotBeNull();
        found!.Name.Should().Be("Med2");
        notFound.Should().BeNull();
    }
}
