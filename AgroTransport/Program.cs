using System;
using AgroTransport.Data;
using AgroTransport.Middleware;
using AgroTransport.Models;
using AgroTransport.Services;
using AgroTransport.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// ====================================================================
// 1. DATABASE CONFIGURATION (Unit V - SQL Server & EF Core Code First)
// ====================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=AgroTransportDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ====================================================================
// 2. IDENTITY & SECURITY (Unit IV - Authentication & Role-Based Auth)
// ====================================================================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings for academic simplicity and standard security
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;

    // User settings
    options.User.RequireUniqueEmail = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Authentication Cookie Paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
});

// ====================================================================
// 3. MVC & SESSION STATE (Unit III - Controllers, Views, Sessions)
// ====================================================================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep PascalCase or standard JSON
    });

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ====================================================================
// 4. DEPENDENCY INJECTION (Unit IV - Inversion of Control)
// ====================================================================
builder.Services.AddScoped<ICostCalculationService, CostCalculationService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<ITransportRequestService, TransportRequestService>();

var app = builder.Build();

// ====================================================================
// 5. DATABASE INITIALIZATION & SEEDING (Roles, Users, Pooling Data)
// ====================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await DbInitializer.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration/seeding.");
    }
}

// ====================================================================
// 6. HTTP REQUEST PIPELINE MIDDLEWARE (Unit IV - Request Pipeline)
// ====================================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Unit IV: Custom Request Logging Middleware
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Route Configuration (Unit III - MVC Pattern)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
