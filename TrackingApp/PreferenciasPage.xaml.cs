namespace TrackingApp;

public partial class PreferenciasPage : ContentPage
{
    public PreferenciasPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.PreferenciasViewModel();
    }
}
