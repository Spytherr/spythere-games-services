using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using SpythereGamesServices;

namespace SpythereGamesServices.Tests.Services;

public class GoogleAuthServiceTests
{
    private static IConfiguration BuildConfig(string? clientId, string? clientSecret)
    {
        var dict = new Dictionary<string, string?>();
        if (clientId is not null) dict["GoogleAuth:ClientId"] = clientId;
        if (clientSecret is not null) dict["GoogleAuth:ClientSecret"] = clientSecret;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private static IHttpClientFactory CreateHttpClientFactory(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler);
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        return factory.Object;
    }

    private static HttpMessageHandler CreateMockHandler(HttpStatusCode statusCode, object? responseBody = null)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage(statusCode);
        if (responseBody is not null)
            response.Content = new StringContent(JsonSerializer.Serialize(responseBody), System.Text.Encoding.UTF8, "application/json");

        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);

        return mockHandler.Object;
    }

    [Fact]
    public async Task VerifyAuthCodeAsync_ReturnsNull_WhenClientIdMissing()
    {
        var config = BuildConfig(null, "secret");
        var handler = CreateMockHandler(HttpStatusCode.OK);
        var factory = CreateHttpClientFactory(handler);

        var service = new GoogleAuthService(config, factory);

        var result = await service.VerifyAuthCodeAsync("some-code");

        Assert.Null(result);
    }

    [Fact]
    public async Task VerifyAuthCodeAsync_ReturnsNull_WhenClientSecretMissing()
    {
        var config = BuildConfig("client-id", null);
        var handler = CreateMockHandler(HttpStatusCode.OK);
        var factory = CreateHttpClientFactory(handler);

        var service = new GoogleAuthService(config, factory);

        var result = await service.VerifyAuthCodeAsync("some-code");

        Assert.Null(result);
    }

    [Fact]
    public async Task VerifyAuthCodeAsync_ReturnsNull_WhenClientIdEmpty()
    {
        var config = BuildConfig("", "secret");
        var handler = CreateMockHandler(HttpStatusCode.OK);
        var factory = CreateHttpClientFactory(handler);

        var service = new GoogleAuthService(config, factory);

        var result = await service.VerifyAuthCodeAsync("some-code");

        Assert.Null(result);
    }

    [Fact]
    public async Task VerifyAuthCodeAsync_ReturnsNull_WhenClientSecretEmpty()
    {
        var config = BuildConfig("client-id", "");
        var handler = CreateMockHandler(HttpStatusCode.OK);
        var factory = CreateHttpClientFactory(handler);

        var service = new GoogleAuthService(config, factory);

        var result = await service.VerifyAuthCodeAsync("some-code");

        Assert.Null(result);
    }

    [Fact]
    public async Task VerifyAuthCodeAsync_ReturnsNull_WhenTokenEndpointFails()
    {
        var config = BuildConfig("client-id", "client-secret");
        var handler = CreateMockHandler(HttpStatusCode.BadRequest);
        var factory = CreateHttpClientFactory(handler);

        var service = new GoogleAuthService(config, factory);

        var result = await service.VerifyAuthCodeAsync("bad-code");

        Assert.Null(result);
    }

    [Fact]
    public async Task VerifyAuthCodeAsync_ReturnsNull_WhenResponseHasNoIdToken()
    {
        var config = BuildConfig("client-id", "client-secret");
        var responseBody = new { access_token = "at-123" };
        var handler = CreateMockHandler(HttpStatusCode.OK, responseBody);
        var factory = CreateHttpClientFactory(handler);

        var service = new GoogleAuthService(config, factory);

        var result = await service.VerifyAuthCodeAsync("some-code");

        Assert.Null(result);
    }

    [Fact]
    public async Task VerifyAuthCodeAsync_ReturnsNull_WhenIdTokenIsEmpty()
    {
        var config = BuildConfig("client-id", "client-secret");
        var responseBody = new { id_token = "" };
        var handler = CreateMockHandler(HttpStatusCode.OK, responseBody);
        var factory = CreateHttpClientFactory(handler);

        var service = new GoogleAuthService(config, factory);

        var result = await service.VerifyAuthCodeAsync("some-code");

        Assert.Null(result);
    }

    [Fact]
    public async Task VerifyAuthCodeAsync_ReturnsNull_WhenJwtValidationFails()
    {
        var config = BuildConfig("client-id", "client-secret");
        var responseBody = new { id_token = "invalid.jwt.token" };
        var handler = CreateMockHandler(HttpStatusCode.OK, responseBody);
        var factory = CreateHttpClientFactory(handler);

        var service = new GoogleAuthService(config, factory);

        var result = await service.VerifyAuthCodeAsync("some-code");

        Assert.Null(result);
    }

    [Fact]
    public async Task VerifyAuthCodeAsync_SendsRequestToGoogleTokenEndpoint()
    {
        var config = BuildConfig("cid", "csec");
        HttpRequestMessage? capturedRequest = null;

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => capturedRequest = req)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.BadRequest));

        var factory = CreateHttpClientFactory(mockHandler.Object);
        var service = new GoogleAuthService(config, factory);

        await service.VerifyAuthCodeAsync("test-code");

        Assert.NotNull(capturedRequest);
        Assert.Equal("https://oauth2.googleapis.com/token", capturedRequest.RequestUri?.ToString());
        Assert.Equal(HttpMethod.Post, capturedRequest.Method);
    }
}
