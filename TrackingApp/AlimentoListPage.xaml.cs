using TrackingApp.Services;
using TrackingApp.ViewModels;

namespace TrackingApp;

public partial class AlimentoListPage : ContentPage
{
    private readonly AlimentoListViewModel _vm;

    public AlimentoListPage()
    {
        InitializeComponent();
        _vm = new AlimentoListViewModel();
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Ensure DB data is loaded before filtering, then refresh the list.
        Dispatcher.DispatchAsync(async () =>
        {
            await AppServices.DataService.ReloadAllDataAsync();
            _vm.Cargar();
        });
    }

    private async void OnGraficasTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(AlimentoGraficasPage));
}
