# IntelligentPlant.BackgroundTasks

Simplifies the registration of fire-and-forget background tasks in ASP.NET Core.


# Getting Started

Install the `IntelligentPlant.BackgroundTasks.AspNetCore` NuGet package and register the services in your application's `Startup.cs` file:

```csharp
services.AddBackgroundTaskService();
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

By default, the `CancellationToken` provided to the work item will fire when the application is shutting down. The [BackgroundTaskServiceExtensions](./src/IntelligentPlant.BackgroundTasks/BackgroundTaskServiceExtensions.cs) class contains extension methods that allow you to specify additional `CancellationToken` instances for the work item. In these scenarios, a composite of the master token and all of the additional tokens is passed to the work item.
