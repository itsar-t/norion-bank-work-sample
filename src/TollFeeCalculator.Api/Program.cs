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

const string FrontendCorsPolicy = "Frontend";

builder.Services.AddCors(options =>
{

    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

WebApplication app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

/*
    Allow the local Next.js application to call the API during
    development. Production origins will be configured separately
    when the applications are deployed.
 */

app.UseCors(FrontendCorsPolicy);

app.MapTollEndpoints();

app.Run();

/*
    Top-level statements generate the Program class automatically.
    This public partial declaration makes it accessible to the API
    integration test project through WebApplicationFactory<Program>.
 */
public partial class Program
{
}