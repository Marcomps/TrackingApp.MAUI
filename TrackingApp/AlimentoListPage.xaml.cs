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
        _vm.Cargar();
    }

    private async void OnGraficasTapped(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(AlimentoGraficasPage));
}
