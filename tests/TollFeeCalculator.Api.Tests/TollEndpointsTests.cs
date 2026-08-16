using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TollFeeCalculator.Api.Contracts;

namespace TollFeeCalculator.Api.Tests;


/*
    WebApplicationFactory starts the API in a test environment.
    This allows the complete HTTP request pipeline to be tested
    without manually starting the application.
 */
public sealed class TollEndpointsTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TollEndpointsTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Calculate_WithValidRequest_ReturnsCalculatedFee()
    {
        var request = new
        {
            vehicleType = "Car",
            passages = new[]
            {
                new DateTime(2013, 1, 2, 6, 10, 0),
                new DateTime(2013, 1, 2, 6, 40, 0),
                new DateTime(2013, 1, 2, 7, 5, 0)
            }
        };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                "/api/toll/calculate",
                request
            );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        CalculateTollResponse? result =
            await response.Content
                .ReadFromJsonAsync<CalculateTollResponse>();

        Assert.NotNull(result);
        Assert.Equal(18, result.TotalFee);


    }

     [Fact]
    public async Task Calculate_WithNoPassages_ReturnsBadRequest()
    {
        var request = new
        {
            vehicleType = "Car",
            passages = Array.Empty<DateTime>()
        };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                "/api/toll/calculate",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task Calculate_WithDifferentDates_ReturnsBadRequest()
    {
        var request = new
        {
            vehicleType = "Car",
            passages = new[]
            {
                new DateTime(2013, 1, 2, 7, 30, 0),
                new DateTime(2013, 1, 3, 8, 0, 0)
            }
        };

        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                "/api/toll/calculate",
                request);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

    }

}