using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace IntelligentPlant.BackgroundTasks.ExampleApp {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddAspNetCoreBackgroundTaskService();
            services.AddControllers();

            var resourceBuilder = ResourceBuilder.CreateEmpty().AddService(GetType().Assembly.GetName().Name, autoGenerateServiceInstanceId: false);

            services.AddOpenTelemetryTracing(builder => {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddSource(nameof(Controllers.TasksController))
                    .AddConsoleExporter();
            });

            services.AddOpenTelemetryMetrics(builder => {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddMeter(BackgroundTaskService.DiagnosticsSourceName)
                    .AddConsoleExporter(options => {
                        options.MetricReaderType = MetricReaderType.Periodic;
                        options.AggregationTemporality = AggregationTemporality.Cumulative;
                        options.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions() {
                            ExportIntervalMilliseconds = 5000
                        };
                    });
            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
