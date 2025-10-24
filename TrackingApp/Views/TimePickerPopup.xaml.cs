using Microsoft.Maui.Controls;

namespace TrackingApp.Views
{
    public partial class TimePickerPopup : ContentPage
    {
        public TimeSpan? SelectedTime { get; private set; }

        public TimePickerPopup(TimeSpan? initialTime = null)
        {
            InitializeComponent();
            TimePicker.Time = initialTime ?? DateTime.Now.TimeOfDay;
        }

        private async void OnAcceptClicked(object sender, EventArgs e)
        {
            SelectedTime = TimePicker.Time;
            await Navigation.PopModalAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            SelectedTime = null;
            await Navigation.PopModalAsync();
        }
    }
}
