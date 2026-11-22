using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AgroTransport.DTOs;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgroTransport.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TripsApiController : ControllerBase
    {
        private readonly ITripService _tripService;
        private readonly IVehicleService _vehicleService;

        public TripsApiController(ITripService tripService, IVehicleService vehicleService)
        {
            _tripService = tripService;
            _vehicleService = vehicleService;
        }

        // GET: api/tripsapi
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TripDto>>>> GetTrips(
            [FromQuery] string? source = null,
            [FromQuery] string? destination = null,
            [FromQuery] string? mandi = null,
            [FromQuery] DateTime? departureDate = null,
            [FromQuery] string? vehicleType = null,
            [FromQuery] decimal? minCapacity = null)
        {
            var trips = await _tripService.GetAvailableTripsAsync(
                source, destination, mandi, departureDate, vehicleType, minCapacity);

            var dtos = trips.Select(t => new TripDto
            {
                Id = t.Id,
                TransporterId = t.TransporterId,
                TransporterName = t.Transporter?.FullName,
                VehicleId = t.VehicleId,
                VehicleRegistration = t.Vehicle?.RegistrationNumber,
                SourceLocation = t.SourceLocation,
                DestinationLocation = t.DestinationLocation,
                MandiName = t.MandiName,
                DepartureDate = t.DepartureDate,
                DepartureTime = t.DepartureTime,
                TotalCapacityKg = t.TotalCapacityKg,
                AvailableCapacityKg = t.AvailableCapacityKg,
                TotalCost = t.TotalCost,
                CostPerKg = t.CostPerKg,
                Description = t.Description,
                TripStatus = t.TripStatus
            }).ToList();

            return Ok(ApiResponse<IEnumerable<TripDto>>.Ok(dtos, $"Found {dtos.Count} available trips."));
        }

        // GET: api/tripsapi/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<TripDto>>> GetTripById(int id)
        {
            var trip = await _tripService.GetTripDetailsAsync(id);
            if (trip == null)
            {
                return NotFound(ApiResponse<TripDto>.Fail($"Trip with ID {id} not found."));
            }

            var dto = new TripDto
            {
                Id = trip.Id,
                TransporterId = trip.TransporterId,
                TransporterName = trip.Transporter?.FullName,
                VehicleId = trip.VehicleId,
                VehicleRegistration = trip.Vehicle?.RegistrationNumber,
                SourceLocation = trip.SourceLocation,
                DestinationLocation = trip.DestinationLocation,
                MandiName = trip.MandiName,
                DepartureDate = trip.DepartureDate,
                DepartureTime = trip.DepartureTime,
                TotalCapacityKg = trip.TotalCapacityKg,
                AvailableCapacityKg = trip.AvailableCapacityKg,
                TotalCost = trip.TotalCost,
                CostPerKg = trip.CostPerKg,
                Description = trip.Description,
                TripStatus = trip.TripStatus
            };

            return Ok(ApiResponse<TripDto>.Ok(dto));
        }

        // POST: api/tripsapi
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TripDto>>> CreateTrip([FromBody] CreateTripDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<TripDto>.Fail("Model validation failed."));
            }

            var vehicle = await _vehicleService.GetVehicleByIdAsync(dto.VehicleId);
            if (vehicle == null)
            {
                return BadRequest(ApiResponse<TripDto>.Fail("Specified vehicle does not exist."));
            }

            var trip = new TransportTrip
            {
                TransporterId = vehicle.TransporterId,
                VehicleId = dto.VehicleId,
                SourceLocation = dto.SourceLocation,
                DestinationLocation = dto.DestinationLocation,
                MandiName = dto.MandiName,
                DepartureDate = dto.DepartureDate,
                DepartureTime = dto.DepartureTime,
                TotalCost = dto.TotalCost,
                Description = dto.Description
            };

            var (success, message, tripId) = await _tripService.CreateTripAsync(trip);
            if (!success)
            {
                return BadRequest(ApiResponse<TripDto>.Fail(message));
            }

            var created = await _tripService.GetTripDetailsAsync(tripId);
            var resultDto = new TripDto
            {
                Id = tripId,
                SourceLocation = trip.SourceLocation,
                DestinationLocation = trip.DestinationLocation,
                MandiName = trip.MandiName,
                TotalCost = trip.TotalCost,
                CostPerKg = trip.CostPerKg,
                TotalCapacityKg = trip.TotalCapacityKg,
                AvailableCapacityKg = trip.AvailableCapacityKg,
                TripStatus = trip.TripStatus
            };

            return CreatedAtAction(nameof(GetTripById), new { id = tripId }, ApiResponse<TripDto>.Ok(resultDto, message));
        }

        // PUT: api/tripsapi/5/status
        [HttpPut("{id:int}/status")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateStatus(int id, [FromQuery] string status)
        {
            var trip = await _tripService.GetTripDetailsAsync(id);
            if (trip == null)
            {
                return NotFound(ApiResponse<string>.Fail($"Trip with ID {id} not found."));
            }

            var (success, message) = await _tripService.UpdateTripStatusAsync(id, trip.TransporterId, status);
            if (!success)
            {
                return BadRequest(ApiResponse<string>.Fail(message));
            }

            return Ok(ApiResponse<string>.Ok(message));
        }

        // DELETE: api/tripsapi/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteTrip(int id)
        {
            var (success, message) = await _tripService.DeleteTripAsync(id, isAdmin: true);
            if (!success)
            {
                return Conflict(ApiResponse<string>.Fail(message));
            }

            return NoContent();
        }
    }
}

