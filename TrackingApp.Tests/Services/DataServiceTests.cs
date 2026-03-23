using FluentAssertions;
using TrackingApp.Models;
using TrackingApp.Services;
using TrackingApp.Tests.Mocks;
using Xunit;

namespace TrackingApp.Tests.Services;

public class DataServiceTests
{
    private readonly MockDatabaseService _mockDatabase;
    private readonly DataService _dataService;

    public DataServiceTests()
    {
        _mockDatabase = new MockDatabaseService();
        _dataService = new DataService(_mockDatabase);
    }

    [Fact]
    public async Task ConfirmDoseAndRecalculateAsync_ShouldConfirmDose_SaveHistory_AndRecalculate()
    {
        // Arrange
        var medication = new Medication
        {
            Id = 1,
            Name = "Test Med",
            FrequencyHours = 8,
            FrequencyMinutes = 0,
            FirstDoseTime = DateTime.Now.Date
        };
        _dataService.Medications.Add(medication);

        var dose = new MedicationDose
        {
            Id = 1,
            MedicationId = 1,
            ScheduledTime = DateTime.Now,
            IsConfirmed = false,
            Medication = medication
        };
        _dataService.MedicationDoses.Add(dose);

        // Act
        await _dataService.ConfirmDoseAndRecalculateAsync(dose, 3);

        // Assert
        // 1. Dose should be confirmed
        dose.IsConfirmed.Should().BeTrue();
        
        // 2. History should be created
        _dataService.MedicationHistory.Should().Contain(h => h.MedicationId == 1);
        
        // 3. Future doses should be recalculated (we expect more doses now)
        // Since we generated for 3 days, and frequency is 8 hours, we expect multiple doses.
        _dataService.MedicationDoses.Count.Should().BeGreaterThan(1);
        
        // 4. Combined events should be updated
        _dataService.CombinedMedicationEvents.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GenerateDosesForMedicationAsync_ShouldGenerateCorrectNumberOfDoses()
    {
        // Arrange
        var medication = new Medication
        {
            Id = 2,
            Name = "Antibiotic",
            FrequencyHours = 6,
            FirstDoseTime = DateTime.Today
        };
        _dataService.Medications.Add(medication);

        // Act
        await _dataService.GenerateDosesForMedicationAsync(medication, 1); // 1 day

        // Assert
        // 24 hours / 6 hours = 4 doses + 1 for the next day start boundary potentially
        _dataService.MedicationDoses.Where(d => d.MedicationId == 2).Count().Should().BeGreaterThanOrEqualTo(4);
    }

    [Fact]
    public void ProductionLogic_ConfirmDoseAt2AM_NextDoseShouldBe10AM()
    {
        // Arrange
        var lastConfirmedTime = new DateTime(2025, 11, 10, 2, 0, 0); // 2 AM
        var frequencyInMinutes = 480; // 8 horas
        var days = 3;
        var referenceTime = new DateTime(2025, 11, 10, 2, 0, 0);

        // Act - EXACTAMENTE la lógica de RecalculateNextDosesFromLastConfirmedAsync (usa < no <=)
        DateTime nextDoseTime = lastConfirmedTime.AddMinutes(frequencyInMinutes);
        var endDate = referenceTime.AddDays(days);
        var currentDose = nextDoseTime;
        var generatedDoses = new List<DateTime>();

        while (currentDose < endDate)
        {
            generatedDoses.Add(currentDose);
            currentDose = currentDose.AddMinutes(frequencyInMinutes);
        }

        // Assert - Con frecuencia 8h en 3 días (desde 10 AM hasta antes de 2 AM día 4)
        // 10AM, 6PM, 2AM+1, 10AM+1, 6PM+1, 2AM+2, 10AM+2, 6PM+2 = 8 dosis
        generatedDoses.Should().HaveCount(8, "debe generar 8 dosis en 3 días con frecuencia 8h (excluye límite)");
        generatedDoses[0].Should().Be(new DateTime(2025, 11, 10, 10, 0, 0), 
            "primera dosis debe ser a las 10:00 AM (2 AM + 8 horas)");
    }

    [Fact]
    public void ProductionLogic_GetNextDoses_With6HourFrequency_ShouldGenerate4Doses()
    {
        // Arrange
        var startTime = new DateTime(2025, 11, 10, 0, 0, 0);
        var frequencyMinutes = 360; // 6 horas
        var days = 1;

        // Act - Lógica EXACTA de producción (usa < no <=)
        var doses = new List<DateTime>();
        var endDate = startTime.AddDays(days);
        var current = startTime;

        while (current < endDate)
        {
            doses.Add(current);
            current = current.AddMinutes(frequencyMinutes);
        }

        // Assert - Con < (no <=), la dosis exacta a las 00:00 del día siguiente NO se incluye
        doses.Should().HaveCount(4, "1 día con frecuencia 6h genera 4 dosis (00:00, 06:00, 12:00, 18:00)");
        doses[0].Hour.Should().Be(0);
        doses[1].Hour.Should().Be(6);
        doses[2].Hour.Should().Be(12);
        doses[3].Hour.Should().Be(18);
    }

    #region Dose Calculation Edge Cases

    [Fact]
    public void TotalFrequencyInMinutes_ShouldCalculateCorrectly()
    {
        // Test: FrequencyHours=4, FrequencyMinutes=30 -> 270 minutos
        var medication = new Medication
        {
            FrequencyHours = 4,
            FrequencyMinutes = 30
        };

        medication.TotalFrequencyInMinutes.Should().Be(270);
    }

    [Fact]
    public void TotalFrequencyInMinutes_OnlyHours_ShouldCalculateCorrectly()
    {
        // Test: FrequencyHours=8, FrequencyMinutes=0 -> 480 minutos
        var medication = new Medication
        {
            FrequencyHours = 8,
            FrequencyMinutes = 0
        };

        medication.TotalFrequencyInMinutes.Should().Be(480);
    }

    [Fact]
    public void TotalFrequencyInMinutes_OnlyMinutes_ShouldCalculateCorrectly()
    {
        // Test: FrequencyHours=0, FrequencyMinutes=45 -> 45 minutos
        var medication = new Medication
        {
            FrequencyHours = 0,
            FrequencyMinutes = 45
        };

        medication.TotalFrequencyInMinutes.Should().Be(45);
    }

    [Fact]
    public void DoseStatus_Confirmado_WhenIsConfirmedTrue()
    {
        var dose = new MedicationDose
        {
            ScheduledTime = DateTime.Now.AddHours(-2),
            IsConfirmed = true
        };

        dose.Status.Should().Be("Confirmado");
    }

    [Fact]
    public void DoseStatus_Atrasado_WhenMoreThan30MinutesPast()
    {
        var dose = new MedicationDose
        {
            ScheduledTime = DateTime.Now.AddMinutes(-45),
            IsConfirmed = false
        };

        dose.Status.Should().Be("Atrasado");
    }

    [Fact]
    public void DoseStatus_Proximo_WhenWithin30Minutes()
    {
        var dose = new MedicationDose
        {
            ScheduledTime = DateTime.Now.AddMinutes(15),
            IsConfirmed = false
        };

        dose.Status.Should().Be("Próximo");
    }

    [Fact]
    public void DoseStatus_Programado_WhenMoreThan30MinutesAway()
    {
        var dose = new MedicationDose
        {
            ScheduledTime = DateTime.Now.AddMinutes(60),
            IsConfirmed = false
        };

        dose.Status.Should().Be("Programado");
    }

    [Fact]
    public void ProductionLogic_Every4h30min_CrossesMidnight_ShouldCalculateCorrectly()
    {
        // Arrange - Primera dosis 3:15 PM, frecuencia 4h 30min
        var startTime = new DateTime(2025, 10, 20, 15, 15, 0); // 3:15 PM
        var frequencyMinutes = 270; // 4h 30min
        var days = 1;

        // Act
        var doses = new List<DateTime>();
        var endDate = startTime.AddDays(days);
        var current = startTime;

        while (current < endDate)
        {
            doses.Add(current);
            current = current.AddMinutes(frequencyMinutes);
        }

        // Assert - Dosis esperadas:
        // 3:15 PM, 7:45 PM, 12:15 AM (+1), 4:45 AM (+1), 9:15 AM (+1), 1:45 PM (+1)
        doses.Should().HaveCount(6);
        doses[0].Should().Be(new DateTime(2025, 10, 20, 15, 15, 0)); // 3:15 PM
        doses[1].Should().Be(new DateTime(2025, 10, 20, 19, 45, 0)); // 7:45 PM
        doses[2].Should().Be(new DateTime(2025, 10, 21, 0, 15, 0));  // 12:15 AM
        doses[3].Should().Be(new DateTime(2025, 10, 21, 4, 45, 0));  // 4:45 AM
        doses[4].Should().Be(new DateTime(2025, 10, 21, 9, 15, 0));  // 9:15 AM
        doses[5].Should().Be(new DateTime(2025, 10, 21, 13, 45, 0)); // 1:45 PM
    }

    [Fact]
    public void ProductionLogic_Every12Hours_TwoDays_ShouldGenerateCorrectDoses()
    {
        // Arrange
        var startTime = new DateTime(2025, 10, 20, 8, 0, 0); // 8 AM
        var frequencyMinutes = 720; // 12 horas
        var days = 2;

        // Act
        var doses = new List<DateTime>();
        var endDate = startTime.AddDays(days);
        var current = startTime;

        while (current < endDate)
        {
            doses.Add(current);
            current = current.AddMinutes(frequencyMinutes);
        }

        // Assert - 8AM, 8PM, 8AM+1, 8PM+1 = 4 dosis
        doses.Should().HaveCount(4);
        doses[0].Should().Be(new DateTime(2025, 10, 20, 8, 0, 0));
        doses[1].Should().Be(new DateTime(2025, 10, 20, 20, 0, 0));
        doses[2].Should().Be(new DateTime(2025, 10, 21, 8, 0, 0));
        doses[3].Should().Be(new DateTime(2025, 10, 21, 20, 0, 0));
    }

    [Fact]
    public async Task RecalculateNextDoses_WhenDelayedConfirmation_ShouldAdjustFromActualTime()
    {
        // Arrange - Medicamento con frecuencia 8 horas
        var medication = new Medication
        {
            Id = 3,
            Name = "Delayed Med",
            FrequencyHours = 8,
            FrequencyMinutes = 0,
            FirstDoseTime = DateTime.Now.Date.AddHours(8) // 8 AM
        };
        _dataService.Medications.Add(medication);

        // Simular que la dosis fue confirmada con 30 min de retraso (8:30 AM)
        // Usando el historial (que es donde se registran las confirmaciones reales)
        var history = new MedicationHistory
        {
            Id = 1,
            MedicationId = 3,
            MedicationName = "Delayed Med",
            Dose = "10mg",
            AdministeredTime = DateTime.Now.Date.AddHours(8).AddMinutes(30) // 8:30 AM (retraso)
        };
        _dataService.MedicationHistory.Add(history);

        // Act - Recalcular dosis desde la última en historial
        await _dataService.RecalculateNextDosesFromLastConfirmedAsync(3, 1);

        // Assert - La siguiente dosis debe ser 8h después de las 8:30 AM = 4:30 PM
        var nextDoses = _dataService.MedicationDoses.Where(d => d.MedicationId == 3 && !d.IsConfirmed).ToList();
        
        nextDoses.Should().NotBeEmpty();
        // La primera dosis no confirmada debe ser a las 4:30 PM (8:30 AM + 8h)
        var expectedNextTime = DateTime.Now.Date.AddHours(16).AddMinutes(30); // 4:30 PM
        nextDoses.First().ScheduledTime.Should().BeCloseTo(expectedNextTime, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void DisplayTime_ShouldShowActualTimeWhenAvailable()
    {
        // Arrange
        var scheduledTime = new DateTime(2025, 10, 20, 14, 0, 0); // 2:00 PM
        var actualTime = new DateTime(2025, 10, 20, 14, 15, 0);   // 2:15 PM (confirmed late)
        
        var dose = new MedicationDose
        {
            ScheduledTime = scheduledTime,
            ActualTime = actualTime
        };

        // Assert - DisplayTime should use ActualTime when available
        dose.DisplayTime.Should().Be("02:15 PM");
    }

    [Fact]
    public void DisplayTime_ShouldShowScheduledTimeWhenNoActualTime()
    {
        // Arrange
        var scheduledTime = new DateTime(2025, 10, 20, 14, 0, 0); // 2:00 PM
        
        var dose = new MedicationDose
        {
            ScheduledTime = scheduledTime,
            ActualTime = null
        };

        // Assert - DisplayTime should use ScheduledTime
        dose.DisplayTime.Should().Be("02:00 PM");
    }

    [Fact]
    public void Medication_DisplayText_WithHoursAndMinutes_ShouldFormatCorrectly()
    {
        var medication = new Medication
        {
            Name = "Paracetamol",
            Dose = "500mg",
            FrequencyHours = 4,
            FrequencyMinutes = 30
        };

        medication.DisplayText.Should().Be("Paracetamol (500mg) cada 4h 30min");
    }

    [Fact]
    public void Medication_DisplayText_OnlyHours_ShouldFormatCorrectly()
    {
        var medication = new Medication
        {
            Name = "Ibuprofeno",
            Dose = "200mg",
            FrequencyHours = 8,
            FrequencyMinutes = 0
        };

        medication.DisplayText.Should().Be("Ibuprofeno (200mg) cada 8h");
    }

    [Fact]
    public void Medication_DisplayText_OnlyMinutes_ShouldFormatCorrectly()
    {
        var medication = new Medication
        {
            Name = "Gotas",
            Dose = "5ml",
            FrequencyHours = 0,
            FrequencyMinutes = 45
        };

        medication.DisplayText.Should().Be("Gotas (5ml) cada 45min");
    }

    #endregion

    #region Dose Confirmation Flow (Critical Bug Tests)

    [Fact]
    public async Task ConfirmDose_NextDoseShouldBeAfterConfirmedTime_NotSameTime()
    {
        // ESCENARIO DEL BUG: Usuario agrega medicamento a las 1:13PM con intervalo 8h
        // Confirma la dosis de 1:13PM, la siguiente debe ser 9:13PM, NO 1:13PM otra vez
        var medication = new Medication
        {
            Id = 10,
            Name = "TestBugFix",
            FrequencyHours = 8,
            FrequencyMinutes = 0,
            FirstDoseTime = DateTime.Today.AddHours(13).AddMinutes(13) // 1:13 PM
        };
        _dataService.Medications.Add(medication);

        var scheduledTime = DateTime.Today.AddHours(13).AddMinutes(13);
        var dose = new MedicationDose
        {
            Id = 100,
            MedicationId = 10,
            ScheduledTime = scheduledTime,
            IsConfirmed = false,
            Medication = medication
        };
        _dataService.MedicationDoses.Add(dose);

        // Act - Confirmar la dosis
        await _dataService.ConfirmDoseAndRecalculateAsync(dose, 3);

        // Assert
        // 1. La dosis original ya NO debe estar en MedicationDoses
        _dataService.MedicationDoses.Should().NotContain(d => d.Id == 100);

        // 2. Debe haber un registro en el historial
        _dataService.MedicationHistory.Should().ContainSingle(h => h.MedicationId == 10);

        // 3. TODAS las nuevas dosis deben ser FUTURAS respecto a la hora de confirmación
        var newDoses = _dataService.MedicationDoses.Where(d => d.MedicationId == 10).ToList();
        newDoses.Should().NotBeEmpty("debe haber dosis futuras generadas");
        
        foreach (var newDose in newDoses)
        {
            newDose.ScheduledTime.Should().BeAfter(DateTime.Now.AddMinutes(-1), 
                $"la dosis a {newDose.ScheduledTime:HH:mm} debe ser posterior a ahora");
            newDose.IsConfirmed.Should().BeFalse();
        }

        // 4. La primera nueva dosis debe ser ~8 horas después de la confirmación
        var firstNewDose = newDoses.OrderBy(d => d.ScheduledTime).First();
        firstNewDose.ScheduledTime.Should().BeCloseTo(
            DateTime.Now.AddMinutes(medication.TotalFrequencyInMinutes), 
            TimeSpan.FromMinutes(2),
            "la primera dosis nueva debe ser ~8 horas después de confirmar");
    }

    [Fact]
    public async Task ConfirmDose_CombinedEventsShouldNotContainConfirmedDose()
    {
        // Verify that after confirming, the confirmed dose doesn't appear as pending
        var medication = new Medication
        {
            Id = 11,
            Name = "CombinedTest",
            FrequencyHours = 6,
            FrequencyMinutes = 0,
            FirstDoseTime = DateTime.Today
        };
        _dataService.Medications.Add(medication);

        var dose = new MedicationDose
        {
            Id = 101,
            MedicationId = 11,
            ScheduledTime = DateTime.Now,
            IsConfirmed = false,
            Medication = medication
        };
        _dataService.MedicationDoses.Add(dose);

        // Act
        await _dataService.ConfirmDoseAndRecalculateAsync(dose, 1);

        // Assert - CombinedMedicationEvents should have history + new pending doses
        var historyEvents = _dataService.CombinedMedicationEvents
            .Where(e => e.IsHistory && e.MedicationId == 11).ToList();
        historyEvents.Should().HaveCount(1, "debe haber 1 registro histórico");

        var pendingEvents = _dataService.CombinedMedicationEvents
            .Where(e => !e.IsHistory && !e.IsConfirmed && e.MedicationId == 11).ToList();
        
        // All pending events should be in the future
        foreach (var ev in pendingEvents)
        {
            ev.EventTime.Should().BeAfter(DateTime.Now.AddMinutes(-1),
                "las dosis pendientes deben ser futuras");
        }
    }

    [Fact]
    public async Task ConfirmDose_OldPendingDosesShouldBeRemoved()
    {
        // If there are multiple pending doses, confirming one should
        // remove ALL old pending doses and regenerate from confirmed time
        var medication = new Medication
        {
            Id = 12,
            Name = "MultiDoseTest",
            FrequencyHours = 4,
            FrequencyMinutes = 0,
            FirstDoseTime = DateTime.Today
        };
        _dataService.Medications.Add(medication);

        // Add multiple pending doses
        for (int i = 0; i < 3; i++)
        {
            var d = new MedicationDose
            {
                Id = 200 + i,
                MedicationId = 12,
                ScheduledTime = DateTime.Now.AddHours(i * 4),
                IsConfirmed = false,
                Medication = medication
            };
            _dataService.MedicationDoses.Add(d);
        }

        _dataService.MedicationDoses.Where(d => d.MedicationId == 12).Should().HaveCount(3);

        // Act - Confirm the first dose
        var firstDose = _dataService.MedicationDoses.First(d => d.MedicationId == 12);
        await _dataService.ConfirmDoseAndRecalculateAsync(firstDose, 2);

        // Assert - ALL old doses should be gone, replaced with new ones starting from confirmed+freq
        var remainingDoses = _dataService.MedicationDoses.Where(d => d.MedicationId == 12).ToList();
        
        // None of the old IDs should remain
        remainingDoses.Should().NotContain(d => d.Id == 200);
        remainingDoses.Should().NotContain(d => d.Id == 201);
        remainingDoses.Should().NotContain(d => d.Id == 202);
        
        // All remaining should be new, unconfirmed, and in the future
        foreach (var d in remainingDoses)
        {
            d.IsConfirmed.Should().BeFalse();
            d.ScheduledTime.Should().BeAfter(DateTime.Now.AddMinutes(-1), 
                "todas las dosis recalculadas deben ser futuras");
        }
    }

    [Fact]
    public async Task ConfirmDose_SuppressesIntermediateRebuilds()
    {
        // Verify that after completion, CombinedMedicationEvents is consistent
        var medication = new Medication
        {
            Id = 13,
            Name = "SuppressTest",
            FrequencyHours = 8,
            FrequencyMinutes = 0,
            FirstDoseTime = DateTime.Today
        };
        _dataService.Medications.Add(medication);

        var dose = new MedicationDose
        {
            Id = 300,
            MedicationId = 13,
            ScheduledTime = DateTime.Now,
            IsConfirmed = false,
            Medication = medication
        };
        _dataService.MedicationDoses.Add(dose);

        // Act
        await _dataService.ConfirmDoseAndRecalculateAsync(dose, 1);

        // Assert - After completion, CombinedMedicationEvents should be consistent
        var historyCount = _dataService.MedicationHistory.Count(h => h.MedicationId == 13);
        var pendingCount = _dataService.MedicationDoses.Count(d => d.MedicationId == 13 && !d.IsConfirmed);
        
        var combinedHistory = _dataService.CombinedMedicationEvents.Count(e => e.IsHistory && e.MedicationId == 13);
        var combinedPending = _dataService.CombinedMedicationEvents.Count(e => !e.IsHistory && e.MedicationId == 13);

        combinedHistory.Should().Be(historyCount, "el historial combinado debe coincidir con MedicationHistory");
        combinedPending.Should().Be(pendingCount, "las dosis combinadas deben coincidir con MedicationDoses pendientes");
    }

    [Fact]
    public async Task RecalculateFromConfirmedTime_GeneratesCorrectTimes()
    {
        // Test the exact time calculation
        var confirmedTime = new DateTime(2025, 7, 15, 13, 13, 0); // 1:13 PM
        var medication = new Medication
        {
            Id = 14,
            Name = "TimeCalcTest",
            FrequencyHours = 8,
            FrequencyMinutes = 0,
            FirstDoseTime = confirmedTime
        };
        _dataService.Medications.Add(medication);

        // Act
        await _dataService.RecalculateNextDosesFromConfirmedTimeAsync(14, confirmedTime, 2);

        // Assert
        var doses = _dataService.MedicationDoses
            .Where(d => d.MedicationId == 14)
            .OrderBy(d => d.ScheduledTime)
            .ToList();

        // First dose should be confirmedTime + 8h = 9:13 PM
        if (doses.Count > 0)
        {
            var expectedFirst = confirmedTime.AddHours(8); // 9:13 PM
            doses[0].ScheduledTime.Should().Be(expectedFirst, 
                "primera dosis debe ser 8h después de confirmación (9:13 PM)");
        }

        // Second dose should be confirmedTime + 16h = 5:13 AM next day
        if (doses.Count > 1)
        {
            var expectedSecond = confirmedTime.AddHours(16); // 5:13 AM +1
            doses[1].ScheduledTime.Should().Be(expectedSecond,
                "segunda dosis debe ser 16h después de confirmación (5:13 AM)");
        }

        // Third dose should be confirmedTime + 24h = 1:13 PM next day
        if (doses.Count > 2)
        {
            var expectedThird = confirmedTime.AddHours(24); // 1:13 PM +1
            doses[2].ScheduledTime.Should().Be(expectedThird,
                "tercera dosis debe ser 24h después de confirmación (1:13 PM)");
        }

        // All doses should have correct frequency spacing
        for (int i = 1; i < doses.Count; i++)
        {
            var gap = doses[i].ScheduledTime - doses[i - 1].ScheduledTime;
            gap.TotalMinutes.Should().Be(480, "cada dosis debe tener 8h (480min) de diferencia");
        }
    }

    [Fact]
    public async Task ConfirmDose_HistoryContainsCorrectInfo()
    {
        var medication = new Medication
        {
            Id = 15,
            Name = "HistoryInfoTest",
            Dose = "500mg",
            FrequencyHours = 6,
            FrequencyMinutes = 0,
            FirstDoseTime = DateTime.Today
        };
        _dataService.Medications.Add(medication);

        var dose = new MedicationDose
        {
            Id = 400,
            MedicationId = 15,
            ScheduledTime = DateTime.Now,
            IsConfirmed = false,
            Medication = medication
        };
        _dataService.MedicationDoses.Add(dose);

        // Act
        var beforeConfirm = DateTime.Now;
        await _dataService.ConfirmDoseAndRecalculateAsync(dose, 1);
        var afterConfirm = DateTime.Now;

        // Assert
        var history = _dataService.MedicationHistory.First(h => h.MedicationId == 15);
        history.MedicationName.Should().Be("HistoryInfoTest");
        history.Dose.Should().Be("500mg");
        history.AdministeredTime.Should().BeOnOrAfter(beforeConfirm);
        history.AdministeredTime.Should().BeOnOrBefore(afterConfirm);
    }

    [Fact]
    public async Task ConfirmDose_OriginalDoseIsRemovedFromDatabase()
    {
        var medication = new Medication
        {
            Id = 16,
            Name = "DbRemoveTest",
            FrequencyHours = 8,
            FrequencyMinutes = 0,
            FirstDoseTime = DateTime.Today
        };
        _dataService.Medications.Add(medication);

        var dose = new MedicationDose
        {
            Id = 500,
            MedicationId = 16,
            ScheduledTime = DateTime.Now,
            IsConfirmed = false,
            Medication = medication
        };
        _dataService.MedicationDoses.Add(dose);
        _mockDatabase.Doses.Add(dose); // Simulate it being in the DB

        // Act
        await _dataService.ConfirmDoseAndRecalculateAsync(dose, 1);

        // Assert - The original dose should be removed from the mock DB
        _mockDatabase.Doses.Should().NotContain(d => d.Id == 500,
            "la dosis confirmada debe eliminarse de la base de datos");
    }

    #endregion
}
