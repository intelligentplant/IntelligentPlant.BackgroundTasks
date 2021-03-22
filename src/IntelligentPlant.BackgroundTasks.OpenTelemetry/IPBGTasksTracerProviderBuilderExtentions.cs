using System;

namespace OpenTelemetry.Trace {

    /// <summary>
    /// Extensions for <see cref="TracerProviderBuilder"/>
    /// </summary>
    public static class IPBGTasksTracerProviderBuilderExtentions {

        /// <summary>
        /// Adds a subscription to the <see cref="IntelligentPlant.BackgroundTasks.BackgroundTaskService.DiagnosticsSourceName"/> 
        /// activity source.
        /// </summary>
        /// <param name="builder">
        ///   The <see cref="TracerProviderBuilder"/>.
        /// </param>
        /// <returns>
        ///   The <see cref="TracerProviderBuilder"/>.
        /// </returns>
        public static TracerProviderBuilder AddIntelligentPlantBackgroundTasksInstrumentation(this TracerProviderBuilder builder) {
            if (builder == null) {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddSource(IntelligentPlant.BackgroundTasks.BackgroundTaskService.DiagnosticsSourceName);

            return builder;
        }

    }
}
