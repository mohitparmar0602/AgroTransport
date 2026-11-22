using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using AgroTransport.ViewModels.Transporter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AgroTransport.Controllers
{
    [Authorize(Roles = "Transporter")]
    public class TransporterController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IVehicleService _vehicleService;
        private readonly ITripService _tripService;
        private readonly IBookingService _bookingService;
        private readonly INotificationService _notificationService;
        private readonly ITransportRequestService _requestService;

        public TransporterController(
            UserManager<ApplicationUser> userManager,
            IVehicleService vehicleService,
            ITripService tripService,
            IBookingService bookingService,
            INotificationService notificationService,
            ITransportRequestService requestService)
        {
            _userManager = userManager;
            _vehicleService = vehicleService;
            _tripService = tripService;
            _bookingService = bookingService;
            _notificationService = notificationService;
            _requestService = requestService;
        }

        // GET: /Transporter or /Transporter/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var transporterId = user.Id;
            var vehicles = await _vehicleService.GetTransporterVehiclesAsync(transporterId);
            var trips = await _tripService.GetTripsByTransporterAsync(transporterId);
            var bookings = await _bookingService.GetTransporterBookingsAsync(transporterId);
            var notifications = await _notificationService.GetUserNotificationsAsync(transporterId, 5);
            var marketRequests = await _requestService.GetOpenRequestsAsync();

            var pendingBookings = bookings.Where(b => b.Status == "Pending").ToList();
            var acceptedBookings = bookings.Where(b => b.Status == "Accepted").ToList();
            var completedTrips = trips.Where(t => t.TripStatus == "Completed").ToList();
            var activeTrips = trips.Where(t => t.TripStatus == "Scheduled" || t.TripStatus == "InTransit").ToList();

            decimal totalEarnings = acceptedBookings.Sum(b => b.TotalCost);
            decimal totalCapacityManaged = vehicles.Sum(v => v.CapacityKg);

            var model = new TransporterDashboardViewModel
            {
                TransporterName = user.FullName,
                TotalVehicles = vehicles.Count,
                TotalTrips = trips.Count,
                ActiveTrips = activeTrips.Count,
                CompletedTrips = completedTrips.Count,
                PendingRequestsCount = pendingBookings.Count,
                TotalEarnings = totalEarnings,
                TotalCapacityManagedKg = totalCapacityManaged,
                Vehicles = vehicles,
                Trips = trips.Take(5).ToList(),
                PendingBookings = pendingBookings,
                RecentNotifications = notifications,
                OpenMarketRequests = marketRequests.Take(3).ToList()
            };

            return View(model);
        }

        // GET: /Transporter/TripBookings/5
        public async Task<IActionResult> TripBookings(int id)
        {
            var transporterId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var trip = await _tripService.GetTripDetailsAsync(id);

            if (trip == null) return NotFound();
            if (trip.TransporterId != transporterId) return Forbid();

            var bookings = await _bookingService.GetTripBookingsAsync(id, transporterId);
            ViewBag.Trip = trip;

            return View(bookings);
        }
    }
}

