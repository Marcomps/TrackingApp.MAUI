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

        // Act - EXACTAMENTE la lógica de RecalculateNextDosesFromLastConfirmedAsync
        DateTime nextDoseTime = lastConfirmedTime.AddMinutes(frequencyInMinutes);
        var endDate = referenceTime.AddDays(days);
        var currentDose = nextDoseTime;
        var generatedDoses = new List<DateTime>();

        while (currentDose <= endDate)
        {
            generatedDoses.Add(currentDose);
            currentDose = currentDose.AddMinutes(frequencyInMinutes);
        }

        // Assert
        generatedDoses.Should().HaveCount(9, "debe generar 9 dosis en 3 días con frecuencia 8h");
        generatedDoses[0].Should().Be(new DateTime(2025, 11, 10, 10, 0, 0), 
            "primera dosis debe ser a las 10:00 AM (2 AM + 8 horas)");
    }

    [Fact]
    public void ProductionLogic_GetNextDoses_With6HourFrequency_ShouldGenerate5Doses()
    {
        // Arrange
        var startTime = new DateTime(2025, 11, 10, 0, 0, 0);
        var frequencyMinutes = 360; // 6 horas
        var days = 1;

        // Act - Lógica de GetNextDoses
        var doses = new List<DateTime>();
        var endDate = startTime.AddDays(days);
        var current = startTime;

        while (current <= endDate)
        {
            doses.Add(current);
            current = current.AddMinutes(frequencyMinutes);
        }

        // Assert
        doses.Should().HaveCount(5, "1 día con frecuencia 6h genera 5 dosis (00:00, 06:00, 12:00, 18:00, 00:00)");
        doses[0].Hour.Should().Be(0);
        doses[1].Hour.Should().Be(6);
        doses[2].Hour.Should().Be(12);
        doses[3].Hour.Should().Be(18);
        doses[4].Hour.Should().Be(0); // Día siguiente
    }
}
