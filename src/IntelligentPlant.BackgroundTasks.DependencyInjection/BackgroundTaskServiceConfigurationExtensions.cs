using System;

using IntelligentPlant.BackgroundTasks;

namespace Microsoft.Extensions.DependencyInjection {

    /// <summary>
    /// Extensions for registering <see cref="IBackgroundTaskService"/> services.
    /// </summary>
    public static class BackgroundTaskServiceConfigurationExtensions {

        /// <summary>
        /// Adds <see cref="BackgroundTaskService.Default"/> as the registered <see cref="IBackgroundTaskService"/>.
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
        public static IServiceCollection AddDefaultBackgroundTaskService(this IServiceCollection services) {
            return services.AddBackgroundTaskService(BackgroundTaskService.Default);
        }


        /// <summary>
        /// Adds the specified implementation as the registered <see cref="IBackgroundTaskService"/>. 
        /// Note that the implementation must be externally initialised before it will be able to 
        /// process queued work items.
        /// </summary>
        /// <param name="services">
        ///   The service collection.
        /// </param>
        /// <param name="implementationInstance">
        ///   The <see cref="IBackgroundTaskService"/> implementation instance to use.
        /// </param>
        /// <returns>
        ///   The service collection.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="services"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="implementationInstance"/> is <see langword="null"/>.
        /// </exception>
        public static IServiceCollection AddBackgroundTaskService(this IServiceCollection services, IBackgroundTaskService implementationInstance) {
            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }
            if (implementationInstance == null) {
                throw new ArgumentNullException(nameof(implementationInstance));
            }

            services.AddSingleton(implementationInstance);

            return services;
        }


        /// <summary>
        /// Adds an <see cref="IBackgroundTaskService"/> registration and supporting services to 
        /// the service collection. Note that the <see cref="IBackgroundTaskService"/> must be 
        /// externally initialised before it will be able to process queued work items.
        /// </summary>
        /// <typeparam name="T">
        ///   The background service implementation type.
        /// </typeparam>
        /// <param name="services">
        ///   The service collection.
        /// </param>
        /// <param name="configure">
        ///   A delegate that can be used to configure the <see cref="BackgroundTaskServiceOptions"/> 
        ///   for the service.
        /// </param>
        /// <returns>
        ///   The service collection.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   <paramref name="services"/> is <see langword="null"/>.
        /// </exception>
        public static IServiceCollection AddBackgroundTaskService<T>(
            this IServiceCollection services,
            Action<BackgroundTaskServiceOptions>? configure = null
        ) where T : class, IBackgroundTaskService {
            if (services == null) {
                throw new ArgumentNullException(nameof(services));
            }

            var options = new BackgroundTaskServiceOptions();
            configure?.Invoke(options);
            services.AddSingleton(options);
            services.AddSingleton<IBackgroundTaskService, T>();

            return services;
        }

    }
}
