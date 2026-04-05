using TrackingApp.Services;
using TrackingApp.ViewModels;

namespace TrackingApp;

public partial class SaludPage : ContentPage
{
    public SaludPage()
    {
        InitializeComponent();
        // Reuse the singleton ViewModel — no extra event subscriptions on revisit
        BindingContext = AppServices.MainViewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Light reload: only medications + doses + profiles — skips food/appointments/growth
        Dispatcher.DispatchAsync(async () =>
        {
            await AppServices.DataService.ReloadMedicationDataAsync();
            AppServices.MainViewModel.NotifyAllDataChanged();
        });
    }

    private async void OnVolverMenuClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//menu");
    }
}
