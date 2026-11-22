using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AgroTransport.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AgroTransport.Data
{
    /// <summary>
    /// Seeds the AgroTransport database with initial roles, test accounts, vehicles,
    /// pooled trips, bookings, and runs SQL view/stored procedure installation.
    /// Demonstrates Unit IV (Identity & Roles) and Unit V (Database Seeding).
    /// </summary>
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

            try
            {
                // Ensure database created
                await context.Database.MigrateAsync();

                // 1. Seed Roles
                string[] roleNames = { "Administrator", "Transporter", "Farmer" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // 2. Seed Administrator
                string adminEmail = "admin@agrotransport.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        FullName = "System Administrator",
                        UserRole = "Administrator",
                        PhoneNumber = "9876543210",
                        Village = "Central Office",
                        District = "Jalandhar",
                        State = "Punjab",
                        EmailConfirmed = true,
                        ProfileCreatedAt = DateTime.UtcNow
                    };
                    var result = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Administrator");
                    }
                }

                // 3. Seed Transporters
                string t1Email = "gurpreet@transporter.com";
                var transporter1 = await userManager.FindByEmailAsync(t1Email);
                if (transporter1 == null)
                {
                    transporter1 = new ApplicationUser
                    {
                        UserName = t1Email,
                        Email = t1Email,
                        FullName = "Gurpreet Singh (Doaba Transport)",
                        UserRole = "Transporter",
                        PhoneNumber = "9814012345",
                        Village = "Urban Estate",
                        District = "Kapurthala",
                        State = "Punjab",
                        Address = "GT Road, Phagwara, Kapurthala, Punjab",
                        EmailConfirmed = true,
                        ProfileCreatedAt = DateTime.UtcNow
                    };
                    var res = await userManager.CreateAsync(transporter1, "Transporter@123");
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(transporter1, "Transporter");
                    }
                }

                string t2Email = "ramesh@transporter.com";
                var transporter2 = await userManager.FindByEmailAsync(t2Email);
                if (transporter2 == null)
                {
                    transporter2 = new ApplicationUser
                    {
                        UserName = t2Email,
                        Email = t2Email,
                        FullName = "Ramesh Kumar (Kisan Logistics)",
                        UserRole = "Transporter",
                        PhoneNumber = "9872054321",
                        Village = "Transport Nagar",
                        District = "Ludhiana",
                        State = "Punjab",
                        Address = "Link Road, Transport Nagar, Ludhiana, Punjab",
                        EmailConfirmed = true,
                        ProfileCreatedAt = DateTime.UtcNow
                    };
                    var res = await userManager.CreateAsync(transporter2, "Transporter@123");
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(transporter2, "Transporter");
                    }
                }

                // 4. Seed Farmers
                string f1Email = "harpal@farmer.com";
                var farmer1 = await userManager.FindByEmailAsync(f1Email);
                if (farmer1 == null)
                {
                    farmer1 = new ApplicationUser
                    {
                        UserName = f1Email,
                        Email = f1Email,
                        FullName = "Harpal Singh (Farmer A)",
                        UserRole = "Farmer",
                        PhoneNumber = "9855011223",
                        Village = "Chaheru",
                        District = "Kapurthala",
                        State = "Punjab",
                        Address = "Near GT Road, Village Chaheru, Tehsil Phagwara",
                        EmailConfirmed = true,
                        ProfileCreatedAt = DateTime.UtcNow
                    };
                    var res = await userManager.CreateAsync(farmer1, "Farmer@123");
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(farmer1, "Farmer");
                    }
                }

                string f2Email = "balwinder@farmer.com";
                var farmer2 = await userManager.FindByEmailAsync(f2Email);
                if (farmer2 == null)
                {
                    farmer2 = new ApplicationUser
                    {
                        UserName = f2Email,
                        Email = f2Email,
                        FullName = "Balwinder Singh (Farmer B)",
                        UserRole = "Farmer",
                        PhoneNumber = "9878033445",
                        Village = "Rawalpindi",
                        District = "Kapurthala",
                        State = "Punjab",
                        Address = "Village Rawalpindi, Near Phagwara Mandi",
                        EmailConfirmed = true,
                        ProfileCreatedAt = DateTime.UtcNow
                    };
                    var res = await userManager.CreateAsync(farmer2, "Farmer@123");
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(farmer2, "Farmer");
                    }
                }

                string f3Email = "surinder@farmer.com";
                var farmer3 = await userManager.FindByEmailAsync(f3Email);
                if (farmer3 == null)
                {
                    farmer3 = new ApplicationUser
                    {
                        UserName = f3Email,
                        Email = f3Email,
                        FullName = "Surinder Kaur (Farmer C)",
                        UserRole = "Farmer",
                        PhoneNumber = "9815077889",
                        Village = "Maheru",
                        District = "Kapurthala",
                        State = "Punjab",
                        Address = "Village Maheru, Kapurthala District",
                        EmailConfirmed = true,
                        ProfileCreatedAt = DateTime.UtcNow
                    };
                    var res = await userManager.CreateAsync(farmer3, "Farmer@123");
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(farmer3, "Farmer");
                    }
                }

                // 5. Seed Vehicles
                if (!context.Vehicles.Any() && transporter1 != null && transporter2 != null)
                {
                    var v1 = new Vehicle
                    {
                        RegistrationNumber = "PB-10-DX-9901",
                        VehicleType = "Medium Truck",
                        CapacityKg = 5000,
                        Model = "Eicher Pro 2049",
                        TransporterId = transporter1.Id,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    };

                    var v2 = new Vehicle
                    {
                        RegistrationNumber = "PB-09-AK-5512",
                        VehicleType = "Medium Truck",
                        CapacityKg = 3500,
                        Model = "Tata 407 LPT",
                        TransporterId = transporter1.Id,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    };

                    var v3 = new Vehicle
                    {
                        RegistrationNumber = "PB-08-CZ-8821",
                        VehicleType = "Pickup Truck",
                        CapacityKg = 1800,
                        Model = "Mahindra Bolero Maxi Truck Plus",
                        TransporterId = transporter2.Id,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    };

                    var v4 = new Vehicle
                    {
                        RegistrationNumber = "PB-36-J-3344",
                        VehicleType = "Tractor Trolley",
                        CapacityKg = 4000,
                        Model = "Farmtrac 60 Powermaxx Trolley",
                        TransporterId = transporter2.Id,
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow
                    };

                    context.Vehicles.AddRange(v1, v2, v3, v4);
                    await context.SaveChangesAsync();
                }

                // 6. Seed Transportation Trips & Feature 6 Pooling Bookings
                if (!context.TransportTrips.Any())
                {
                    var v1 = await context.Vehicles.FirstOrDefaultAsync(v => v.RegistrationNumber == "PB-10-DX-9901");
                    var v2 = await context.Vehicles.FirstOrDefaultAsync(v => v.RegistrationNumber == "PB-09-AK-5512");
                    var v3 = await context.Vehicles.FirstOrDefaultAsync(v => v.RegistrationNumber == "PB-08-CZ-8821");

                    if (v1 != null && v2 != null && v3 != null && transporter1 != null && transporter2 != null)
                    {
                        // Trip 1: The Master Example from Problem Statement Feature 6:
                        // Source: Phagwara, Destination: Delhi (Azadpur Mandi), Capacity: 5000 kg, Total Cost: Rs 10,000 (Rs 2.00 / kg)
                        var trip1 = new TransportTrip
                        {
                            TransporterId = transporter1.Id,
                            VehicleId = v1.Id,
                            SourceLocation = "Phagwara",
                            DestinationLocation = "Delhi",
                            MandiName = "Azadpur Mandi",
                            DepartureDate = DateTime.UtcNow.Date.AddDays(3),
                            DepartureTime = new TimeSpan(5, 0, 0),
                            TotalCapacityKg = 5000,
                            AvailableCapacityKg = 2500, // 5000 - 1000 (Farmer A accepted) - 1500 (Farmer B accepted) = 2500 kg remaining
                            TotalCost = 10000,
                            CostPerKg = 2.00m,
                            Description = "Direct morning transit to Azadpur Mandi, Delhi. Open for produce pooling (Potatoes, Onions, Vegetables, Wheat). Covered waterproof tarp equipped.",
                            TripStatus = "Scheduled",
                            CreatedAt = DateTime.UtcNow,
                            ConcurrencyStamp = Guid.NewGuid()
                        };

                        // Trip 2: Jalandhar -> Ludhiana (Dana Mandi)
                        var trip2 = new TransportTrip
                        {
                            TransporterId = transporter1.Id,
                            VehicleId = v2.Id,
                            SourceLocation = "Jalandhar",
                            DestinationLocation = "Ludhiana",
                            MandiName = "Dana Mandi Ludhiana",
                            DepartureDate = DateTime.UtcNow.Date.AddDays(2),
                            DepartureTime = new TimeSpan(6, 30, 0),
                            TotalCapacityKg = 3500,
                            AvailableCapacityKg = 3500,
                            TotalCost = 4200,
                            CostPerKg = 1.20m,
                            Description = "Transporting fresh harvest directly to Ludhiana grain market. Clean wooden floor bed.",
                            TripStatus = "Scheduled",
                            CreatedAt = DateTime.UtcNow,
                            ConcurrencyStamp = Guid.NewGuid()
                        };

                        // Trip 3: Kapurthala -> Amritsar (Amritsar Grain Market)
                        var trip3 = new TransportTrip
                        {
                            TransporterId = transporter2.Id,
                            VehicleId = v3.Id,
                            SourceLocation = "Kapurthala",
                            DestinationLocation = "Amritsar",
                            MandiName = "Amritsar Grain Market",
                            DepartureDate = DateTime.UtcNow.Date.AddDays(4),
                            DepartureTime = new TimeSpan(7, 0, 0),
                            TotalCapacityKg = 1800,
                            AvailableCapacityKg = 1800,
                            TotalCost = 2700,
                            CostPerKg = 1.50m,
                            Description = "Fast pickup truck run for small farmers and vegetable growers to Amritsar.",
                            TripStatus = "Scheduled",
                            CreatedAt = DateTime.UtcNow,
                            ConcurrencyStamp = Guid.NewGuid()
                        };

                        context.TransportTrips.AddRange(trip1, trip2, trip3);
                        await context.SaveChangesAsync();

                        // Seed Pooling Bookings (Exact Feature 6 demonstration)
                        if (farmer1 != null && farmer2 != null && farmer3 != null)
                        {
                            // Farmer A requests 1000 kg -> Rs 2000 (Accepted)
                            var b1 = new Booking
                            {
                                BookingReference = $"BK-{DateTime.UtcNow:yyyyMMdd}-001",
                                TripId = trip1.Id,
                                FarmerId = farmer1.Id,
                                ProduceType = "Potatoes (Kufri Pukhraj)",
                                RequestedQuantityKg = 1000,
                                CostPerKg = 2.00m,
                                TotalCost = 2000m, // 1000 * 2 = 2000
                                PickupVillage = "Chaheru",
                                DropoffLocation = "Shed No. 4, Azadpur Mandi",
                                SpecialInstructions = "Packed in 50kg jute bags. Loaded near Chaheru bridge.",
                                Status = "Accepted",
                                CreatedAt = DateTime.UtcNow.AddHours(-10),
                                UpdatedAt = DateTime.UtcNow.AddHours(-8)
                            };

                            // Farmer B requests 1500 kg -> Rs 3000 (Accepted)
                            var b2 = new Booking
                            {
                                BookingReference = $"BK-{DateTime.UtcNow:yyyyMMdd}-002",
                                TripId = trip1.Id,
                                FarmerId = farmer2.Id,
                                ProduceType = "Wheat (Sharbati Grain)",
                                RequestedQuantityKg = 1500,
                                CostPerKg = 2.00m,
                                TotalCost = 3000m, // 1500 * 2 = 3000
                                PickupVillage = "Rawalpindi",
                                DropoffLocation = "Gate 2, Azadpur Mandi",
                                SpecialInstructions = "Standard grain packaging. Please arrive by 5:30 AM.",
                                Status = "Accepted",
                                CreatedAt = DateTime.UtcNow.AddHours(-8),
                                UpdatedAt = DateTime.UtcNow.AddHours(-6)
                            };

                            // Farmer C requests 500 kg -> Rs 1000 (Pending)
                            var b3 = new Booking
                            {
                                BookingReference = $"BK-{DateTime.UtcNow:yyyyMMdd}-003",
                                TripId = trip1.Id,
                                FarmerId = farmer3.Id,
                                ProduceType = "Tomatoes (Hybrid)",
                                RequestedQuantityKg = 500,
                                CostPerKg = 2.00m,
                                TotalCost = 1000m, // 500 * 2 = 1000
                                PickupVillage = "Maheru",
                                DropoffLocation = "Vegetable Section, Azadpur Mandi",
                                SpecialInstructions = "Crates stacking required. Fragile fresh produce.",
                                Status = "Pending",
                                CreatedAt = DateTime.UtcNow.AddHours(-2)
                            };

                            context.Bookings.AddRange(b1, b2, b3);
                            await context.SaveChangesAsync();
                        }
                    }
                }

                // 7. Seed Transport Requests
                if (!context.TransportRequests.Any() && farmer1 != null && farmer2 != null)
                {
                    var req1 = new TransportRequest
                    {
                        FarmerId = farmer1.Id,
                        PickupLocation = "Chaheru Village, Kapurthala",
                        DestinationLocation = "Delhi",
                        MandiName = "Azadpur Mandi",
                        ProduceType = "Fresh Potatoes",
                        RequiredCapacityKg = 2000,
                        PreferredDate = DateTime.UtcNow.Date.AddDays(5),
                        EstimatedBudget = 4000,
                        AdditionalNotes = "Need transport pooling for 40 bags of high quality potatoes.",
                        Status = "Open",
                        CreatedAt = DateTime.UtcNow
                    };

                    var req2 = new TransportRequest
                    {
                        FarmerId = farmer2.Id,
                        PickupLocation = "Rawalpindi Village, Phagwara",
                        DestinationLocation = "Khanna",
                        MandiName = "Khanna Grain Market (Asia's Largest)",
                        ProduceType = "Wheat Produce",
                        RequiredCapacityKg = 1200,
                        PreferredDate = DateTime.UtcNow.Date.AddDays(6),
                        EstimatedBudget = 2500,
                        AdditionalNotes = "Looking to share tractor trolley or mini truck space towards Khanna.",
                        Status = "Open",
                        CreatedAt = DateTime.UtcNow
                    };

                    context.TransportRequests.AddRange(req1, req2);
                    await context.SaveChangesAsync();
                }

                // 8. Seed Notifications
                if (!context.Notifications.Any() && farmer1 != null && transporter1 != null)
                {
                    context.Notifications.AddRange(
                        new Notification
                        {
                            UserId = farmer1.Id,
                            Title = "Welcome to AgroTransport",
                            Message = "Your farmer account is ready! Search for transport pooling and save up to 60% on your logistics costs.",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = true,
                            NotificationType = "General"
                        },
                        new Notification
                        {
                            UserId = farmer1.Id,
                            Title = "Booking Accepted!",
                            Message = "Your pooled transport booking (BK-001) for 1000 kg has been accepted by Gurpreet Singh.",
                            CreatedAt = DateTime.UtcNow.AddHours(-8),
                            IsRead = false,
                            NotificationType = "BookingAccepted"
                        },
                        new Notification
                        {
                            UserId = transporter1.Id,
                            Title = "New Booking Request",
                            Message = "Surinder Kaur requested to pool 500 kg of Tomatoes on your Phagwara to Delhi trip.",
                            CreatedAt = DateTime.UtcNow.AddHours(-2),
                            IsRead = false,
                            NotificationType = "BookingSubmitted"
                        }
                    );
                    await context.SaveChangesAsync();
                }

                // Ensure all scheduled demo trips remain in the future for live evaluation
                var pastTrips = await context.TransportTrips.Where(t => t.TripStatus == "Scheduled" && t.DepartureDate < DateTime.UtcNow.Date).ToListAsync();
                if (pastTrips.Any())
                {
                    foreach (var pt in pastTrips)
                    {
                        pt.DepartureDate = DateTime.UtcNow.Date.AddDays(2);
                    }
                    await context.SaveChangesAsync();
                }

                // 9. Execute SQL Views and Stored Procedures creation if database exists
                try
                {
                    await ExecuteSqlScriptsAsync(context, logger);
                }
                catch (Exception sqlEx)
                {
                    logger.LogWarning(sqlEx, "SQL view/procedure initialization warning: {Message}", sqlEx.Message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the AgroTransport database: {Message}", ex.Message);
                throw;
            }
        }

        private static async Task ExecuteSqlScriptsAsync(ApplicationDbContext context, ILogger logger)
        {
            // Execute SQL Views
            string viewSql1 = @"
CREATE OR ALTER VIEW dbo.vw_ActiveTransportationTrips
AS
SELECT 
    t.Id AS TripId,
    u.FullName AS TransporterName,
    u.Email AS TransporterEmail,
    v.RegistrationNumber AS VehicleRegistration,
    v.VehicleType AS VehicleType,
    t.SourceLocation,
    t.DestinationLocation,
    t.MandiName,
    t.DepartureDate,
    t.DepartureTime,
    t.TotalCapacityKg,
    t.AvailableCapacityKg,
    t.TotalCost,
    t.CostPerKg,
    t.TripStatus,
    COUNT(CASE WHEN b.Status = 'Accepted' THEN b.Id END) AS ActiveBookingsCount
FROM dbo.TransportTrips t
INNER JOIN dbo.AspNetUsers u ON t.TransporterId = u.Id
INNER JOIN dbo.Vehicles v ON t.VehicleId = v.Id
LEFT JOIN dbo.Bookings b ON t.Id = b.TripId
WHERE t.TripStatus = 'Scheduled'
GROUP BY 
    t.Id, u.FullName, u.Email, v.RegistrationNumber, v.VehicleType,
    t.SourceLocation, t.DestinationLocation, t.MandiName,
    t.DepartureDate, t.DepartureTime, t.TotalCapacityKg,
    t.AvailableCapacityKg, t.TotalCost, t.CostPerKg, t.TripStatus;";

            string viewSql2 = @"
CREATE OR ALTER VIEW dbo.vw_TripBookingSummary
AS
SELECT 
    t.Id AS TripId,
    (t.SourceLocation + ' to ' + t.DestinationLocation + ' (' + t.MandiName + ')') AS Route,
    u.FullName AS TransporterName,
    t.TotalCapacityKg,
    ISNULL(SUM(CASE WHEN b.Status = 'Accepted' THEN b.RequestedQuantityKg ELSE 0 END), 0) AS TotalBookedKg,
    t.AvailableCapacityKg,
    COUNT(b.Id) AS TotalBookingsCount,
    COUNT(CASE WHEN b.Status = 'Accepted' THEN 1 END) AS AcceptedBookingsCount,
    ISNULL(SUM(CASE WHEN b.Status = 'Accepted' THEN b.TotalCost ELSE 0 END), 0) AS TotalCollectedRevenue,
    t.TotalCost AS TripTotalCost
FROM dbo.TransportTrips t
INNER JOIN dbo.AspNetUsers u ON t.TransporterId = u.Id
LEFT JOIN dbo.Bookings b ON t.Id = b.TripId
GROUP BY 
    t.Id, t.SourceLocation, t.DestinationLocation, t.MandiName,
    u.FullName, t.TotalCapacityKg, t.AvailableCapacityKg, t.TotalCost;";

            string spSql = @"
CREATE OR ALTER PROCEDURE dbo.usp_GetAvailableTripsByRoute
    @Source NVARCHAR(100) = NULL,
    @Destination NVARCHAR(100) = NULL,
    @MinCapacity DECIMAL(10,2) = 0,
    @DepartureDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        t.Id,
        t.SourceLocation,
        t.DestinationLocation,
        t.MandiName,
        t.DepartureDate,
        t.DepartureTime,
        t.TotalCapacityKg,
        t.AvailableCapacityKg,
        t.TotalCost,
        t.CostPerKg,
        t.Description,
        t.TripStatus,
        v.RegistrationNumber AS VehicleRegistration,
        v.VehicleType,
        v.Model AS VehicleModel,
        u.FullName AS TransporterName,
        u.PhoneNumber AS TransporterPhone
    FROM dbo.TransportTrips t
    INNER JOIN dbo.Vehicles v ON t.VehicleId = v.Id
    INNER JOIN dbo.AspNetUsers u ON t.TransporterId = u.Id
    WHERE t.TripStatus = 'Scheduled'
      AND t.AvailableCapacityKg >= @MinCapacity
      AND (@Source IS NULL OR t.SourceLocation LIKE '%' + @Source + '%')
      AND (@Destination IS NULL OR t.DestinationLocation LIKE '%' + @Destination + '%' OR t.MandiName LIKE '%' + @Destination + '%')
      AND (@DepartureDate IS NULL OR CAST(t.DepartureDate AS DATE) = @DepartureDate)
    ORDER BY t.DepartureDate ASC, t.CostPerKg ASC;
END;";

            await context.Database.ExecuteSqlRawAsync(viewSql1);
            await context.Database.ExecuteSqlRawAsync(viewSql2);
            await context.Database.ExecuteSqlRawAsync(spSql);
            logger.LogInformation("SQL Server views and stored procedures installed successfully.");
        }
    }
}

