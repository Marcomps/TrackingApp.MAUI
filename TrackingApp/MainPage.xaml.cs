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

}