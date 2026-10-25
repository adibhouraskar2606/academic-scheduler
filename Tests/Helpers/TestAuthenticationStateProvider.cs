
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace AcademicScheduler.Tests.Helpers;

public class TestAuthenticationStateProvider
    : AuthenticationStateProvider
{
    private readonly AuthenticationState _authenticationState;

    public TestAuthenticationStateProvider(string? role = "Administrator")
    {
        ClaimsPrincipal user;

        if (role is null)
        {
            user = new ClaimsPrincipal(
                new ClaimsIdentity());
        }
        else
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "test-user"),
                new Claim(ClaimTypes.Role, role)
            };

            user = new ClaimsPrincipal(
                new ClaimsIdentity(claims, "TestAuthentication"));
        }

        _authenticationState = new AuthenticationState(user);
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(_authenticationState);
    }
}
