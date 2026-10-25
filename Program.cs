using AcademicScheduler.Components;
using AcademicScheduler.Data;
using AcademicScheduler.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AcademicSchedulerDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AcademicSchedulerDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageEnrollments", policy =>
        policy.RequireRole("Administrator"));

    options.AddPolicy("CanExportReports", policy =>
        policy.RequireRole("Administrator"));
});

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<EnrollmentService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<EnrollmentReportService>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapPost("/account/login", async (
    HttpContext httpContext,
    SignInManager<IdentityUser> signInManager,
    [FromForm] string email,
    [FromForm] string password) =>
{
    var result = await signInManager.PasswordSignInAsync(
        email,
        password,
        isPersistent: false,
        lockoutOnFailure: true);

    if (!result.Succeeded)
    {
        return Results.LocalRedirect("/login?error=invalid");
    }

    return Results.LocalRedirect("/dashboard");
});


app.MapPost("/account/logout", async (
    SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();

    return Results.LocalRedirect("/login");
})
.RequireAuthorization();


using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<AcademicSchedulerDbContext>();

    await dbContext.Database.MigrateAsync();

    var environment = scope.ServiceProvider
        .GetRequiredService<IWebHostEnvironment>();

    await DbSeeder.SeedAsync(dbContext, environment);

    await IdentitySeeder.SeedAsync(
        scope.ServiceProvider,
        app.Configuration,
        environment);
}

app.Run();
