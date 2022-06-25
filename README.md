# Shenyu .NET client

[![build](https://github.com/apache/incubator-shenyu-client-dotnet/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/apache/incubator-shenyu-client-dotnet/actions)
[![codecov.io](https://codecov.io/gh/apache/incubator-shenyu-client-dotnet/coverage.svg?branch=main)](https://app.codecov.io/gh/apache/incubator-shenyu-client-dotnet?branch=main)

## Getting Started

### ASP.NET Core project

For ASP.NET Core project, we can refer to the example code at `examples/AspNetCoreExample`. What you need to do is quite
simple and straightforward.

1. add the Shenyu ASP.NET Core dependency into project.

```shell
dotnet add package <todo>
```

2. in `Startup.ConfigureServices` method, add the `ShenyuRegister` service.

```c#
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddShenyuRegister(this.Configuration);
    ...
}
```

3. set your `Shenyu` configurations in `appsettings.json`.

```json
{
    "Shenyu": {
        "Register": {
            "ServerList": "http://localhost:9095",
            "Props": {
              "UserName": "<your_admin_user>",
              "Password": "<your_admin_password>"
            }
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

4. enable calling via ip.

When running on your local machine, ASP.NET Core service can only be called from `localhost`. To enable calling by IP, you can replace `https://localhost:{port};http://localhost:{port}` with `https://*:{port};http://*:{port}` by one of the following ways.

- Setting in `launchSettings.json`. Replace for `applicationUrl` field.
- Setting by environment variables `ASPNETCORE_URLS`. e.g. `ASPNETCORE_URLS "http://*:5000"`
- Adding `--urls` when start. e.g. `dotnet run --urls "https://*:5001;http://*:5000"`
- Setting progratically by `UseUrls()` in `Program.cs`.

e.g.

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls("http://*:5000", "https://*:5001");
            });
```

That's all! After finished above steps, you can just start your project and you can visit `shenyu-admin` portal to see the APIs have been registered in Shenyu.
