using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace IntelligentPlant.BackgroundTasks {

    /// <summary>
    /// Extensions for configuring OpenTelemetry instrumentation for <see cref="BackgroundTaskService"/>.
    /// </summary>
    public static class BackgroundTaskServiceOpenTelemetryExtensions {

        /// <summary>
        /// Adds metrics instrumentation for <see cref="IBackgroundTaskService"/> implementations 
        /// derived from <see cref="BackgroundTaskService"/>.
        /// </summary>
        /// <param name="builder">
        ///   The <see cref="MeterProviderBuilder"/>.
        /// </param>
        /// <returns>
        ///   The <see cref="MeterProviderBuilder"/>.
        /// </returns>
        public static MeterProviderBuilder AddBackgroundTaskServiceInstrumentation(this MeterProviderBuilder builder) {
            builder.AddMeter(BackgroundTaskService.DiagnosticsSourceName);
            return builder;
        }

    }
}
