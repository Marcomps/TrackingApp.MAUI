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
        await Shell.Current.GoToAsync("//MainPage");
    }

    private async void OnPesoMamaTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Peso de mamá", "Función de peso próximamente", "OK");
    }

    private async void OnCalmarTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Calmar", "Función para calmar al bebé próximamente", "OK");
    }

    private async void OnSaludTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }

    private async void OnPanalTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Pañal", "Registro de pañales próximamente", "OK");
    }

    // Bottom Navigation
    private async void OnCalendarioTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Calendario", "Función de calendario próximamente", "OK");
    }

    private async void OnFavoritosTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Favoritos", "Función de favoritos próximamente", "OK");
    }

    private async void OnChatTapped(object sender, EventArgs e)
    {
        await DisplayAlert("Chat", "Función de chat próximamente", "OK");
    }
}
