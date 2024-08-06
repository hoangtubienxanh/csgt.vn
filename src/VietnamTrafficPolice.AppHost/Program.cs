using Projects;

using Warp.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/external-parameters
// Value set in app settings and overrides the default
// specified in the Dockerfile.

var sqlPassword = builder.AddParameter("sql-root-password", true);

var mariadb = builder.AddMySql("sqldata", sqlPassword)
    .WithImage("mariadb", "lts")
    .WithDataVolume();

var warpLicenseKey = builder.AddParameter("warp-license", true);

var warp = builder.AddWarp("warp", warpLicenseKey)
    .WithEnvironment("WARP_SLEEP", "2")
    .WithDataVolume();

// Setting up dependencies for web application
var vietnamTrafficIncidentContext = mariadb.AddDatabase("traffic-incident-context", "traffic_incident_context");

// var app = builder.AddProject<VietnamTrafficPolice_WebApi>("webapi")
//     .WithReference(vietnamTrafficIncidentContext)
//     .WithReference(warp)
//     .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.AddParameter("global-environment"))
//     .WithEnvironment("TESSERACTOPTIONS__DIRECTORYPATH", builder.AddParameter("tesseract-directory-path"))
//     .WithEnvironment("TESSERACTOPTIONS__STEMPATH", builder.AddParameter("tesseract-stem-path"));

builder.Build().Run();