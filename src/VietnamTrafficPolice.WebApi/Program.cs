
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/openapi

var services = builder.Services;
var configuration = builder.Configuration;
services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/openapi", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.Run();