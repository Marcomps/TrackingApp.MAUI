using TrackingApp.ViewModels;

namespace TrackingApp.Services
{
    /// <summary>
    /// Provides singleton instances of shared services and ViewModels.
    /// </summary>
    public static class AppServices
    {
        private static DataService? _dataService;
        private static MainViewModel? _mainViewModel;

        /// <summary>Singleton DataService backed by the SQLite DatabaseService.</summary>
        public static DataService DataService => _dataService ??= new DataService(
            DatabaseService.Instance,
            NotificationService.Instance);

        /// <summary>
        /// Singleton MainViewModel — created once and reused across pages
        /// (SaludPage, MainPage) to avoid re-subscribing to collection events
        /// on every navigation.
        /// </summary>
        public static MainViewModel MainViewModel => _mainViewModel ??= new MainViewModel();
    }
}
