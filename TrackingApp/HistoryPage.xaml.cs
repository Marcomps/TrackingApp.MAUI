namespace TrackingApp;

public partial class HistoryPage : ContentPage
{
    public HistoryPage()
    {
        InitializeComponent();
    }

    private async void OnGraficasAlimentoClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(AlimentoGraficasPage));

    private async void OnCitasListClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync(nameof(CitasListPage));
}
