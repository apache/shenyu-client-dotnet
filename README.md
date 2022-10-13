# ShenYu .NET client

[![build](https://github.com/apache/shenyu-client-dotnet/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/apache/shenyu-client-dotnet/actions)
[![codecov.io](https://codecov.io/gh/apache/shenyu-client-dotnet/coverage.svg?branch=main)](https://app.codecov.io/gh/apache/shenyu-client-dotnet?branch=main)

ShenYu .NET client can help you register your ASP.NET Core applications into ShenYu, similar with Java client. It
supports below registration type,

-   http registration
-   zookeeper registration
-   nacos registration
-   consul registration
-   etcd registration

## Getting Started

Please visit related document to start to start.

For http registration, please visit [HTTP Registration](./docs/http_registration.md).

For zookeeper registration, please visit [Zookeeper Registration](./docs/zookeeper_registration.md).

For consul registration, please visit [Consul Registration](./docs/consul_registration.md).

For nacos registration, please visit [Nacos Registration](./docs/nacos_registration.md).

For etcd registration, please visit [Etcd Registration](./docs/etcd_registration.md).

## Attributes

You can use `ShenyuClient` attribute to register your APIs.

e.g. add `ShenyuClient` attribute in class as route prefix.

```csharp
[ShenyuClient("/test/**")]
public class TestController {
  ...
}
```

e.g. add `ShenyuClient` attribute in method as route path. The final route path will be `/test/hello` for this endpoint.

```csharp
[ShenyuClient("/test")]
public class TestController {
    [ShenyuClient("hello")]
    public IEnumerable<WeatherForecast> GetTest()
    {
        ...
    }
    ...
}
```
