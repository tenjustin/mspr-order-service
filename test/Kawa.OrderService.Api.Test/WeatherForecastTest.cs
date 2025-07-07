namespace Kawa.OrderService.Api.Test;

public class WeatherForecastTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    
    public WeatherForecastTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public void GetWeather_ShouldReturn200()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // Act
        var response = client.GetAsync("/weatherforecast").Result;
        
        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }
}