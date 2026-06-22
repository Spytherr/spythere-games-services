using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SpythereGamesServices;

namespace SpythereGamesServices.Tests.Middleware;

public class ApiKeyMiddlewareTests
{
    private static IConfiguration BuildConfig(string? apiKey)
    {
        var dict = new Dictionary<string, string?>();
        if (apiKey is not null)
            dict["ApiSettings:ApiKey"] = apiKey;
        return new ConfigurationBuilder().AddInMemoryCollection(dict).Build();
    }

    private static (ApiKeyMiddleware middleware, DefaultHttpContext context, bool[] nextCalled) CreateMiddleware(
        string? configuredApiKey,
        string method = "POST",
        string path = "/api/players",
        string? providedApiKey = null)
    {
        var nextCalled = new[] { false };
        RequestDelegate next = _ =>
        {
            nextCalled[0] = true;
            return Task.CompletedTask;
        };

        var config = BuildConfig(configuredApiKey);
        var middleware = new ApiKeyMiddleware(next, config);

        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.Path = path;
        if (providedApiKey is not null)
            context.Request.Headers["X-Api-Key"] = providedApiKey;

        context.Response.Body = new MemoryStream();

        return (middleware, context, nextCalled);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    public async Task SkipsValidation_ForGetAndHeadRequests(string method)
    {
        var (middleware, context, nextCalled) = CreateMiddleware("secret-key", method: method);

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled[0]);
    }

    [Theory]
    [InlineData("POST")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task RequiresApiKey_ForMutatingMethods(string method)
    {
        var (middleware, context, nextCalled) = CreateMiddleware("secret-key", method: method);

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled[0]);
        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task SkipsValidation_ForHealthEndpoint()
    {
        var (middleware, context, nextCalled) = CreateMiddleware("secret-key", method: "POST", path: "/api/health");

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled[0]);
    }

    [Fact]
    public async Task Returns401_WhenApiKeyHeaderMissing()
    {
        var (middleware, context, nextCalled) = CreateMiddleware("secret-key", method: "POST");

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled[0]);
        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task Returns401_WhenApiKeyIsWrong()
    {
        var (middleware, context, nextCalled) = CreateMiddleware("correct-key", method: "POST", providedApiKey: "wrong-key");

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled[0]);
        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task Returns401_WhenConfiguredKeyIsEmpty()
    {
        var (middleware, context, nextCalled) = CreateMiddleware("", method: "POST", providedApiKey: "any-key");

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled[0]);
        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task Returns401_WhenConfiguredKeyIsNull()
    {
        var (middleware, context, nextCalled) = CreateMiddleware(null, method: "POST", providedApiKey: "any-key");

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled[0]);
        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task PassesThrough_WhenApiKeyIsCorrect()
    {
        var (middleware, context, nextCalled) = CreateMiddleware("my-secret", method: "POST", providedApiKey: "my-secret");

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled[0]);
    }

    [Fact]
    public async Task ApiKeyComparison_IsCaseSensitive()
    {
        var (middleware, context, nextCalled) = CreateMiddleware("MySecret", method: "POST", providedApiKey: "mysecret");

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled[0]);
        Assert.Equal(401, context.Response.StatusCode);
    }
}
