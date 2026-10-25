
using AcademicScheduler.Data;
using AcademicScheduler.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicScheduler.Tests.Helpers;

public static class EnrollmentServiceFactory
{
    public static EnrollmentService Create(
        AcademicSchedulerDbContext dbContext,
        string? role = "Administrator")
    {
        var services = new ServiceCollection();

        // Required by ASP.NET Core's authorization services.
        services.AddLogging();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("CanManageEnrollments", policy =>
                policy.RequireRole("Administrator"));
        });

        var provider = services.BuildServiceProvider();

        var authorizationService = provider
            .GetRequiredService<IAuthorizationService>();

        var authenticationStateProvider =
            new TestAuthenticationStateProvider(role);

        return new EnrollmentService(
            dbContext,
            authorizationService,
            authenticationStateProvider);
    }
}
