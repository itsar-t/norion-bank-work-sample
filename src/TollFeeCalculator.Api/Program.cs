using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using TollFeeCalculator.Api.Endpoints;
using TollFeeCalculator.Services;

WebApplicationBuilder builder =
    WebApplication.CreateBuilder(args);

// Add service OpenApi to new builder
builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter()
    );
});

builder.Services.AddSingleton<TollCalculator>();

WebApplication app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Activates later when frontend runs on different address,
// app.UseCors();

app.MapTollEndpoints();

app.Run();