# IntelligentPlant.BackgroundTasks

Simplifies the registration of fire-and-forget background tasks in .NET applications.


# Getting Started

For ASP.NET Core, add a reference to the [IntelligentPlant.BackgroundTasks.AspNetCore](https://www.nuget.org/packages/IntelligentPlant.BackgroundTasks.AspNetCore/) NuGet package and register the services in your application's `Startup.cs` file:

```csharp
services.AddAspNetCoreBackgroundTaskService();
```

Logging is automatically added for all events related to work items (enqueueing, dequeueing, running, completion, errors).

You can also configure the [BackgroundTaskServiceOptions](./src/IntelligentPlant.BackgroundTasks/BackgroundTaskServiceOptions.cs) passed to the service to perform additional event handling as follows:

```csharp
services.AddBackgroundTaskService(options => {
    options.OnRunning = (workItem) => {
        // Add custom logic here...
    };

    options.OnError = (workItem, error) => {
        // Add custom logic here...
    }
});
```


# Registering Background Tasks

Inject the [IBackgroundTaskService](./src/IntelligentPlant.BackgroundTasks/IBackgroundTaskService.cs) service into the class that you want to register the task from. Use one of the `QueueBackgroundWorkItem` overloads to enqueue the task:


```csharp
public class EmailNotifier {
    private readonly IBackgroundTaskService _backgroundTaskService;


    public EmailNotifier(IBackgroundTaskService backgroundTaskService) {
        _backgroundTaskService = backgroundTaskService;
    }


    public void SendEmail(string recipient, string subject, string content) {
        _backgroundTaskService.QueueBackgroundWorkItem(async ct => {
            // The provided cancellation token will fire when the application is shutting down.
            await SendEmailInternal(recipient, subject, content, ct);
        });
    }
}
```

By default, the `CancellationToken` provided to the work item will fire when the application is shutting down. The [BackgroundTaskServiceExtensions](./src/IntelligentPlant.BackgroundTasks/BackgroundTaskServiceExtensions.cs) class contains extension methods that allow you to specify additional `CancellationToken` instances for the work item. In these scenarios, a composite of the master token and all of the additional tokens is passed to the work item. Examples of when you might want to use this functionality include starting a long-running background task from an object that should stop when the object is disposed, e.g.

```csharp
public class MyClass : IDisposable {

    private readonly CancellationTokenSource _shutdownSource = new CancellationTokenSource();

    public MyClass(IBackgroundTaskService backgroundTaskService) {
        backgroundTaskService.QueueBackgroundWorkItem(LongRunningTask, _shutdownSource.Token);
    }

    private async Task LongRunningTask(CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {
            // Do long-running work. The cancellation token will fire when either 
            // _shutdownSource is cancelled, or the IBackgroundTaskService is shut 
            // down.
        }
    }

    public void Dispose() {
        _shutdownSource.Cancel();
        _shutdownSource.Dispose();
    }

}
```


# EventSource

The `IntelligentPlant.BackgroundTasks` event source emits events when background work items are enqueued, dequeued, started, and completed. The `BackgroundTaskService.EventIds` class contains constants for the different event IDs that can be emitted. See [here](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventsource) for more information about event sources.


# EventCounters

When running on .NET Core 3.0 or later, event counters are available via the `IntelligentPlant.BackgroundTasks` event source. See [here](https://docs.microsoft.com/en-us/dotnet/core/diagnostics/event-counters) for more information about .NET event counters.


# OpenTelemetry

The `IntelligentPlant.BackgroundTasks` activity source is used to generate [OpenTelemetry](https://github.com/open-telemetry)-compatible instrumentation for background work items via the [System.Diagnostics.DiagnosticSource](https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource) NuGet package.

To configure OpenTelemetry instrumentation for an ASP.NET Core application:

1. Follow the instructions for enabling ASP.NET Core OpenTelemetry instrumentation [here](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore/README.md).
2. Add a reference to the [IntelligentPlant.BackgroundTasks.OpenTelemetry](https://www.nuget.org/packages/IntelligentPlant.BackgroundTasks.OpenTelemetry) NuGet package.
3. In your `ConfigureServices` method in your `Startup.cs` file, enable instrumentation for `IntelligentPlant.BackgroundTasks`:

```csharp
services.AddOpenTelemetryTracing(builder => {
    builder
        .AddAspNetCoreInstrumentation()
        .AddIntelligentPlantBackgroundTasksInstrumentation()
        // Your exporters here e.g.
        .AddConsoleExporter();
});
```
