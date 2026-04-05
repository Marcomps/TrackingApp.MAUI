using TrackingApp.ViewModels;

namespace TrackingApp;

public partial class GraficasPage : ContentPage
{
    private readonly GraficasViewModel _vm;

    public GraficasPage()
    {
        InitializeComponent();
        _vm = new GraficasViewModel();
        BindingContext = _vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Refresh the active tab each time the page becomes visible (e.g. after add/edit/delete)
        _vm.RefrescarTabActivo();
    }

    private async void OnVolverMenuClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("//menu");
}
