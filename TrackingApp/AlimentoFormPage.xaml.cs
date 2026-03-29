using TrackingApp.ViewModels;

namespace TrackingApp;

public partial class AlimentoFormPage : ContentPage
{
    public AlimentoFormPage()
    {
        InitializeComponent();
        BindingContext = new AlimentoFormViewModel();
    }
}
