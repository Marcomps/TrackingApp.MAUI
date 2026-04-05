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

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Refresh data in the background so the page renders immediately.
        // Using Dispatcher so UI-bound collections update on the main thread.
        Dispatcher.DispatchAsync(async () =>
        {
            await AppServices.DataService.ReloadAllDataAsync();
            if (BindingContext is MainViewModel viewModel)
                viewModel.NotifyAllDataChanged();
        });
    }

    private async void OnVolverMenuClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//menu");
    }
}
