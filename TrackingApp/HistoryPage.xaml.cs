namespace TrackingApp;

public partial class HistoryPage : ContentPage
{
    public HistoryPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TrackingApp.ViewModels.HistoryViewModel vm)
            await vm.ReloadAsync();
    }

    private async void OnGraficasAlimentoClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(AlimentoGraficasPage));

    private async void OnCitasListClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(CitasListPage));
}
