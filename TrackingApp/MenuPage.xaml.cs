namespace TrackingApp;

public partial class MenuPage : ContentPage
{
    public MenuPage()
    {
        InitializeComponent();
    }

    private async void OnCrecimientoTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CrecimientoPage));
    }

    private async void OnAlimentoTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AlimentoListPage));
    }

    private async void OnSaludTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SaludPage));
    }

    private async void OnCitasMedicasTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CitasListPage));
    }

    private async void OnGraficasTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(GraficasPage));
    }
}
