using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AgroTransport.Data;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using AgroTransport.ViewModels.Farmer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroTransport.Controllers
{
    [Authorize(Roles = "Farmer")]
    public class FarmerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBookingService _bookingService;
        private readonly ITripService _tripService;
        private readonly INotificationService _notificationService;
        private readonly ITransportRequestService _requestService;

        public FarmerController(
            UserManager<ApplicationUser> userManager,
            IBookingService bookingService,
            ITripService tripService,
            INotificationService notificationService,
            ITransportRequestService requestService)
        {
            _userManager = userManager;
            _bookingService = bookingService;
            _tripService = tripService;
            _notificationService = notificationService;
            _requestService = requestService;
        }

        // GET: /Farmer or /Farmer/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var farmerId = user.Id;
            var bookings = await _bookingService.GetFarmerBookingsAsync(farmerId);
            var availableTrips = await _tripService.GetAvailableTripsAsync();
            var notifications = await _notificationService.GetUserNotificationsAsync(farmerId, 5);
            var requests = await _requestService.GetFarmerRequestsAsync(farmerId);

            var activeBookings = bookings.Where(b => b.Status == "Accepted").ToList();
            var pendingBookings = bookings.Where(b => b.Status == "Pending").ToList();
            var completedBookings = bookings.Where(b => b.Status == "Completed").ToList();

            decimal totalSpent = activeBookings.Concat(completedBookings).Sum(b => b.TotalCost);
            // In pooling, estimated savings = solo truck cost minus pooled share
            decimal totalEstimatedSavings = activeBookings.Concat(completedBookings)
                .Sum(b => b.Trip != null ? (b.Trip.TotalCost - b.TotalCost) : 0);

            var model = new FarmerDashboardViewModel
            {
                FarmerName = user.FullName,
                Location = $"{user.Village}, {user.District}",
                TotalBookings = bookings.Count,
                ActiveBookings = activeBookings.Count,
                PendingBookings = pendingBookings.Count,
                CompletedBookings = completedBookings.Count,
                TotalSpent = totalSpent,
                TotalEstimatedSavings = totalEstimatedSavings,
                RecentBookings = bookings.Take(5).ToList(),
                AvailableTrips = availableTrips.Take(4).ToList(),
                RecentNotifications = notifications,
                MyRequests = requests.Take(3).ToList()
            };

            return View(model);
        }
    }
}

