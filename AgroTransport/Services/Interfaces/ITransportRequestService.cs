using System.Collections.Generic;
using System.Threading.Tasks;
using AgroTransport.Models;

namespace AgroTransport.Services.Interfaces
{
    public interface ITransportRequestService
    {
        Task<List<TransportRequest>> GetOpenRequestsAsync(string? produceType = null, string? destination = null);
        Task<List<TransportRequest>> GetFarmerRequestsAsync(string farmerId);
        Task<TransportRequest?> GetRequestByIdAsync(int id);
        Task<(bool Success, string Message, int RequestId)> CreateRequestAsync(TransportRequest request);
        Task<(bool Success, string Message)> UpdateStatusAsync(int id, string farmerId, string newStatus);
        Task<(bool Success, string Message)> DeleteRequestAsync(int id, string farmerId, bool isAdmin = false);
    }
}

