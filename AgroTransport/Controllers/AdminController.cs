using System;
using System.Linq;
using System.Threading.Tasks;
using AgroTransport.Data;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using AgroTransport.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroTransport.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITripService _tripService;
        private readonly IBookingService _bookingService;
        private readonly IVehicleService _vehicleService;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ITripService tripService,
            IBookingService bookingService,
            IVehicleService vehicleService)
        {
            _context = context;
            _userManager = userManager;
            _tripService = tripService;
            _bookingService = bookingService;
            _vehicleService = vehicleService;
        }

        // GET: /Admin or /Admin/Index
        public async Task<IActionResult> Index()
        {
            var totalFarmers = await _context.Users.CountAsync(u => u.UserRole == "Farmer");
            var totalTransporters = await _context.Users.CountAsync(u => u.UserRole == "Transporter");
            var totalVehicles = await _context.Vehicles.CountAsync();
            var totalTrips = await _context.TransportTrips.CountAsync();
            var activeTrips = await _context.TransportTrips.CountAsync(t => t.TripStatus == "Scheduled");
            var totalBookings = await _context.Bookings.CountAsync();
            var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == "Pending");
            var completedBookings = await _context.Bookings.CountAsync(b => b.Status == "Completed");

            var acceptedBookings = await _context.Bookings.Where(b => b.Status == "Accepted" || b.Status == "Completed").ToListAsync();
            decimal totalPlatformVolumeKg = acceptedBookings.Sum(b => b.RequestedQuantityKg);
            decimal totalEconomicValue = acceptedBookings.Sum(b => b.TotalCost);

            var recentUsers = await _context.Users
                .OrderByDescending(u => u.ProfileCreatedAt)
                .Take(6)
                .ToListAsync();

            var recentTrips = await _context.TransportTrips
                .Include(t => t.Transporter)
                .Include(t => t.Vehicle)
                .Include(t => t.Bookings)
                .OrderByDescending(t => t.CreatedAt)
                .Take(5)
                .ToListAsync();

            var recentBookings = await _context.Bookings
                .Include(b => b.Farmer)
                .Include(b => b.Trip)
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToListAsync();

            var vehicles = await _context.Vehicles
                .Include(v => v.Transporter)
                .Take(5)
                .ToListAsync();

            // Prepare Chart.js data: Trips vs Capacity
            var tripsForChart = await _context.TransportTrips
                .OrderBy(t => t.DepartureDate)
                .Take(7)
                .ToListAsync();

            var chartLabels = tripsForChart.Select(t => $"{t.SourceLocation.Substring(0, Math.Min(4, t.SourceLocation.Length))}-{t.DestinationLocation.Substring(0, Math.Min(4, t.DestinationLocation.Length))}").ToList();
            var chartBookedCapacity = tripsForChart.Select(t => t.TotalCapacityKg - t.AvailableCapacityKg).ToList();
            var chartRevenue = tripsForChart.Select(t => t.TotalCost).ToList();

            var model = new AdminDashboardViewModel
            {
                TotalFarmers = totalFarmers,
                TotalTransporters = totalTransporters,
                TotalVehicles = totalVehicles,
                TotalTrips = totalTrips,
                ActiveTrips = activeTrips,
                TotalBookings = totalBookings,
                PendingBookings = pendingBookings,
                CompletedBookings = completedBookings,
                TotalPlatformVolumeKg = totalPlatformVolumeKg,
                TotalEconomicValue = totalEconomicValue,
                RecentUsers = recentUsers,
                RecentTrips = recentTrips,
                RecentBookings = recentBookings,
                AllVehicles = vehicles,
                ChartLabels = chartLabels,
                ChartBookedCapacityData = chartBookedCapacity,
                ChartRevenueData = chartRevenue
            };

            return View(model);
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users(string? role = null)
        {
            var query = _context.Users.AsQueryable();
            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.UserRole == role);
            }

            var users = await query.OrderByDescending(u => u.ProfileCreatedAt).ToListAsync();
            ViewBag.SelectedRole = role;
            return View(users);
        }

        // POST: /Admin/ToggleUserStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (user.UserRole == "Administrator")
            {
                TempData["ErrorMessage"] = "Administrator accounts cannot be deactivated.";
                return RedirectToAction(nameof(Users));
            }

            user.IsActive = !user.IsActive;
            await _userManager.UpdateAsync(user);

            TempData["SuccessMessage"] = $"User {user.FullName} status set to {(user.IsActive ? "Active" : "Deactivated")}.";
            return RedirectToAction(nameof(Users));
        }

        // GET: /Admin/Trips
        public async Task<IActionResult> Trips()
        {
            var trips = await _tripService.GetAllTripsAdminAsync();
            return View(trips);
        }

        // POST: /Admin/DeleteTrip
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTrip(int tripId)
        {
            var (success, message) = await _tripService.DeleteTripAsync(tripId, isAdmin: true);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Trips));
        }

        // GET: /Admin/Bookings
        public async Task<IActionResult> Bookings()
        {
            var bookings = await _bookingService.GetAllBookingsAdminAsync();
            return View(bookings);
        }

        // GET: /Admin/Vehicles
        public async Task<IActionResult> Vehicles()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAdminAsync();
            return View(vehicles);
        }
    }
}

