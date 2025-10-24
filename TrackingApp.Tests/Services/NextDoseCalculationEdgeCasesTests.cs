using Xunit;
using FluentAssertions;
using TrackingApp.Models;

namespace TrackingApp.Tests.Services;

public class NextDoseCalculationEdgeCasesTests
{
    [Fact]
    public void NoHistory_FirstDosePassed_ShouldCalculateCorrectNextDose()
    {
        // Arrange - Medicamento con primera dosis a las 8:00 AM, frecuencia 8h
        // Hora actual: 11:00 PM del mismo día
        var medication = new Medication
        {
            Name = "Ibuprofeno",
            Dose = "200mg",
            FrequencyHours = 8,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2025, 10, 20, 8, 0, 0) // 8:00 AM
        };
        
        var currentTime = new DateTime(2025, 10, 20, 23, 0, 0); // 11:00 PM mismo día
        
        // Act - Calcular próxima dosis usando lógica de app.js
        var nextDoseTime = CalculateNextDoseFromFirstDose(medication.FirstDoseTime, medication.TotalFrequencyInMinutes, currentTime);
        
        // Assert - Debería ser 12:00 AM del día siguiente (8am + 8h = 4pm, 4pm + 8h = 12am)
        nextDoseTime.Should().Be(new DateTime(2025, 10, 21, 0, 0, 0)); // 12:00 AM
    }
    
    [Fact]
    public void NoHistory_FirstDosePassed_Simeticona12_05AM_CurrentTime11PM()
    {
        // Arrange - Simeticona con primera dosis a las 12:05 AM, frecuencia 8h
        // Hora actual: 11:00 PM del mismo día
        var medication = new Medication
        {
            Name = "Simeticona",
            Dose = "0.5ml",
            FrequencyHours = 8,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2025, 10, 20, 0, 5, 0) // 12:05 AM
        };
        
        var currentTime = new DateTime(2025, 10, 20, 23, 0, 0); // 11:00 PM mismo día
        
        // Act
        var nextDoseTime = CalculateNextDoseFromFirstDose(medication.FirstDoseTime, medication.TotalFrequencyInMinutes, currentTime);
        
        // Assert - 12:05am → 8:05am → 4:05pm → PRÓXIMA debe ser 12:05am del día siguiente
        // Elapsed: 22h 55min = 1375 min
        // Frequency: 480 min
        // Doses elapsed: ceil(1375/480) = 3
        // Next: 12:05 + (3 * 8h) = 12:05 + 24h = 12:05 del día siguiente
        nextDoseTime.Should().Be(new DateTime(2025, 10, 21, 0, 5, 0)); // 12:05 AM siguiente día
    }
    
    [Fact]
    public void NoHistory_FirstDoseWasYesterday_ShouldCalculateFromElapsedDoses()
    {
        // Arrange - Primera dosis fue ayer a las 3:00 PM, frecuencia 6h
        // Hora actual: Hoy 10:00 AM
        var medication = new Medication
        {
            Name = "Paracetamol",
            Dose = "500mg",
            FrequencyHours = 6,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2025, 10, 19, 15, 0, 0) // Ayer 3:00 PM
        };
        
        var currentTime = new DateTime(2025, 10, 20, 10, 0, 0); // Hoy 10:00 AM
        
        // Act
        var nextDoseTime = CalculateNextDoseFromFirstDose(medication.FirstDoseTime, medication.TotalFrequencyInMinutes, currentTime);
        
        // Assert - Elapsed: 19 horas = 1140 min, Frequency: 360 min
        // Doses elapsed: ceil(1140/360) = 4
        // Next: 3pm ayer + (4 * 6h) = 3pm + 24h = 3pm hoy
        nextDoseTime.Should().Be(new DateTime(2025, 10, 20, 15, 0, 0)); // Hoy 3:00 PM
    }
    
    [Fact]
    public void NoHistory_FirstDoseIsInFuture_ShouldReturnFirstDose()
    {
        // Arrange - Primera dosis es en el futuro
        var medication = new Medication
        {
            Name = "Nueva Medicina",
            Dose = "10mg",
            FrequencyHours = 12,
            FrequencyMinutes = 0,
            FirstDoseTime = new DateTime(2025, 10, 21, 9, 0, 0) // Mañana 9:00 AM
        };
        
        var currentTime = new DateTime(2025, 10, 20, 14, 0, 0); // Hoy 2:00 PM
        
        // Act
        var nextDoseTime = CalculateNextDoseFromFirstDose(medication.FirstDoseTime, medication.TotalFrequencyInMinutes, currentTime);
        
        // Assert - La primera dosis aún no ha pasado, debe retornar la misma
        nextDoseTime.Should().Be(new DateTime(2025, 10, 21, 9, 0, 0));
    }
    
    [Fact]
    public void NoHistory_FirstDosePassedByMinutes_Every4h30min()
    {
        // Arrange - Primera dosis a las 10:00 AM, frecuencia 4h 30min
        // Hora actual: 2:45 PM (pasaron 4h 45min)
        var medication = new Medication
        {
            Name = "Medicina Bebé",
            Dose = "2ml",
            FrequencyHours = 4,
            FrequencyMinutes = 30,
            FirstDoseTime = new DateTime(2025, 10, 20, 10, 0, 0) // 10:00 AM
        };
        
        var currentTime = new DateTime(2025, 10, 20, 14, 45, 0); // 2:45 PM
        
        // Act
        var nextDoseTime = CalculateNextDoseFromFirstDose(medication.FirstDoseTime, medication.TotalFrequencyInMinutes, currentTime);
        
        // Assert - Elapsed: 4h 45min = 285 min, Frequency: 270 min (4h 30min)
        // Doses elapsed: ceil(285/270) = 2
        // Next: 10:00 + (2 * 4h30min) = 10:00 + 9h = 7:00 PM
        nextDoseTime.Should().Be(new DateTime(2025, 10, 20, 19, 0, 0)); // 7:00 PM
    }
    
    // Helper method que implementa la lógica de app.js
    private DateTime CalculateNextDoseFromFirstDose(DateTime firstDoseTime, int frequencyInMinutes, DateTime currentTime)
    {
        // Si la primera dosis es en el futuro, retornarla directamente
        if (firstDoseTime >= currentTime)
        {
            return firstDoseTime;
        }
        
        // Calcular cuánto tiempo ha pasado desde la primera dosis
        var elapsedMinutes = (currentTime - firstDoseTime).TotalMinutes;
        
        // Calcular cuántas dosis han transcurrido
        var dosesElapsed = Math.Ceiling(elapsedMinutes / frequencyInMinutes);
        
        // Calcular la próxima dosis
        var nextDoseTime = firstDoseTime.AddMinutes(dosesElapsed * frequencyInMinutes);
        
        return nextDoseTime;
    }
}
