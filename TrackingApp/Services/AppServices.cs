namespace TrackingApp.Services
{
    /// <summary>
    /// Provides a singleton instance of DataService for MAUI applications.
    /// This is a MAUI-specific wrapper since DataService in Core doesn't have
    /// access to the MAUI-specific DatabaseService.
    /// </summary>
    public static class AppServices
    {
        private static DataService? _dataService;
        
        /// <summary>
        /// Gets the singleton instance of DataService using DatabaseService.Instance
        /// </summary>
        public static DataService DataService => _dataService ??= new DataService(DatabaseService.Instance);
    }
}
