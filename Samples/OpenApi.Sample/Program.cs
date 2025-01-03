using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NodaTime;
using OpenApi.Extensions.Extensions;
using OpenApi.NodaTime.Extensions;
using OpenApi.Sample;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(opt =>
{
    opt.AddDescription("This project contains samples on the extensions library OpenApi.Extensions.");
    opt.ConfigureNodaTime();
    opt.AddXmlComments<Module>();
    opt.AddResponseType<ProblemDetails>(HttpStatusCode.BadRequest);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast",
        () =>
        {
            var forecast = Enumerable.Range(1, 5)
                .Select(index =>
                    new WeatherForecast(
                        LocalDate.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        summaries[Random.Shared.Next(summaries.Length)]
                    ))
                .ToArray();
            return forecast;
        })
    .WithName("GetWeatherForecast");

app.Run();

/// <summary>
/// Shows the weather forecast
/// </summary>
internal record WeatherForecast(LocalDate Date, int TemperatureC, string? Summary)
{
    /// <summary>
    /// Uses TemperatureC to calculate TemperatureF
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
