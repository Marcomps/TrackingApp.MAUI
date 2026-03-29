using TrackingApp.ViewModels;

namespace TrackingApp;

public partial class CitasListPage : ContentPage
{
    private readonly CitasListViewModel _vm;

    public CitasListPage()
    {
        InitializeComponent();
        _vm = new CitasListViewModel();
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _vm.Cargar();
    }
}
