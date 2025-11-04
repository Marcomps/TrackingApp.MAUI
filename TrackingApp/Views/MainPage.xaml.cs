using TrackingApp.Services;
using TrackingApp.ViewModels;
using TrackingApp.Models;

namespace TrackingApp;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		
		// Cargar historial de medicamentos
		await DataService.Instance.LoadMedicationHistoryAsync();
	}

	private void OnConfirmButtonClicked(object sender, EventArgs e)
	{
		System.Diagnostics.Debug.WriteLine("=== OnConfirmButtonClicked FIRED ===");
		
		if (sender is Button button)
		{
			System.Diagnostics.Debug.WriteLine($"Button found. CommandParameter type: {button.CommandParameter?.GetType().Name ?? "null"}");
			
			if (button.CommandParameter is MedicationEvent medicationEvent)
			{
				System.Diagnostics.Debug.WriteLine($"MedicationEvent: Id={medicationEvent.Id}, MedicationId={medicationEvent.MedicationId}, SourceId={medicationEvent.SourceId}, IsHistory={medicationEvent.IsHistory}");
				
				if (BindingContext is MainViewModel viewModel)
				{
					System.Diagnostics.Debug.WriteLine("ViewModel found, executing command...");
					if (viewModel.ConfirmEventCommand.CanExecute(medicationEvent))
					{
						viewModel.ConfirmEventCommand.Execute(medicationEvent);
					}
					else
					{
						System.Diagnostics.Debug.WriteLine("Command CanExecute returned false");
					}
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("ViewModel NOT found");
				}
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("CommandParameter is NOT a MedicationEvent");
			}
		}
		else
		{
			System.Diagnostics.Debug.WriteLine("Sender is NOT a Button");
		}
	}

	private async void OnConfirmAppointmentButtonClicked(object sender, EventArgs e)
	{
		System.Diagnostics.Debug.WriteLine("=== OnConfirmAppointmentButtonClicked FIRED ===");
		
		if (sender is Button button)
		{
			System.Diagnostics.Debug.WriteLine($"Button found. CommandParameter type: {button.CommandParameter?.GetType().Name ?? "null"}");
			
			if (button.CommandParameter is MedicalAppointment appointment)
			{
				System.Diagnostics.Debug.WriteLine($"MedicalAppointment: Id={appointment.Id}, Title={appointment.Title}");
				
				if (BindingContext is MainViewModel viewModel)
				{
					System.Diagnostics.Debug.WriteLine("ViewModel found, confirming appointment...");
					
					// Confirmar la cita usando el servicio
					await DataService.Instance.ConfirmAppointmentAsync(appointment);
					
					// Recargar las citas para actualizar la UI
					await DataService.Instance.LoadAppointmentsAsync();
					
					System.Diagnostics.Debug.WriteLine("Appointment confirmed successfully");
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("ViewModel NOT found");
				}
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("CommandParameter is NOT a MedicalAppointment");
			}
		}
		else
		{
			System.Diagnostics.Debug.WriteLine("Sender is NOT a Button");
		}
	}
}