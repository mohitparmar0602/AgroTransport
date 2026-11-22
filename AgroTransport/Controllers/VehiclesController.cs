using System.Security.Claims;
using System.Threading.Tasks;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using AgroTransport.ViewModels.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroTransport.Controllers
{
    [Authorize(Roles = "Transporter,Administrator")]
    public class VehiclesController : Controller
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // GET: /Vehicles
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            bool isAdmin = User.IsInRole("Administrator");

            var vehicles = isAdmin 
                ? await _vehicleService.GetAllVehiclesAdminAsync()
                : await _vehicleService.GetTransporterVehiclesAsync(currentUserId);

            return View(vehicles);
        }

        // GET: /Vehicles/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            bool isAdmin = User.IsInRole("Administrator");

            var vehicle = await _vehicleService.GetVehicleByIdAsync(id, isAdmin ? null : currentUserId);
            if (vehicle == null) return NotFound();

            return View(vehicle);
        }

        // GET: /Vehicles/Create
        [Authorize(Roles = "Transporter")]
        public IActionResult Create()
        {
            return View(new VehicleFormViewModel());
        }

        // POST: /Vehicles/Create
        [HttpPost]
        [Authorize(Roles = "Transporter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VehicleFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var transporterId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var vehicle = new Vehicle
            {
                RegistrationNumber = viewModel.RegistrationNumber,
                VehicleType = viewModel.VehicleType,
                CapacityKg = viewModel.CapacityKg,
                Model = viewModel.Model.Trim(),
                TransporterId = transporterId,
                Status = viewModel.Status
            };

            var (success, message, vehicleId) = await _vehicleService.CreateVehicleAsync(vehicle);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(viewModel);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        // GET: /Vehicles/Edit/5
        [Authorize(Roles = "Transporter")]
        public async Task<IActionResult> Edit(int id)
        {
            var transporterId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id, transporterId);
            if (vehicle == null) return NotFound();

            var viewModel = new VehicleFormViewModel
            {
                Id = vehicle.Id,
                RegistrationNumber = vehicle.RegistrationNumber,
                VehicleType = vehicle.VehicleType,
                CapacityKg = vehicle.CapacityKg,
                Model = vehicle.Model,
                Status = vehicle.Status
            };

            return View(viewModel);
        }

        // POST: /Vehicles/Edit/5
        [HttpPost]
        [Authorize(Roles = "Transporter")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VehicleFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var transporterId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var vehicle = new Vehicle
            {
                Id = viewModel.Id,
                RegistrationNumber = viewModel.RegistrationNumber,
                VehicleType = viewModel.VehicleType,
                CapacityKg = viewModel.CapacityKg,
                Model = viewModel.Model.Trim(),
                TransporterId = transporterId,
                Status = viewModel.Status
            };

            var (success, message) = await _vehicleService.UpdateVehicleAsync(vehicle, transporterId);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(viewModel);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(Index));
        }

        // POST: /Vehicles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            bool isAdmin = User.IsInRole("Administrator");

            var (success, message) = await _vehicleService.DeleteVehicleAsync(id, currentUserId, isAdmin);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
