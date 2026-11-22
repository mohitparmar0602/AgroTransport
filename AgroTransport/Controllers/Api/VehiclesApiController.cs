using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AgroTransport.Data;
using AgroTransport.DTOs;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AgroTransport.Controllers.Api
{
    /// <summary>
    /// RESTful Web API Controller for Vehicle Management (Course: INT402 - Unit VI Web API).
    /// Demonstrates Attribute Routing, DTOs, HTTP Verbs, and Standard Status Codes (200, 201, 204, 400, 404, 409).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VehiclesApiController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly ApplicationDbContext _context;

        public VehiclesApiController(IVehicleService vehicleService, ApplicationDbContext context)
        {
            _vehicleService = vehicleService;
            _context = context;
        }

        // GET: api/vehiclesapi
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<VehicleDto>>>> GetAllVehicles()
        {
            var vehicles = await _vehicleService.GetAllVehiclesAdminAsync();
            var dtos = vehicles.Select(v => new VehicleDto
            {
                Id = v.Id,
                RegistrationNumber = v.RegistrationNumber,
                VehicleType = v.VehicleType,
                CapacityKg = v.CapacityKg,
                Model = v.Model,
                Status = v.Status,
                TransporterId = v.TransporterId,
                TransporterName = v.Transporter?.FullName
            }).ToList();

            return Ok(ApiResponse<IEnumerable<VehicleDto>>.Ok(dtos, "Vehicles retrieved successfully."));
        }

        // GET: api/vehiclesapi/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<VehicleDto>>> GetVehicleById(int id)
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null)
            {
                return NotFound(ApiResponse<VehicleDto>.Fail($"Vehicle with ID {id} was not found."));
            }

            var dto = new VehicleDto
            {
                Id = vehicle.Id,
                RegistrationNumber = vehicle.RegistrationNumber,
                VehicleType = vehicle.VehicleType,
                CapacityKg = vehicle.CapacityKg,
                Model = vehicle.Model,
                Status = vehicle.Status,
                TransporterId = vehicle.TransporterId,
                TransporterName = vehicle.Transporter?.FullName
            };

            return Ok(ApiResponse<VehicleDto>.Ok(dto));
        }

        // POST: api/vehiclesapi
        [HttpPost]
        public async Task<ActionResult<ApiResponse<VehicleDto>>> CreateVehicle([FromBody] VehicleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<VehicleDto>.Fail("Invalid model validation failed."));
            }

            // In API testing / postman, assign transporter or fallback to first transporter
            string transporterId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            if (string.IsNullOrEmpty(transporterId))
            {
                if (!string.IsNullOrEmpty(dto.TransporterId))
                {
                    transporterId = dto.TransporterId;
                }
                else
                {
                    var defaultTransporter = await _context.Users.FirstOrDefaultAsync(u => u.UserRole == "Transporter");
                    transporterId = defaultTransporter?.Id ?? "";
                }
            }

            var vehicle = new Vehicle
            {
                RegistrationNumber = dto.RegistrationNumber,
                VehicleType = dto.VehicleType,
                CapacityKg = dto.CapacityKg,
                Model = dto.Model,
                Status = dto.Status ?? "Active",
                TransporterId = transporterId
            };

            var (success, message, vehicleId) = await _vehicleService.CreateVehicleAsync(vehicle);
            if (!success)
            {
                return Conflict(ApiResponse<VehicleDto>.Fail(message)); // 409 Conflict if duplicate registration
            }

            dto.Id = vehicleId;
            return CreatedAtAction(nameof(GetVehicleById), new { id = vehicleId }, ApiResponse<VehicleDto>.Ok(dto, message));
        }

        // PUT: api/vehiclesapi/5
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> UpdateVehicle(int id, [FromBody] VehicleDto dto)
        {
            if (id != dto.Id && dto.Id != 0)
            {
                return BadRequest(ApiResponse<string>.Fail("Route ID and payload ID do not match."));
            }

            var existing = await _vehicleService.GetVehicleByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<string>.Fail($"Vehicle with ID {id} not found."));
            }

            var vehicle = new Vehicle
            {
                Id = id,
                RegistrationNumber = dto.RegistrationNumber,
                VehicleType = dto.VehicleType,
                CapacityKg = dto.CapacityKg,
                Model = dto.Model,
                Status = dto.Status ?? "Active"
            };

            var (success, message) = await _vehicleService.UpdateVehicleAsync(vehicle, existing.TransporterId);
            if (!success)
            {
                return BadRequest(ApiResponse<string>.Fail(message));
            }

            return Ok(ApiResponse<string>.Ok("Vehicle updated successfully."));
        }

        // DELETE: api/vehiclesapi/5
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse<string>>> DeleteVehicle(int id)
        {
            var existing = await _vehicleService.GetVehicleByIdAsync(id);
            if (existing == null)
            {
                return NotFound(ApiResponse<string>.Fail($"Vehicle with ID {id} not found."));
            }

            var (success, message) = await _vehicleService.DeleteVehicleAsync(id, existing.TransporterId, isAdmin: true);
            if (!success)
            {
                return Conflict(ApiResponse<string>.Fail(message));
            }

            return NoContent(); // 204 No Content
        }
    }
}
