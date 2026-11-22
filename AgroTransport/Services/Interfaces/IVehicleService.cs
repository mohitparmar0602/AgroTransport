using System.Collections.Generic;
using System.Threading.Tasks;
using AgroTransport.Models;

namespace AgroTransport.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<List<Vehicle>> GetTransporterVehiclesAsync(string transporterId);
        Task<Vehicle?> GetVehicleByIdAsync(int id, string? transporterId = null);
        Task<(bool Success, string Message, int VehicleId)> CreateVehicleAsync(Vehicle vehicle);
        Task<(bool Success, string Message)> UpdateVehicleAsync(Vehicle vehicle, string transporterId);
        Task<(bool Success, string Message)> DeleteVehicleAsync(int vehicleId, string transporterId, bool isAdmin = false);
        Task<List<Vehicle>> GetAllVehiclesAdminAsync();
    }
}

