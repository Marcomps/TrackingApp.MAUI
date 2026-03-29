namespace TrackingApp;

public partial class CrecimientoPage : ContentPage
{
    private readonly ViewModels.CrecimientoViewModel _vm;

    public CrecimientoPage()
    {
        InitializeComponent();
        _vm = new ViewModels.CrecimientoViewModel();
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Cargar();
    }

    private async void OnGraficasCrecimientoClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(CrecimientoGraficasPage));
}
