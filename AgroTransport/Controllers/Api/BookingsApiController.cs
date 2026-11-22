using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AgroTransport.Data;
using AgroTransport.DTOs;
using AgroTransport.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroTransport.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class BookingsApiController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ApplicationDbContext _context;

        public BookingsApiController(IBookingService bookingService, ApplicationDbContext context)
        {
            _bookingService = bookingService;
            _context = context;
        }

        // GET: api/bookingsapi
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingDto>>>> GetAllBookings()
        {
            var bookings = await _bookingService.GetAllBookingsAdminAsync();
            var dtos = bookings.Select(b => new BookingDto
            {
                Id = b.Id,
                BookingReference = b.BookingReference,
                TripId = b.TripId,
                Route = b.Trip != null ? $"{b.Trip.SourceLocation} -> {b.Trip.DestinationLocation} ({b.Trip.MandiName})" : "N/A",
                FarmerId = b.FarmerId,
                FarmerName = b.Farmer?.FullName,
                ProduceType = b.ProduceType,
                RequestedQuantityKg = b.RequestedQuantityKg,
                CostPerKg = b.CostPerKg,
                TotalCost = b.TotalCost,
                PickupVillage = b.PickupVillage,
                DropoffLocation = b.DropoffLocation,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            }).ToList();

            return Ok(ApiResponse<IEnumerable<BookingDto>>.Ok(dtos, "Bookings retrieved successfully."));
        }

        // GET: api/bookingsapi/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<BookingDto>>> GetBookingById(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null)
            {
                return NotFound(ApiResponse<BookingDto>.Fail($"Booking with ID {id} not found."));
            }

            var dto = new BookingDto
            {
                Id = booking.Id,
                BookingReference = booking.BookingReference,
                TripId = booking.TripId,
                Route = booking.Trip != null ? $"{booking.Trip.SourceLocation} -> {booking.Trip.DestinationLocation}" : "N/A",
                FarmerId = booking.FarmerId,
                FarmerName = booking.Farmer?.FullName,
                ProduceType = booking.ProduceType,
                RequestedQuantityKg = booking.RequestedQuantityKg,
                CostPerKg = booking.CostPerKg,
                TotalCost = booking.TotalCost,
                PickupVillage = booking.PickupVillage,
                DropoffLocation = booking.DropoffLocation,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt
            };

            return Ok(ApiResponse<BookingDto>.Ok(dto));
        }

        // POST: api/bookingsapi
        [HttpPost]
        public async Task<ActionResult<ApiResponse<BookingDto>>> CreateBooking([FromBody] CreateBookingDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<BookingDto>.Fail("Model validation failed."));
            }

            // In API context, fetch first farmer if unauthenticated
            string farmerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (string.IsNullOrEmpty(farmerId))
            {
                var defaultFarmer = await _context.Users.FirstOrDefaultAsync(u => u.UserRole == "Farmer");
                farmerId = defaultFarmer?.Id ?? "";
            }

            var (success, message, bookingId) = await _bookingService.CreateBookingRequestAsync(
                dto.TripId,
                farmerId,
                dto.ProduceType,
                dto.RequestedQuantityKg,
                dto.PickupVillage,
                dto.DropoffLocation,
                dto.SpecialInstructions);

            if (!success)
            {
                return BadRequest(ApiResponse<BookingDto>.Fail(message));
            }

            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            var resultDto = new BookingDto
            {
                Id = bookingId,
                BookingReference = booking!.BookingReference,
                TripId = dto.TripId,
                ProduceType = dto.ProduceType,
                RequestedQuantityKg = dto.RequestedQuantityKg,
                CostPerKg = booking.CostPerKg,
                TotalCost = booking.TotalCost,
                Status = "Pending",
                CreatedAt = booking.CreatedAt
            };

            return CreatedAtAction(nameof(GetBookingById), new { id = bookingId }, ApiResponse<BookingDto>.Ok(resultDto, message));
        }

        // PUT: api/bookingsapi/5/accept
        [HttpPut("{id:int}/accept")]
        public async Task<ActionResult<ApiResponse<string>>> AcceptBooking(int id, [FromQuery] string? remarks = null)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound(ApiResponse<string>.Fail("Booking not found."));

            string transporterId = booking.Trip?.TransporterId ?? "";
            var (success, message) = await _bookingService.AcceptBookingAsync(id, transporterId, remarks);

            if (!success)
            {
                return BadRequest(ApiResponse<string>.Fail(message));
            }

            return Ok(ApiResponse<string>.Ok(message));
        }

        // PUT: api/bookingsapi/5/reject
        [HttpPut("{id:int}/reject")]
        public async Task<ActionResult<ApiResponse<string>>> RejectBooking(int id, [FromQuery] string? reason = null)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound(ApiResponse<string>.Fail("Booking not found."));

            string transporterId = booking.Trip?.TransporterId ?? "";
            var (success, message) = await _bookingService.RejectBookingAsync(id, transporterId, reason);

            if (!success)
            {
                return BadRequest(ApiResponse<string>.Fail(message));
            }

            return Ok(ApiResponse<string>.Ok(message));
        }

        // DELETE: api/bookingsapi/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> CancelBooking(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null) return NotFound(ApiResponse<string>.Fail("Booking not found."));

            var (success, message) = await _bookingService.CancelBookingAsync(id, booking.FarmerId, isAdmin: true);
            if (!success)
            {
                return BadRequest(ApiResponse<string>.Fail(message));
            }

            return Ok(ApiResponse<string>.Ok(message));
        }
    }
}

