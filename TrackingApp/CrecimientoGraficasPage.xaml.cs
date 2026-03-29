namespace TrackingApp;

public partial class CrecimientoGraficasPage : ContentPage
{
    private readonly ViewModels.CrecimientoGraficasViewModel _vm;

    public CrecimientoGraficasPage()
    {
        InitializeComponent();
        _vm = new ViewModels.CrecimientoGraficasViewModel();
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.CargarGraficas();
    }
}
