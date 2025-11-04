using Xunit;
using FluentAssertions;
using TrackingApp.Models;

namespace TrackingApp.Tests.Helpers;

public class AppointmentFilteringTests
{
    [Fact]
    public void AppointmentList_CanBeFilteredByProfile()
    {
        // Arrange
        var appointments = new List<MedicalAppointment>
        {
            new MedicalAppointment { Id = 1, UserType = "Bebé", IsConfirmed = true },
            new MedicalAppointment { Id = 2, UserType = "Mamá", IsConfirmed = true },
            new MedicalAppointment { Id = 3, UserType = "Bebé", IsConfirmed = true }
        };

        var selectedProfile = "Bebé";

        // Act
        var filtered = appointments.Where(a => a.UserType == selectedProfile).ToList();

        // Assert
        filtered.Should().HaveCount(2);
        filtered.Should().OnlyContain(a => a.UserType == "Bebé");
    }

    [Fact]
    public void AppointmentList_CanBeFilteredByConfirmationStatus()
    {
        // Arrange
        var appointments = new List<MedicalAppointment>
        {
            new MedicalAppointment { Id = 1, IsConfirmed = true, ConfirmedDate = DateTime.Now },
            new MedicalAppointment { Id = 2, IsConfirmed = false },
            new MedicalAppointment { Id = 3, IsConfirmed = true, ConfirmedDate = DateTime.Now.AddHours(-1) }
        };

        // Act
        var confirmed = appointments.Where(a => a.IsConfirmed).ToList();

        // Assert
        confirmed.Should().HaveCount(2);
        confirmed.Should().OnlyContain(a => a.IsConfirmed);
    }

    [Fact]
    public void AppointmentList_CanBeFilteredByDateRange()
    {
        // Arrange
        var now = DateTime.Now;
        var appointments = new List<MedicalAppointment>
        {
            new MedicalAppointment { Id = 1, ConfirmedDate = now.AddDays(-5), IsConfirmed = true },
            new MedicalAppointment { Id = 2, ConfirmedDate = now.AddDays(-2), IsConfirmed = true },
            new MedicalAppointment { Id = 3, ConfirmedDate = now.AddDays(-10), IsConfirmed = true }
        };

        var startDate = now.AddDays(-7);
        var endDate = now.AddDays(-3);

        // Act
        var filtered = appointments
            .Where(a => a.ConfirmedDate.HasValue && 
                       a.ConfirmedDate.Value >= startDate && 
                       a.ConfirmedDate.Value <= endDate)
            .ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered.First().Id.Should().Be(1);
    }

    [Fact]
    public void AppointmentStatistics_CountsConfirmedAppointments()
    {
        // Arrange
        var appointments = new List<MedicalAppointment>
        {
            new MedicalAppointment { Id = 1, IsConfirmed = true },
            new MedicalAppointment { Id = 2, IsConfirmed = true },
            new MedicalAppointment { Id = 3, IsConfirmed = false }
        };

        // Act
        var totalConfirmed = appointments.Count(a => a.IsConfirmed);

        // Assert
        totalConfirmed.Should().Be(2);
    }

    [Theory]
    [InlineData("Bebé")]
    [InlineData("Mamá")]
    [InlineData("Papá")]
    public void AppointmentFilter_SupportsMultipleProfiles(string profileName)
    {
        // Arrange
        var appointments = new List<MedicalAppointment>
        {
            new MedicalAppointment { Id = 1, UserType = profileName, IsConfirmed = true },
            new MedicalAppointment { Id = 2, UserType = "Other", IsConfirmed = true }
        };

        // Act
        var filtered = appointments.Where(a => a.UserType == profileName).ToList();

        // Assert
        filtered.Should().HaveCount(1);
        filtered.First().UserType.Should().Be(profileName);
    }
}
