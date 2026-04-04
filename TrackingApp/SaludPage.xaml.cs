using TrackingApp.Services;
using TrackingApp.ViewModels;

namespace TrackingApp;

public partial class SaludPage : ContentPage
{
    public SaludPage()
    {
        InitializeComponent();
        BindingContext = new MainViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await AppServices.DataService.ReloadAllDataAsync();
        if (BindingContext is MainViewModel viewModel)
        {
            viewModel.NotifyAllDataChanged();
        }
    }

    private async void OnVolverMenuClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//menu");
    }
}
