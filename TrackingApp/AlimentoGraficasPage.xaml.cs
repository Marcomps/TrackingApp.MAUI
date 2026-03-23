using TrackingApp.ViewModels;

namespace TrackingApp;

public partial class AlimentoGraficasPage : ContentPage
{
    private readonly AlimentoGraficasViewModel _viewModel;

    public AlimentoGraficasPage()
    {
        InitializeComponent();
        _viewModel = new AlimentoGraficasViewModel();
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh data each time the page becomes visible
        _viewModel.CargarGrafica();
    }
}
