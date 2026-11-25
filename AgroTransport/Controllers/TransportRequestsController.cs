using System;
using System.Security.Claims;
using System.Threading.Tasks;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroTransport.Controllers
{
    [Authorize]
    public class TransportRequestsController : Controller
    {
        private readonly ITransportRequestService _requestService;

        public TransportRequestsController(ITransportRequestService requestService)
        {
            _requestService = requestService;
        }

        // GET: /TransportRequests
        [AllowAnonymous]
        public async Task<IActionResult> Index(string? produceType = null, string? destination = null)
        {
            var requests = await _requestService.GetOpenRequestsAsync(produceType, destination);
            ViewBag.FilterProduce = produceType;
            ViewBag.FilterDestination = destination;
            return View(requests);
        }

        // GET: /TransportRequests/Create
        [Authorize(Roles = "Farmer")]
        public IActionResult Create()
        {
            return View(new TransportRequest
            {
                PreferredDate = DateTime.UtcNow.Date.AddDays(2)
            });
        }

        // POST: /TransportRequests/Create
        [HttpPost]
        [Authorize(Roles = "Farmer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransportRequest model)
        {
            var farmerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(farmerId))
            {
                return Challenge();
            }

            model.FarmerId = farmerId;
            model.Status = "Open";
            ModelState.Remove(nameof(TransportRequest.FarmerId));
            ModelState.Remove(nameof(TransportRequest.Farmer));
            ModelState.Remove(nameof(TransportRequest.Status));
            ModelState.Remove("FarmerId");
            ModelState.Remove("Farmer");
            ModelState.Remove("Status");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, message, requestId) = await _requestService.CreateRequestAsync(model);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["SuccessMessage"] = message;
            return RedirectToAction(nameof(MyRequests));
        }

        // GET: /TransportRequests/MyRequests
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> MyRequests()
        {
            var farmerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var requests = await _requestService.GetFarmerRequestsAsync(farmerId);
            return View(requests);
        }

        // POST: /TransportRequests/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            bool isAdmin = User.IsInRole("Administrator");

            var (success, message) = await _requestService.DeleteRequestAsync(id, userId, isAdmin);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(MyRequests));
        }
    }
}

