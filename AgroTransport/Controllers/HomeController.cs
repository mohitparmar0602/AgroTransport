using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgroTransport.Controllers
{
    /// <summary>
    /// Home controller serving the public landing page, informational views, and error handling.
    /// Demonstrates Unit III MVC Controllers & Actions.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ITripService _tripService;

        public HomeController(ITripService tripService)
        {
            _tripService = tripService;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            // Retrieve upcoming trips for the live featured trips section
            try
            {
                var featuredTrips = await _tripService.GetAvailableTripsAsync();
                ViewBag.FeaturedTrips = featuredTrips.Take(4).ToList();
            }
            catch (Exception ex)
            {
                ViewBag.FeaturedTrips = new List<TransportTrip>();
                ViewBag.DatabaseWarning = ex.Message;
            }

            return View();
        }

        // GET: /Home/HowItWorks
        public IActionResult HowItWorks()
        {
            return View();
        }

        // GET: /Home/About
        public IActionResult About()
        {
            return View();
        }

        // GET: /Home/Contact
        public IActionResult Contact()
        {
            return View();
        }

        // GET: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorMessage = exceptionFeature?.Error?.Message,
                StackTrace = exceptionFeature?.Error?.StackTrace
            });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string? ErrorMessage { get; set; }
        public string? StackTrace { get; set; }
    }
}

