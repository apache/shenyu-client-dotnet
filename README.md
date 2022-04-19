# Shenyu .NET client

## Getting Started

### ASP.NET Core project

For ASP.NET Core project, we can refer to the example code at `examples/AspNetCoreExample`. What you need to do is quite
simple and straightforward.

First, add the Shenyu ASP.NET Core dependency into project.

```shell
dotnet add package <todo>
```

Then, in `Startup.ConfigureServices` method, add the `ShenyuRegister` service.

```c#
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddShenyuRegister(this.Configuration);
    ...
}
```

Finnaly, set your `Shenyu` configurations in `appsettings.json`.

```json
{
    "Shenyu": {
        "Register": {
            "ServerList": "http://localhost:9095",
            "UserName": "<your_admin_user>",
            "Password": "<your_admin_password>"
        },
        "Client": {
            "AppName": "dotnet-example",
            "ContextPath": "/dotnet",
            "IsFull": false,
            "ClientType": "http"
        }
    }
}
```

That's all! After finished above steps, you can just start your project and you can visit `shenyu-admin` portal to see the APIs have been registered in Shenyu.
