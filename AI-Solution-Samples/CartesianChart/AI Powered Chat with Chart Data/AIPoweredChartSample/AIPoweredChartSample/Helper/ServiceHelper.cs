namespace AIPoweredChartSample
{
    /// <summary>
    /// ServiceHelper is a static class that provides access to the current service provider for the application.
    /// </summary>
    public static class ServiceHelper
    {
        /// <summary>
        /// Gets or sets the current service provider for the application.
        /// </summary>
        public static IServiceProvider? CurrentServices { get; set; }

        /// <summary>
        /// Method to retrieve a service of the specified type from the current service provider.
        /// </summary>
        public static IServiceProvider Services =>
            CurrentServices ?? throw new InvalidOperationException("Services not initialized yet.");
    }
}
