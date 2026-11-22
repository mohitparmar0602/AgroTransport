using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgroTransport.Data;
using AgroTransport.Models;
using AgroTransport.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgroTransport.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ApplicationDbContext _context;

        public VehicleService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vehicle>> GetTransporterVehiclesAsync(string transporterId)
        {
            return await _context.Vehicles
                .Include(v => v.Trips)
                .Where(v => v.TransporterId == transporterId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int id, string? transporterId = null)
        {
            var query = _context.Vehicles
                .Include(v => v.Transporter)
                .Include(v => v.Trips)
                .AsQueryable();

            if (!string.IsNullOrEmpty(transporterId))
            {
                query = query.Where(v => v.TransporterId == transporterId);
            }

            return await query.FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<(bool Success, string Message, int VehicleId)> CreateVehicleAsync(Vehicle vehicle)
        {
            // Normalize registration number uppercase
            vehicle.RegistrationNumber = vehicle.RegistrationNumber.Trim().ToUpperInvariant();

            // Check unique registration number
            bool exists = await _context.Vehicles.AnyAsync(v => v.RegistrationNumber == vehicle.RegistrationNumber);
            if (exists)
            {
                return (false, "A vehicle with this registration number is already registered in the system.", 0);
            }

            vehicle.CreatedAt = DateTime.UtcNow;
            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            return (true, "Vehicle registered successfully!", vehicle.Id);
        }

        public async Task<(bool Success, string Message)> UpdateVehicleAsync(Vehicle vehicle, string transporterId)
        {
            var existing = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicle.Id);
            if (existing == null)
            {
                return (false, "Vehicle not found.");
            }

            if (existing.TransporterId != transporterId)
            {
                return (false, "Unauthorized: You do not own this vehicle.");
            }

            string reg = vehicle.RegistrationNumber.Trim().ToUpperInvariant();
            bool regConflict = await _context.Vehicles.AnyAsync(v => v.RegistrationNumber == reg && v.Id != vehicle.Id);
            if (regConflict)
            {
                return (false, "Registration number is already in use by another vehicle.");
            }

            existing.RegistrationNumber = reg;
            existing.VehicleType = vehicle.VehicleType;
            existing.CapacityKg = vehicle.CapacityKg;
            existing.Model = vehicle.Model;
            existing.Status = vehicle.Status;

            await _context.SaveChangesAsync();
            return (true, "Vehicle updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteVehicleAsync(int vehicleId, string transporterId, bool isAdmin = false)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Trips)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle == null)
            {
                return (false, "Vehicle not found.");
            }

            if (!isAdmin && vehicle.TransporterId != transporterId)
            {
                return (false, "Unauthorized: You do not own this vehicle.");
            }

            // Rule 3: Delete vehicles when they are NOT assigned to active trips
            bool hasActiveTrips = vehicle.Trips.Any(t => t.TripStatus == "Scheduled" || t.TripStatus == "InTransit");
            if (hasActiveTrips)
            {
                return (false, "Cannot delete vehicle because it is currently assigned to active or scheduled trips. Complete or cancel those trips first.");
            }

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return (true, "Vehicle removed successfully.");
        }

        public async Task<List<Vehicle>> GetAllVehiclesAdminAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Transporter)
                .Include(v => v.Trips)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }
    }
}

