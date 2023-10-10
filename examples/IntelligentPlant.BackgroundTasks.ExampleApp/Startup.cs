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

            var resourceBuilder = ResourceBuilder.CreateEmpty().AddService(GetType().Assembly.GetName().Name!, serviceInstanceId: System.Net.Dns.GetHostName());

            services.AddOpenTelemetry()
                .WithTracing(builder => {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddAspNetCoreInstrumentation()
                        .AddSource(nameof(Controllers.TasksController))
                        .AddConsoleExporter();
                })
                .WithMetrics(builder => {
                    builder
                        .SetResourceBuilder(resourceBuilder)
                        .AddBackgroundTaskServiceInstrumentation()
                        .AddConsoleExporter((exporterOptions, readerOptions) => { 
                            readerOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000;
                            readerOptions.TemporalityPreference = MetricReaderTemporalityPreference.Cumulative;
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
