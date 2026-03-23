using TrackingApp.ViewModels;

namespace TrackingApp;

public partial class PerfilesPage : ContentPage
{
    private readonly PerfilesViewModel _vm;

    public PerfilesPage()
    {
        InitializeComponent();
        _vm = new PerfilesViewModel();
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Cargar();
    }
}
