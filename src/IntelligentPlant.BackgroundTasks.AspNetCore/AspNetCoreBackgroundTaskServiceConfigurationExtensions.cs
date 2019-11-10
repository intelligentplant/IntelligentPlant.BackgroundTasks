using System;
using IntelligentPlant.BackgroundTasks;
using IntelligentPlant.BackgroundTasks.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection {

    /// <summary>
    /// Dependency injection helpers.
    /// </summary>
    public static class AspNetCoreBackgroundTaskServiceConfigurationExtensions {

        /// <summary>
        /// Adds an <see cref="IBackgroundTaskService"/> registration and supporting services to 
        /// the service collection.
        /// </summary>
        /// <param name="services">
        ///   The service collection.
        /// </param>
        /// <returns>
        ///   The service collection.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="services"/> is <see langword="null"/>.
        /// </exception>
        public static IServiceCollection AddBackgroundTaskService(this IServiceCollection services) {
            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddSingleton<IBackgroundTaskService, DefaultBackgroundTaskService>();
            services.AddHostedService<AspNetCoreBackgroundTaskServiceRunner>();

            return services;
        }

    }
}
