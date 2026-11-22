using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using AgroTransport.ViewModels.Trips;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroTransport.Controllers
{
    public class TripsController : Controller
    {
        private readonly ITripService _tripService;
        private readonly IVehicleService _vehicleService;
        private readonly IBookingService _bookingService;

        public TripsController(
            ITripService tripService,
            IVehicleService vehicleService,
            IBookingService bookingService)
        {
            _tripService = tripService;
            _vehicleService = vehicleService;
            _bookingService = bookingService;
        }

        // GET: /Trips/Search?source=Phagwara&destination=Delhi
        [HttpGet]
        public async Task<IActionResult> Search(
            string? source = null,
            string? destination = null,
            string? mandi = null,
            DateTime? departureDate = null,
            string? vehicleType = null,
            decimal? minCapacity = null)
        {
            var trips = await _tripService.GetAvailableTripsAsync(
                source, destination, mandi, departureDate, vehicleType, minCapacity);

            var model = new TripSearchViewModel
            {
                Source = source,
                Destination = destination,
                Mandi = mandi,
                DepartureDate = departureDate,
                VehicleType = vehicleType,
                MinCapacity = minCapacity,
                Trips = trips,
                VehicleTypeList = new()
                {
                    new SelectListItem { Value = "", Text = "All Vehicle Types" },
                    new SelectListItem { Value = "Mini Truck", Text = "Mini Truck" },
                    new SelectListItem { Value = "Pickup Truck", Text = "Pickup Truck" },
                    new SelectListItem { Value = "Tractor Trolley", Text = "Tractor Trolley" },
                    new SelectListItem { Value = "Medium Truck", Text = "Medium Truck" },
                    new SelectListItem { Value = "Heavy Truck", Text = "Heavy Truck" }
                }
            };

            return View(model);
        }

        // GET: /Trips/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var trip = await _tripService.GetTripDetailsAsync(id);
            if (trip == null) return NotFound();

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var confirmedBookings = trip.Bookings
                .Where(b => b.Status == "Accepted")
                .ToList();

            var currentUserBooking = !string.IsNullOrEmpty(currentUserId)
                ? trip.Bookings.FirstOrDefault(b => b.FarmerId == currentUserId && (b.Status == "Pending" || b.Status == "Accepted"))
                : null;

            bool isFarmer = User.IsInRole("Farmer");
            bool isOwner = trip.TransporterId == currentUserId;
            bool canBook = isFarmer && !isOwner && currentUserBooking == null && trip.TripStatus == "Scheduled" && trip.AvailableCapacityKg > 0;

            var model = new TripDetailsViewModel
            {
                Trip = trip,
                ConfirmedBookings = confirmedBookings,
                CanBook = canBook,
                IsOwner = isOwner,
                CurrentUserBooking = currentUserBooking
            };

            return View(model);
        }

        // GET: /Trips/Create
        [HttpGet]
        [Authorize(Roles = "Transporter")]
        public async Task<IActionResult> Create()
        {
            var transporterId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var vehicles = await _vehicleService.GetTransporterVehiclesAsync(transporterId);

            if (!vehicles.Any(v => v.Status == "Active"))
            {
                TempData["ErrorMessage"] = "You must register at least one Active vehicle before publishing a trip.";
                return RedirectToAction("Create", "Vehicles");
            }

            var model = new CreateTripViewModel
            {
                DepartureDate = DateTime.UtcNow.Date.AddDays(1),
                DepartureTime = new TimeSpan(6, 0, 0),
                AvailableVehicles = vehicles
                    .Where(v => v.Status == "Active")
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = $"{v.RegistrationNumber} - {v.Model} ({v.VehicleType}, Capacity: {v.CapacityKg:N0} kg)"
                    })
                    .ToList()
            };

            return View(model);
        }

        // POST: /Trips/Create
        [HttpPost]
        [Authorize(Roles = "Transporter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateTripViewModel model)
        {
            var transporterId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (!ModelState.IsValid)
            {
                var vehicles = await _vehicleService.GetTransporterVehiclesAsync(transporterId);
                model.AvailableVehicles = vehicles
                    .Where(v => v.Status == "Active")
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = $"{v.RegistrationNumber} - {v.Model} ({v.VehicleType}, Capacity: {v.CapacityKg:N0} kg)"
                    })
                    .ToList();
                return View(model);
            }

            var trip = new TransportTrip
            {
                TransporterId = transporterId,
                VehicleId = model.VehicleId,
                SourceLocation = model.SourceLocation.Trim(),
                DestinationLocation = model.DestinationLocation.Trim(),
                MandiName = model.MandiName.Trim(),
                DepartureDate = model.DepartureDate,
                DepartureTime = model.DepartureTime,
                TotalCost = model.TotalCost,
                Description = model.Description?.Trim()
            };

            var (success, message, tripId) = await _tripService.CreateTripAsync(trip);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                var vehicles = await _vehicleService.GetTransporterVehiclesAsync(transporterId);
                model.AvailableVehicles = vehicles
                    .Where(v => v.Status == "Active")
                    .Select(v => new SelectListItem
                    {
                        Value = v.Id.ToString(),
                        Text = $"{v.RegistrationNumber} - {v.Model} ({v.VehicleType}, Capacity: {v.CapacityKg:N0} kg)"
                    })
                    .ToList();
                return View(model);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Details), new { id = tripId });
        }

        // POST: /Trips/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "Transporter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int tripId, string status)
        {
            var transporterId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var (success, message) = await _tripService.UpdateTripStatusAsync(tripId, transporterId, status);

            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Details), new { id = tripId });
        }

        // Demonstration of SQL Views (Unit V)
        [HttpGet]
        public async Task<IActionResult> ActiveTripsView()
        {
            var activeTripsFromSqlView = await _tripService.GetActiveTripsViewAsync();
            ViewBag.IsSqlViewDemo = true;
            return View("ActiveTripsView", activeTripsFromSqlView);
        }

        // Demonstration of SQL Booking Summary View (Unit V)
        [HttpGet]
        public async Task<IActionResult> BookingSummaryView()
        {
            var summaryFromSqlView = await _tripService.GetBookingSummaryViewAsync();
            return View("BookingSummaryView", summaryFromSqlView);
        }
    }
}

