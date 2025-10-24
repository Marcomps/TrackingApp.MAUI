using Xunit;
using FluentAssertions;
using TrackingApp.Models;

namespace TrackingApp.Tests.Services;

public class DoseCalculationTests
{
    [Fact]
    public void CalculateNextDoseTime_AddsCorrectInterval()
    {
        // Arrange
        var medication = new Medication
        {
            FrequencyHours = 8,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2024, 1, 1, 8, 0, 0)
        };
        
        var lastDoseTime = new DateTime(2024, 1, 1, 8, 0, 0);
        var expectedNextDose = new DateTime(2024, 1, 1, 16, 0, 0);
        
        // Act
        var nextDoseTime = lastDoseTime.AddMinutes(medication.TotalFrequencyInMinutes);
        
        // Assert
        nextDoseTime.Should().Be(expectedNextDose);
    }
    
    [Fact]
    public void CalculateNextDoseTime_HandlesHoursAndMinutes()
    {
        // Arrange
        var medication = new Medication
        {
            FrequencyHours = 2,
            FrequencyMinutes = 30,
            FirstDoseTime = new DateTime(2024, 1, 1, 8, 0, 0)
        };
        
        var lastDoseTime = new DateTime(2024, 1, 1, 8, 0, 0);
        var expectedNextDose = new DateTime(2024, 1, 1, 10, 30, 0);
        
        // Act
        var nextDoseTime = lastDoseTime.AddMinutes(medication.TotalFrequencyInMinutes);
        
        // Assert
        nextDoseTime.Should().Be(expectedNextDose);
    }
    
    [Fact]
    public void TotalFrequencyInMinutes_ConvertsHoursCorrectly()
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
        totalMinutes.Should().Be(360);
    }
    
    [Fact]
    public void TotalFrequencyInMinutes_CombinesHoursAndMinutes()
    {
        // Arrange
        var medication = new Medication
        {
            FrequencyHours = 1,
            FrequencyMinutes = 45
        };
        
        // Act
        var totalMinutes = medication.TotalFrequencyInMinutes;
        
        // Assert
        totalMinutes.Should().Be(105);
    }
    
    [Fact]
    public void GenerateMultipleDoses_CreatesCorrectSchedule()
    {
        // Arrange
        var medication = new Medication
        {
            FrequencyHours = 4,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2024, 1, 1, 8, 0, 0)
        };
        
        var doses = new List<DateTime>();
        var currentTime = medication.FirstDoseTime;
        var frequencyInMinutes = medication.TotalFrequencyInMinutes;
        
        // Act - Generate 5 doses
        for (int i = 0; i < 5; i++)
        {
            doses.Add(currentTime);
            currentTime = currentTime.AddMinutes(frequencyInMinutes);
        }
        
        // Assert
        doses.Count.Should().Be(5);
        doses[0].Should().Be(new DateTime(2024, 1, 1, 8, 0, 0));
        doses[1].Should().Be(new DateTime(2024, 1, 1, 12, 0, 0));
        doses[2].Should().Be(new DateTime(2024, 1, 1, 16, 0, 0));
        doses[3].Should().Be(new DateTime(2024, 1, 1, 20, 0, 0));
        doses[4].Should().Be(new DateTime(2024, 1, 2, 0, 0, 0));
    }
    
    [Fact]
    public void Simeticona_FirstDose12_05AM_NextDose8_05AM()
    {
        // Arrange - Caso real: Simeticona cada 8 horas, primera dosis a las 12:05 AM
        var medication = new Medication
        {
            Name = "Simeticona",
            Dose = "0.5ml",
            FrequencyHours = 8,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2025, 10, 20, 0, 5, 0) // 12:05 AM
        };
        
        var doses = new List<DateTime>();
        var currentTime = medication.FirstDoseTime;
        var frequencyInMinutes = medication.TotalFrequencyInMinutes;
        
        // Act - Generate first 3 doses
        for (int i = 0; i < 3; i++)
        {
            doses.Add(currentTime);
            currentTime = currentTime.AddMinutes(frequencyInMinutes);
        }
        
        // Assert
        doses[0].Should().Be(new DateTime(2025, 10, 20, 0, 5, 0));  // 12:05 AM
        doses[1].Should().Be(new DateTime(2025, 10, 20, 8, 5, 0));  // 8:05 AM
        doses[2].Should().Be(new DateTime(2025, 10, 20, 16, 5, 0)); // 4:05 PM
        
        // Verify time components
        doses[0].Hour.Should().Be(0);
        doses[0].Minute.Should().Be(5);
        doses[1].Hour.Should().Be(8);
        doses[1].Minute.Should().Be(5);
        doses[2].Hour.Should().Be(16);
        doses[2].Minute.Should().Be(5);
    }
    
    [Fact]
    public void Ibuprofeno_FirstDose6_30AM_Every6Hours()
    {
        // Arrange - Ibuprofeno cada 6 horas, primera dosis a las 6:30 AM
        var medication = new Medication
        {
            Name = "Ibuprofeno",
            Dose = "200mg",
            FrequencyHours = 6,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2025, 10, 20, 6, 30, 0) // 6:30 AM
        };
        
        var doses = new List<DateTime>();
        var currentTime = medication.FirstDoseTime;
        var frequencyInMinutes = medication.TotalFrequencyInMinutes;
        
        // Act - Generate 4 doses in 24 hours
        for (int i = 0; i < 4; i++)
        {
            doses.Add(currentTime);
            currentTime = currentTime.AddMinutes(frequencyInMinutes);
        }
        
        // Assert - Should cycle through the day
        doses[0].Should().Be(new DateTime(2025, 10, 20, 6, 30, 0));  // 6:30 AM
        doses[1].Should().Be(new DateTime(2025, 10, 20, 12, 30, 0)); // 12:30 PM
        doses[2].Should().Be(new DateTime(2025, 10, 20, 18, 30, 0)); // 6:30 PM
        doses[3].Should().Be(new DateTime(2025, 10, 21, 0, 30, 0));  // 12:30 AM (next day)
    }
    
    [Fact]
    public void BabyMedicine_FirstDose3_15PM_Every4Hours30Minutes()
    {
        // Arrange - Medicina para bebé cada 4 horas 30 minutos, primera dosis a las 3:15 PM
        var medication = new Medication
        {
            Name = "Medicina Bebé",
            Dose = "2.5ml",
            FrequencyHours = 4,
            FrequencyMinutes = 30,
            FirstDoseTime = new DateTime(2025, 10, 20, 15, 15, 0) // 3:15 PM
        };
        
        var doses = new List<DateTime>();
        var currentTime = medication.FirstDoseTime;
        var frequencyInMinutes = medication.TotalFrequencyInMinutes;
        
        // Act - Generate 5 doses
        for (int i = 0; i < 5; i++)
        {
            doses.Add(currentTime);
            currentTime = currentTime.AddMinutes(frequencyInMinutes);
        }
        
        // Assert - Interval should be 270 minutes (4h 30min)
        medication.TotalFrequencyInMinutes.Should().Be(270);
        doses[0].Should().Be(new DateTime(2025, 10, 20, 15, 15, 0)); // 3:15 PM
        doses[1].Should().Be(new DateTime(2025, 10, 20, 19, 45, 0)); // 7:45 PM
        doses[2].Should().Be(new DateTime(2025, 10, 21, 0, 15, 0));  // 12:15 AM
        doses[3].Should().Be(new DateTime(2025, 10, 21, 4, 45, 0));  // 4:45 AM
        doses[4].Should().Be(new DateTime(2025, 10, 21, 9, 15, 0));  // 9:15 AM
    }
    
    [Fact]
    public void Paracetamol_FirstDose11_45PM_CrossesMidnight()
    {
        // Arrange - Paracetamol cada 8 horas, primera dosis cerca de medianoche
        var medication = new Medication
        {
            Name = "Paracetamol",
            Dose = "500mg",
            FrequencyHours = 8,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2025, 10, 20, 23, 45, 0) // 11:45 PM
        };
        
        var doses = new List<DateTime>();
        var currentTime = medication.FirstDoseTime;
        var frequencyInMinutes = medication.TotalFrequencyInMinutes;
        
        // Act - Generate 3 doses that cross midnight
        for (int i = 0; i < 3; i++)
        {
            doses.Add(currentTime);
            currentTime = currentTime.AddMinutes(frequencyInMinutes);
        }
        
        // Assert - Verify correct date transitions
        doses[0].Should().Be(new DateTime(2025, 10, 20, 23, 45, 0)); // 11:45 PM Oct 20
        doses[1].Should().Be(new DateTime(2025, 10, 21, 7, 45, 0));  // 7:45 AM Oct 21
        doses[2].Should().Be(new DateTime(2025, 10, 21, 15, 45, 0)); // 3:45 PM Oct 21
        
        // Verify days changed correctly
        doses[0].Day.Should().Be(20);
        doses[1].Day.Should().Be(21);
        doses[2].Day.Should().Be(21);
    }
    
    [Fact]
    public void VitaminD_FirstDose9_00AM_Every24Hours()
    {
        // Arrange - Vitamina D diaria, una vez al día a las 9:00 AM
        var medication = new Medication
        {
            Name = "Vitamina D",
            Dose = "400 UI",
            FrequencyHours = 24,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2025, 10, 20, 9, 0, 0) // 9:00 AM
        };
        
        var doses = new List<DateTime>();
        var currentTime = medication.FirstDoseTime;
        var frequencyInMinutes = medication.TotalFrequencyInMinutes;
        
        // Act - Generate 7 days of doses
        for (int i = 0; i < 7; i++)
        {
            doses.Add(currentTime);
            currentTime = currentTime.AddMinutes(frequencyInMinutes);
        }
        
        // Assert - Should be same time each day
        medication.TotalFrequencyInMinutes.Should().Be(1440); // 24 hours
        
        for (int i = 0; i < 7; i++)
        {
            doses[i].Hour.Should().Be(9);
            doses[i].Minute.Should().Be(0);
            doses[i].Day.Should().Be(20 + i);
        }
        
        // Verify specific dates
        doses[0].Should().Be(new DateTime(2025, 10, 20, 9, 0, 0)); // Oct 20
        doses[1].Should().Be(new DateTime(2025, 10, 21, 9, 0, 0)); // Oct 21
        doses[6].Should().Be(new DateTime(2025, 10, 26, 9, 0, 0)); // Oct 26
    }
    
    [Fact]
    public void CalculateNextDoseTime_HandlesDecimalDoses()
    {
        // Arrange - Test with decimal doses like 0.5 mg
        var medication = new Medication
        {
            Name = "Insulina",
            Dose = "0.5 mg",
            FrequencyHours = 4,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2024, 1, 1, 8, 0, 0)
        };
        
        var lastDoseTime = new DateTime(2024, 1, 1, 8, 0, 0);
        var expectedNextDose = new DateTime(2024, 1, 1, 12, 0, 0);
        
        // Act
        var nextDoseTime = lastDoseTime.AddMinutes(medication.TotalFrequencyInMinutes);
        
        // Assert
        nextDoseTime.Should().Be(expectedNextDose);
        medication.Dose.Should().Be("0.5 mg");
    }
    
    [Fact]
    public void CalculateNextDoseTime_HandlesFractionalDoses()
    {
        // Arrange - Test with fractional doses like 1.25 ml
        var medication = new Medication
        {
            Name = "Jarabe para la tos",
            Dose = "1.25 ml",
            FrequencyHours = 6,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2024, 1, 1, 9, 0, 0)
        };
        
        var lastDoseTime = new DateTime(2024, 1, 1, 9, 0, 0);
        var expectedNextDose = new DateTime(2024, 1, 1, 15, 0, 0);
        
        // Act
        var nextDoseTime = lastDoseTime.AddMinutes(medication.TotalFrequencyInMinutes);
        
        // Assert
        nextDoseTime.Should().Be(expectedNextDose);
        medication.Dose.Should().Be("1.25 ml");
    }
    
    [Fact]
    public void CalculateNextDoseTime_HandlesSmallDecimalDoses()
    {
        // Arrange - Test with very small decimal doses like 0.025 mg
        var medication = new Medication
        {
            Name = "Medicamento homeopático",
            Dose = "0.025 mg",
            FrequencyHours = 12,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2024, 1, 1, 8, 0, 0)
        };
        
        var lastDoseTime = new DateTime(2024, 1, 1, 8, 0, 0);
        var expectedNextDose = new DateTime(2024, 1, 1, 20, 0, 0);
        
        // Act
        var nextDoseTime = lastDoseTime.AddMinutes(medication.TotalFrequencyInMinutes);
        
        // Assert
        nextDoseTime.Should().Be(expectedNextDose);
        medication.Dose.Should().Be("0.025 mg");
    }
}
