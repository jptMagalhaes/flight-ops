using System.Globalization;
using FlightOps;
using FlightOps.Data;
using FlightOps.Entities;
using FlightOps.Features.Airports.Queries;
using FlightOps.Features.Flights.Commands;
using FlightOps.Features.Flights.Queries;
using FlightOps.Features.Flights.Scheduling;
using FlightOps.Features.Flights.Simulation;
using FlightOps.Features.Aircrafts.Queries;
using FlightOps.Features.Home.Queries;
using FlightOps.Infrastructure;
using FlightOps.Resources;
using FlightOps.Repositories.Airports;
using FlightOps.Repositories.Flights;
using FlightOps.Repositories.Aircrafts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services
    .AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (_, factory) =>
            factory.Create(typeof(SharedResources));
    });

builder.Services.AddDbContext<FlightOpsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<FlightOpsDbContext>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OperatorOnly", policy => policy.RequireRole(IdentitySeeder.OperatorRole));
    options.AddPolicy("ViewerOrOperator", policy => policy.RequireRole(IdentitySeeder.ViewerRole, IdentitySeeder.OperatorRole));
    // Secure by default: any endpoint without an explicit [Authorize]/[AllowAnonymous] requires a logged-in user.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<FlightOpsDbContext>("database");

builder.Services.AddHostedService<FlightLifecycleBackgroundService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
}

builder.Services.AddScoped<IFlightCommands, FlightCommands>();
builder.Services.AddScoped<IAircraftLocationResolver, AircraftLocationResolver>();
builder.Services.AddScoped<IFlightScheduleValidator, FlightScheduleValidator>();
builder.Services.AddScoped<IFlightLifecycleApplier, FlightLifecycleApplier>();
builder.Services.AddScoped<IAircraftDetailsQuery, AircraftDetailsQuery>();
builder.Services.AddScoped<IAirportDetailsQuery, AirportDetailsQuery>();
builder.Services.AddScoped<IFlightReportQuery, FlightReportQuery>();
builder.Services.AddScoped<IFlightCalculationPreviewQuery, FlightCalculationPreviewQuery>();
builder.Services.AddScoped<IOperationsDashboardQuery, OperationsDashboardQuery>();

builder.Services.AddScoped<IFlightSimulator, FlightSimulator>();

builder.Services.AddScoped<IFlightRepository, FlightRepository>();
builder.Services.AddScoped<IAircraftRepository, AircraftRepository>();
builder.Services.AddScoped<IAirportRepository, AirportRepository>();
builder.Services.AddScoped<IFlightTimeConverter, FlightTimeConverter>();
builder.Services.AddScoped<IUserTimeZoneAccessor, UserTimeZoneAccessor>();

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    FlightOpsDbContext db = scope.ServiceProvider.GetRequiredService<FlightOpsDbContext>();
    TimeProvider timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db, timeProvider);
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

CultureInfo[] supportedCultures =
[
    new("en"),
    new("pt-PT"),
    new("de-DE")
];

RequestLocalizationOptions localizationOptions = new()
{
    DefaultRequestCulture = new RequestCulture("en"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    RequestCultureProviders =
    [
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    ]
};

app.UseHttpsRedirection();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseRouting();

app.UseRequestLocalization(localizationOptions);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets().AllowAnonymous();

app.MapHealthChecks("/health").AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
