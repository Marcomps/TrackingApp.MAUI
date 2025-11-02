using Xunit;
using FluentAssertions;
using TrackingApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace TrackingApp.Tests.Services;

public class DoseCalculationTests
{
    [Fact]
    public void ProductionLogic_ShouldMatchGetNextDoses_With6HourFrequencyAnd1Day()
    {
        // Este test verifica que la lógica esperada en producción coincide con GetNextDoses
        // El método RecalculateNextDosesFromLastConfirmedAsync en DataService.cs debe:
        // - Calcular desde ActualTime (o FirstDoseTime si no hay confirmadas)
        // - Agregar frecuencia (en minutos) iterativamente
        // - Generar dosis hasta endDate (DateTime.Now + days)
        
        // Arrange - Simular comportamiento de producción
        var lastConfirmedTime = new DateTime(2025, 10, 24, 3, 0, 0); // 3:00 AM
        int frequencyInHours = 6;
        int frequencyInMinutes = frequencyInHours * 60; // 360 minutos
        int days = 1;
        
        // Simular el cálculo de producción (debe usar <= para incluir la dosis en el límite exacto)
        var calculatedDoses = new System.Collections.Generic.List<DateTime>();
        var endDate = lastConfirmedTime.AddDays(days); // 2025-10-25 03:00:00
        var currentDose = lastConfirmedTime.AddMinutes(frequencyInMinutes); // Primera dosis después de la confirmada
        
        while (currentDose <= endDate)
        {
            calculatedDoses.Add(currentDose);
            currentDose = currentDose.AddMinutes(frequencyInMinutes);
        }
        
        // Act - Calcular con GetNextDoses para comparar
        var medicationDose = new MedicationDose
        {
            ScheduledTime = lastConfirmedTime
        };
        var expectedDoses = medicationDose.GetNextDoses(frequencyInHours, days).ToList();
        
        // Assert - Ambos métodos deben producir el mismo resultado
        calculatedDoses.Should().HaveCount(4, "porque con frecuencia de 6h en 24h hay 4 dosis");
        calculatedDoses.Should().BeEquivalentTo(expectedDoses, "la lógica de producción debe coincidir con GetNextDoses");
        
        // Verificar las horas exactas esperadas por el unit test original
        calculatedDoses[0].Should().Be(new DateTime(2025, 10, 24, 9, 0, 0), "primera dosis a las 9 AM");
        calculatedDoses[1].Should().Be(new DateTime(2025, 10, 24, 15, 0, 0), "segunda dosis a las 3 PM");
        calculatedDoses[2].Should().Be(new DateTime(2025, 10, 24, 21, 0, 0), "tercera dosis a las 9 PM");
        calculatedDoses[3].Should().Be(new DateTime(2025, 10, 25, 3, 0, 0), "cuarta dosis a las 3 AM del día siguiente");
    }

    [Fact]
    public void GetNextDoses_LogicValidation_With8HourFrequency()
    {
        // Test adicional: Verificar lógica con frecuencia de 8 horas
        
        // Arrange
        var scheduledTime = new DateTime(2025, 10, 24, 6, 0, 0); // 6:00 AM
        var medicationDose = new MedicationDose
        {
            ScheduledTime = scheduledTime
        };
        
        int frequencyInHours = 8;
        int days = 1;
        
        // Act
        var nextDoses = medicationDose.GetNextDoses(frequencyInHours, days).ToList();
        
        // Assert - Con 8h de frecuencia en 24h debe haber 3 dosis
        nextDoses.Should().HaveCount(3);
        nextDoses[0].Should().Be(new DateTime(2025, 10, 24, 14, 0, 0), "primera dosis a las 2 PM");
        nextDoses[1].Should().Be(new DateTime(2025, 10, 24, 22, 0, 0), "segunda dosis a las 10 PM");
        nextDoses[2].Should().Be(new DateTime(2025, 10, 25, 6, 0, 0), "tercera dosis a las 6 AM del siguiente día");
    }

    [Fact]
    public void GetNextDoses_LogicValidation_With4HourFrequency()
    {
        // Test adicional: Verificar lógica con frecuencia de 4 horas
        
        // Arrange
        var scheduledTime = new DateTime(2025, 10, 24, 0, 0, 0); // 12:00 AM (medianoche)
        var medicationDose = new MedicationDose
        {
            ScheduledTime = scheduledTime
        };
        
        int frequencyInHours = 4;
        int days = 1;
        
        // Act
        var nextDoses = medicationDose.GetNextDoses(frequencyInHours, days).ToList();
        
        // Assert - Con 4h de frecuencia en 24h debe haber 6 dosis
        nextDoses.Should().HaveCount(6);
        nextDoses[0].Should().Be(new DateTime(2025, 10, 24, 4, 0, 0));
        nextDoses[1].Should().Be(new DateTime(2025, 10, 24, 8, 0, 0));
        nextDoses[2].Should().Be(new DateTime(2025, 10, 24, 12, 0, 0));
        nextDoses[3].Should().Be(new DateTime(2025, 10, 24, 16, 0, 0));
        nextDoses[4].Should().Be(new DateTime(2025, 10, 24, 20, 0, 0));
        nextDoses[5].Should().Be(new DateTime(2025, 10, 25, 0, 0, 0));
    }
}
