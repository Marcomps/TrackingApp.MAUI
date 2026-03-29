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
}
