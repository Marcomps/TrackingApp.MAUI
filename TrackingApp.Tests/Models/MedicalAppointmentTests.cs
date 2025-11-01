using Xunit;
using FluentAssertions;
using TrackingApp.Models;

namespace TrackingApp.Tests.Models;

public class MedicalAppointmentTests
{
    [Fact]
    public void MedicalAppointment_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var appointment = new MedicalAppointment();

        // Assert
        appointment.Id.Should().Be(0);
        appointment.Title.Should().Be(string.Empty);
        appointment.Notes.Should().Be(string.Empty);
        appointment.IsConfirmed.Should().BeFalse();
        appointment.ConfirmedDate.Should().BeNull();
        appointment.UserType.Should().Be(string.Empty);
    }

    [Fact]
    public void MedicalAppointment_PropertiesSetCorrectly()
    {
        // Arrange
        var appointmentDate = new DateTime(2024, 12, 15, 10, 30, 0);
        var confirmedDate = new DateTime(2024, 12, 15, 10, 35, 0);

        // Act
        var appointment = new MedicalAppointment
        {
            Id = 1,
            Title = "Checkup mensual",
            Notes = "Llevar carnet de vacunación",
            AppointmentDate = appointmentDate,
            IsConfirmed = true,
            ConfirmedDate = confirmedDate,
            UserType = "Bebé"
        };

        // Assert
        appointment.Id.Should().Be(1);
        appointment.Title.Should().Be("Checkup mensual");
        appointment.Notes.Should().Be("Llevar carnet de vacunación");
        appointment.AppointmentDate.Should().Be(appointmentDate);
        appointment.IsConfirmed.Should().BeTrue();
        appointment.ConfirmedDate.Should().Be(confirmedDate);
        appointment.UserType.Should().Be("Bebé");
    }

    [Fact]
    public void FormattedAppointmentDate_ReturnsCorrectFormat()
    {
        // Arrange
        var appointment = new MedicalAppointment
        {
            AppointmentDate = new DateTime(2024, 12, 15, 10, 30, 0)
        };

        // Act
        var formatted = appointment.FormattedAppointmentDate;

        // Assert
        formatted.Should().Contain("15");
        formatted.Should().Contain("10:30");
    }

    [Fact]
    public void FormattedConfirmedDate_WithConfirmedAppointment_ReturnsCorrectFormat()
    {
        // Arrange
        var appointment = new MedicalAppointment
        {
            IsConfirmed = true,
            ConfirmedDate = new DateTime(2024, 12, 15, 10, 35, 0)
        };

        // Act
        var formatted = appointment.FormattedConfirmedDate;

        // Assert
        formatted.Should().Contain("15");
        formatted.Should().Contain("10:35");
    }

    [Fact]
    public void FormattedConfirmedDate_WithUnconfirmedAppointment_ReturnsNA()
    {
        // Arrange
        var appointment = new MedicalAppointment
        {
            IsConfirmed = false,
            ConfirmedDate = null
        };

        // Act
        var formatted = appointment.FormattedConfirmedDate;

        // Assert
        formatted.Should().Be("N/A");
    }

    [Fact]
    public void ConfirmAppointment_UpdatesConfirmationStatus()
    {
        // Arrange
        var appointment = new MedicalAppointment
        {
            Title = "Checkup",
            AppointmentDate = DateTime.Now.AddDays(1),
            IsConfirmed = false
        };

        var confirmationDate = DateTime.Now;

        // Act
        appointment.IsConfirmed = true;
        appointment.ConfirmedDate = confirmationDate;

        // Assert
        appointment.IsConfirmed.Should().BeTrue();
        appointment.ConfirmedDate.Should().Be(confirmationDate);
    }

    [Fact]
    public void MedicalAppointment_WithUserType_AssociatesCorrectly()
    {
        // Arrange & Act
        var appointment = new MedicalAppointment
        {
            Title = "Vacuna",
            UserType = "Bebé",
            AppointmentDate = DateTime.Now.AddDays(7)
        };

        // Assert
        appointment.UserType.Should().Be("Bebé");
    }

    [Theory]
    [InlineData("Bebé")]
    [InlineData("Mamá")]
    [InlineData("Papá")]
    public void MedicalAppointment_SupportsMultipleUserTypes(string userType)
    {
        // Arrange & Act
        var appointment = new MedicalAppointment
        {
            UserType = userType
        };

        // Assert
        appointment.UserType.Should().Be(userType);
    }
}
