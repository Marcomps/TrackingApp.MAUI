using TrackingApp.Services;

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
}