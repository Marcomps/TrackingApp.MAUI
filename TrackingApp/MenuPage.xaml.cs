using TrackingApp.Services;

namespace TrackingApp;

public partial class MenuPage : ContentPage
{
    public MenuPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ActualizarPerfilHeader();
    }

    private void ActualizarPerfilHeader()
    {
        var perfil = AppServices.DataService.PerfilActivo;
        if (perfil == null) return;

        LblPerfilEmoji.Text   = perfil.TipoPerfilEmoji;
        LblPerfilNombre.Text  = perfil.Nombre.Length > 8
            ? perfil.Nombre[..8] + "…"
            : perfil.Nombre;
        LblPerfilActivo.Text  = $"Perfil: {perfil.TipoPerfilDisplay}";
    }

    private async void OnPerfilTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//PerfilesTab");
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
