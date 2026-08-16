using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HomeChef.Api.Common;
using HomeChef.Application.Features.Auth.Contracts;

namespace HomeChef.Api.Tests;

public class AuthEndpointsTests : IClassFixture<HomeChefApiFactory>
{
    private readonly HomeChefApiFactory _factory;

    public AuthEndpointsTests(HomeChefApiFactory factory)
    {
        _factory = factory;
    }

    private static RegisterRequest NewRegister(
        string role = "Customer",
        string? email = null)
    {
        var uniqueEmail = email ?? $"it-{Guid.NewGuid():N}@test.com";
        return new RegisterRequest
        {
            FirstName = "Integration",
            LastName = "Tester",
            Email = uniqueEmail,
            Password = "Password123",
            Role = role,
        };
    }

    private static LoginRequest NewLogin(string email, string password) =>
        new()
        {
            Email = email,
            Password = password,
        };

    [Fact]
    public async Task Register_ReturnsCreated_SetsAuthCookie_AndMeWorks()
    {
        var client = _factory.CreateClient();
        var request = NewRegister();

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var registered = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        Assert.NotNull(registered?.Data);
        Assert.Equal(request.Email, registered!.Data.Email);
        Assert.Contains("Customer", registered.Data.Roles);

        Assert.True(registerResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies, cookie => cookie.StartsWith("HomeChef.Auth="));

        var meResponse = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var me = await meResponse.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        Assert.Equal(registered.Data.Id, me!.Data.Id);
        Assert.Equal(request.Email, me.Data.Email);
    }

    [Fact]
    public async Task Me_WithoutAuthentication_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var request = NewRegister();

        var first = await client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var second = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        var error = await second.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("EMAIL_TAKEN", error!.Error.Code);
    }

    [Fact]
    public async Task Register_InvalidRole_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var request = NewRegister(role: "Admin");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("INVALID_ROLE", error!.Error.Code);
    }

    [Fact]
    public async Task Register_ChefRole_ReturnsChefRole()
    {
        var client = _factory.CreateClient();
        var request = NewRegister(role: "Chef");

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var registered = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        Assert.Contains("Chef", registered!.Data.Roles);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsInvalidCredentials()
    {
        var client = _factory.CreateClient();
        var request = NewRegister();
        var registered = await client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);

        var login = NewLogin(request.Email, "WrongPassword1");
        var response = await client.PostAsJsonAsync("/api/auth/login", login);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.Equal("INVALID_CREDENTIALS", error!.Error.Code);
    }

    [Fact]
    public async Task Login_CorrectPassword_ReturnsUserAndCookie()
    {
        var client = _factory.CreateClient();
        var request = NewRegister();
        var registered = await client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);

        var login = NewLogin(request.Email, request.Password);
        var response = await client.PostAsJsonAsync("/api/auth/login", login);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies, cookie => cookie.StartsWith("HomeChef.Auth="));

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        Assert.Equal(request.Email, body!.Data.Email);
    }

    [Fact]
    public async Task Logout_ReturnsNoContent_AndInvalidatesSessionCookie()
    {
        var client = _factory.CreateClient();
        var request = NewRegister();
        var registered = await client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);

        var logoutResponse = await client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        Assert.True(logoutResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
        Assert.Contains(cookies, cookie => cookie.StartsWith("HomeChef.Auth="));

        var meResponse = await client.GetAsync("/api/users/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
    }

    [Fact]
    public async Task BearerToken_AlsoAuthenticates_MeEndpoint()
    {
        var client = _factory.CreateClient();
        var request = NewRegister();
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", request);
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var setCookie = registerResponse.Headers.GetValues("Set-Cookie").First();
        var token = setCookie.Split(';')[0].Split('=', 2)[1];

        var bearerClient = _factory.CreateClient();
        bearerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await bearerClient.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
        Assert.Equal(request.Email, me!.Data.Email);
    }
}