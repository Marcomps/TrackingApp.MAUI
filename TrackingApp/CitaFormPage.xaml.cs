using TrackingApp.ViewModels;

namespace TrackingApp;

public partial class CitaFormPage : ContentPage
{
    public CitaFormPage()
    {
        InitializeComponent();
        BindingContext = new CitaFormViewModel();
    }
}
