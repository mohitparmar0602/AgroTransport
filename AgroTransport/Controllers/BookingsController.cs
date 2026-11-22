using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using AgroTransport.ViewModels.Bookings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroTransport.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ITripService _tripService;
        private readonly ICostCalculationService _costCalculator;

        public BookingsController(
            IBookingService bookingService,
            ITripService tripService,
            ICostCalculationService costCalculator)
        {
            _bookingService = bookingService;
            _tripService = tripService;
            _costCalculator = costCalculator;
        }

        // GET: /Bookings/Create?tripId=1
        [HttpGet]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> Create(int tripId)
        {
            var trip = await _tripService.GetTripDetailsAsync(tripId);
            if (trip == null) return NotFound();

            if (trip.TripStatus != "Scheduled" || trip.AvailableCapacityKg <= 0)
            {
                TempData["ErrorMessage"] = "This trip is currently unavailable for new bookings.";
                return RedirectToAction("Details", "Trips", new { id = tripId });
            }

            var farmerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            // Transporters cannot book own trips
            if (trip.TransporterId == farmerId)
            {
                TempData["ErrorMessage"] = "You cannot book your own transportation trip.";
                return RedirectToAction("Details", "Trips", new { id = tripId });
            }

            var model = new CreateBookingViewModel
            {
                TripId = trip.Id,
                SourceLocation = trip.SourceLocation,
                DestinationLocation = trip.DestinationLocation,
                MandiName = trip.MandiName,
                DepartureDate = trip.DepartureDate,
                DepartureTime = trip.DepartureTime,
                VehicleType = trip.Vehicle?.VehicleType ?? "Truck",
                VehicleModel = trip.Vehicle?.Model ?? "Commercial Vehicle",
                TransporterName = trip.Transporter?.FullName ?? "Transporter",
                TotalCapacityKg = trip.TotalCapacityKg,
                AvailableCapacityKg = trip.AvailableCapacityKg,
                TotalTripCost = trip.TotalCost,
                CostPerKg = trip.CostPerKg,
                RequestedQuantityKg = Math.Min(100, trip.AvailableCapacityKg)
            };

            return View(model);
        }

        // POST: /Bookings/Create
        [HttpPost]
        [Authorize(Roles = "Farmer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingViewModel model)
        {
            var farmerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (!ModelState.IsValid)
            {
                var trip = await _tripService.GetTripDetailsAsync(model.TripId);
                if (trip != null)
                {
                    model.SourceLocation = trip.SourceLocation;
                    model.DestinationLocation = trip.DestinationLocation;
                    model.MandiName = trip.MandiName;
                    model.DepartureDate = trip.DepartureDate;
                    model.DepartureTime = trip.DepartureTime;
                    model.VehicleType = trip.Vehicle?.VehicleType ?? "Truck";
                    model.VehicleModel = trip.Vehicle?.Model ?? "Commercial Vehicle";
                    model.TransporterName = trip.Transporter?.FullName ?? "Transporter";
                    model.TotalCapacityKg = trip.TotalCapacityKg;
                    model.AvailableCapacityKg = trip.AvailableCapacityKg;
                    model.TotalTripCost = trip.TotalCost;
                    model.CostPerKg = trip.CostPerKg;
                }
                return View(model);
            }

            var (success, message, bookingId) = await _bookingService.CreateBookingRequestAsync(
                model.TripId,
                farmerId,
                model.ProduceType,
                model.RequestedQuantityKg,
                model.PickupVillage,
                model.DropoffLocation,
                model.SpecialInstructions);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                var trip = await _tripService.GetTripDetailsAsync(model.TripId);
                if (trip != null)
                {
                    model.AvailableCapacityKg = trip.AvailableCapacityKg;
                    model.CostPerKg = trip.CostPerKg;
                }
                return View(model);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Details), new { id = bookingId });
        }

        // GET: /Bookings/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            bool isFarmer = booking.FarmerId == currentUserId;
            bool isTransporter = booking.Trip?.TransporterId == currentUserId;
            bool isAdmin = User.IsInRole("Administrator");

            if (!isFarmer && !isTransporter && !isAdmin)
            {
                return Forbid();
            }

            ViewBag.IsFarmer = isFarmer;
            ViewBag.IsTransporter = isTransporter;
            ViewBag.IsAdmin = isAdmin;

            return View(booking);
        }

        // GET: /Bookings/MyBookings
        [HttpGet]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> MyBookings()
        {
            var farmerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var bookings = await _bookingService.GetFarmerBookingsAsync(farmerId);
            return View(bookings);
        }

        // POST: /Bookings/Accept
        [HttpPost]
        [Authorize(Roles = "Transporter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int bookingId, string? remarks, string? returnUrl = null)
        {
            var transporterId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, message) = await _bookingService.AcceptBookingAsync(bookingId, transporterId, remarks);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id = bookingId });
        }

        // POST: /Bookings/Reject
        [HttpPost]
        [Authorize(Roles = "Transporter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int bookingId, string? reason, string? returnUrl = null)
        {
            var transporterId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, message) = await _bookingService.RejectBookingAsync(bookingId, transporterId, reason);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Details), new { id = bookingId });
        }

        // POST: /Bookings/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            bool isAdmin = User.IsInRole("Administrator");

            var (success, message) = await _bookingService.CancelBookingAsync(bookingId, userId, isAdmin);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Details), new { id = bookingId });
        }
    }
}

