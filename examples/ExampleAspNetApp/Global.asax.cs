using System;
using System.Web;
using System.Web.Http;

using IntelligentPlant.BackgroundTasks;
using IntelligentPlant.BackgroundTasks.AspNet;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ExampleAspNetApp {
    public class WebApiApplication : HttpApplication {

        internal static IBackgroundTaskService BackgroundTaskService { get; }

        internal static IServiceProvider ServiceProvider { get; }


        static WebApiApplication() {
            var services = new ServiceCollection();
            services.AddLogging(options => {
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddDebug();
                options.AddConsole();
            });
            ServiceProvider = services.BuildServiceProvider();

            BackgroundTaskService = ActivatorUtilities.CreateInstance<AspNetBackgroundTaskService>(ServiceProvider, new BackgroundTaskServiceOptions());
        }


        protected void Application_Start() {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            ((AspNetBackgroundTaskService) BackgroundTaskService).RegisterWithIIS();
        }
    }
}
