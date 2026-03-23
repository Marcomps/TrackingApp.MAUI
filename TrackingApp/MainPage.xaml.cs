using TrackingApp.Services;
using TrackingApp.ViewModels;

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
		
		// Recargar todos los datos desde la base de datos
		await AppServices.DataService.ReloadAllDataAsync();
		
		// Notificar al ViewModel que los datos se actualizaron
		if (BindingContext is MainViewModel viewModel)
		{
			viewModel.NotifyAllDataChanged();
		}
	}

	private async void OnGraficasAlimentoClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(AlimentoGraficasPage));
	}

	private async void OnCrecimientoClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(CrecimientoPage));
	}

	private async void OnPreferenciasClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(PreferenciasPage));
	}
}