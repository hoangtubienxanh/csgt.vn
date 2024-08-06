# Aspire.Hosting.Warp library

Provides extension methods and resource definitions for a .NET Aspire AppHost to configure a WARP resource.

## Getting started

### Install the package

In your AppHost project, install the .NET Aspire Warp Hosting library with

```dotnetcli
dotnet add reference $(ProjectRootPath)/Aspire.Hosting.Warp.csproj
```

## Usage example

Then, in the _Program.cs_ file of `AppHost`, add a Warp resource and consume the connection using the following methods:

```csharp
var warp = builder.AddWarp("warp");

var myService = builder.AddProject<Projects.MyService>()
                       .WithReference(warp);
```

### Persistent Application Data

To retain WARP's data and configuration across application restarts, add a data volume or bind mount when adding the
Warp
component. This tells the WARP container where to store WARP's data and configuration on the host file system.

A [data volume](https://docs.docker.com/storage/volumes/) creates persistent storage for the WARP container managed by
the container runtime:

```csharp
var warp = builder.AddWarp("warp")
    .WithLicenseKey();
    .WithDataVolume();
```

A data [bind mount](https://docs.docker.com/storage/bind-mounts/) maps a local directory into the container. Note that
the specified directory must already exist:

```csharp
var warp = builder.AddWarp("warp")
    .WithLicenseKey();
    .WithDataBindMound(".idea/bindMount/warp");
```

## Additional documentation

https://blog.caomingjun.com/run-cloudflare-warp-in-docker/en/
