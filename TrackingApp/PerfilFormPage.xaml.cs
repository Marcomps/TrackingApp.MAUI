using TrackingApp.ViewModels;

namespace TrackingApp;

public partial class PerfilFormPage : ContentPage
{
    public PerfilFormPage()
    {
        InitializeComponent();
        BindingContext = new PerfilFormViewModel();
    }
}
