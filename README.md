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
services.AddAspNetCoreBackgroundTaskService(options => {
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
        backgroundTaskService.QueueBackgroundWorkItem(
            LongRunningTask, 
            _shutdownSource.Token
        );
    }

    private async Task LongRunningTask(CancellationToken cancellationToken) {
        while (!cancellationToken.IsCancellationRequested) {
            // Do long-running work.
        }
    }

    public void Dispose() {
        _shutdownSource.Cancel();
        _shutdownSource.Dispose();
    }

}
```


# Waiting for Background Tasks to Complete

## Using `BackgroundWorkItem.Task`

All `QueueBackgroundWorkItem` overloads return an instance of the [BackgroundWorkItem](./src/IntelligentPlant.BackgroundTasks/BackgroundWorkItem.cs) type. This type has a `Task` property that can be used to wait for the work item to complete. For example:

```csharp
public class MyClass : IAsyncDisposable {

    private readonly CancellationTokenSource _shutdownSource = new CancellationTokenSource();

    private readonly Task _workItemTask;

    public MyClass(IBackgroundTaskService backgroundTaskService) {
        var workItem = backgroundTaskService.QueueBackgroundWorkItem(
            LongRunningTask,
            _shutdownSource.Token
        );
        _workItemTask = workItem.Task;
    }

    private async Task LongRunningTask(CancellationToken cancellationToken) {
        try {
            while (!cancellationToken.IsCancellationRequested) {
                // Do long-running work.
            }
        }
        finally {
            // Perform cleanup here.
        }
    }

    public async ValueTask DisposeAsync() {
        _shutdownSource.Cancel();
        await _workItemTask;
        _shutdownSource.Dispose();
    }

}
```

## Using Synchronization Primitives

Alternatively, you can use synchronization primitives such as `SemaphoreSlim` to wait for work items to complete. For example:

```csharp
public class MyClass : IAsyncDisposable {

    private readonly CancellationTokenSource _shutdownSource = new CancellationTokenSource();

    private readonly SemaphoreSlim _workItemSemaphore = new SemaphoreSlim(0);

    public MyClass(IBackgroundTaskService backgroundTaskService) {
        backgroundTaskService.QueueBackgroundWorkItem(
            LongRunningTask,
            _shutdownSource.Token
        );
    }

    private async Task LongRunningTask(CancellationToken cancellationToken) {
        try {
            while (!cancellationToken.IsCancellationRequested) {
                // Do long-running work.
            }
        }
        finally {
            // Perform cleanup here.
            _workItemSemaphore.Release();
        }
    }

    public async ValueTask DisposeAsync() {
        _shutdownSource.Cancel();
        await _workItemSemaphore.WaitAsync();
        _shutdownSource.Dispose();
        _workItemSemaphore.Dispose();
    }

}
```

# EventSource

The `IntelligentPlant.BackgroundTasks` event source emits events when background work items are enqueued, dequeued, started, and completed. The `BackgroundTaskService.EventIds` class contains constants for the different event IDs that can be emitted. See [here](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.tracing.eventsource) for more information about event sources.


# OpenTelemetry

[OpenTelemetry](https://github.com/open-telemetry)-compatible tracing and metrics for background work items is provided via the [System.Diagnostics.DiagnosticSource](https://www.nuget.org/packages/System.Diagnostics.DiagnosticSource) NuGet package.

To enable OpenTelemetry tracing and metrics instrumentation in an ASP.NET Core application:

1. Add a reference to the [IntelligentPlant.BackgroundTasks.OpenTelemetry](https://www.nuget.org/packages/IntelligentPlant.BackgroundTasks.OpenTelemetry) NuGet package.
2. If you want to enable tracing for ASP.NET Core requests, follow the instructions for enabling ASP.NET Core OpenTelemetry instrumentation [here](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Instrumentation.AspNetCore/README.md).
3. In your `ConfigureServices` method in your `Startup.cs` file, enable instrumentation for the custom `ActivitySource` instances you want to record, and enable the collection of background task service metrics:

```csharp
using IntelligentPlant.BackgroundTasks;

// Assumes that you have already created an OpenTelemetry resource builder.

services.AddOpenTelemetry()
    .WithTracing(builder => {
        builder
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddSource("MyCompany.EmailNotifier", "MyCompany.MyClass")
            // Configure OpenTelemetry trace exporters here e.g.
            .AddConsoleExporter();
    })
    .WithMetrics(builder => {
        builder
            .SetResourceBuilder(resourceBuilder)
            .AddBackgroundTaskServiceInstrumentation()
            // Configure OpenTelemetry metrics exporters here e.g.
            .AddConsoleExporter((exporterOptions, readerOptions) => { 
                readerOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000;
                readerOptions.TemporalityPreference = MetricReaderTemporalityPreference.Cumulative;
            });
    });
```

Consider our `EmailNotifier` class from above. We can enhance this class so that it can generate `Activity` objects for use in traces:

```csharp
public class EmailNotifier {

    private static readonly ActivitySource s_activitySource { get; } = new ActivitySource("MyCompany.EmailNotifier", "1.0.0");

    private readonly IBackgroundTaskService _backgroundTaskService;


    public EmailNotifier(IBackgroundTaskService backgroundTaskService) {
        _backgroundTaskService = backgroundTaskService;
    }


    public void SendEmail(string recipient, string subject, string content) {
        _backgroundTaskService.QueueBackgroundWorkItem(async ct => {
            using (s_activitySource.StartActivity("send_email")) {
                Activity.Current?.SetTag("recipient", recipient);
                // The provided cancellation token will fire when the application is shutting down.
                await SendEmailInternal(recipient, subject, content, ct);
            }
        }, captureParentActivity: true);
    }
}
```

By setting the `captureParentActivity` parameter to `true`, we are saying that we want to capture the value of `Activity.Current` at the moment we call `QueueBackgroundWorkItem`, and then restore it as the current activity when the work item is run. This means that the activity that we create inside the work item will be created as a child of the activity that we captured, rather than being created as a new top-level activity.


## About Metrics

The following metric instruments are defined:

- `Total_Queued_Items` - total number of work items that have been added to the background task service's queue since observation began.
- `Total_Dequeued_Items` - total number of work items that the background task service has dequeued since observation began.
- `Total_Started_Items` - total number of work items that the background task service has started executing since observation began.
- `Total_Completed_Items` - total number of work items that have either run to completion or faulted since observation began.
- `Total_Completed_Items_Success` - total number of work items that have run to completion since observation began.
- `Total_Completed_Items_Fail` - total number of work items that have faulted since observation began.
- `Running_Items` - current numer of running work items.
- `Processing_Time` - histogram that records the time that a completed or faulted work item ran for.
- `Queue_Size` - current number of pending work items.

Each sample emitted by any of the above instruments has a tag named `IntelligentPlant.BackgroundTasks.Service_Name` that defines the name of the background task service that provided the sample. This can be useful in scenarios where multiple background task service instances exist in an application.
