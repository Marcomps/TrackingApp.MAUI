namespace TrackingApp;

public partial class CrecimientoFormPage : ContentPage
{
    public CrecimientoFormPage()
    {
        InitializeComponent();
        BindingContext = new ViewModels.CrecimientoFormViewModel();
    }
}
