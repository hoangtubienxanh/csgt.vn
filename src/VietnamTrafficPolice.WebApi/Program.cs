using Scalar.AspNetCore;

using VietnamTrafficPolice.ServiceDefaults;
using VietnamTrafficPolice.WebApi.Apis;
using VietnamTrafficPolice.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi
builder.AddServiceDefaults();

builder.AddApplicationServices();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/openapi", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
    app.UseDeveloperExceptionPage();
    app.MapTroubleshootApi();
}

app.MapTrafficIncidentApi();

app.Run();