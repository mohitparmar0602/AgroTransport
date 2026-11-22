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
    public class TransportRequestService : ITransportRequestService
    {
        private readonly ApplicationDbContext _context;

        public TransportRequestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TransportRequest>> GetOpenRequestsAsync(string? produceType = null, string? destination = null)
        {
            var query = _context.TransportRequests
                .Include(r => r.Farmer)
                .Where(r => r.Status == "Open" && r.PreferredDate.Date >= DateTime.UtcNow.Date);

            if (!string.IsNullOrWhiteSpace(produceType))
            {
                var pt = produceType.Trim();
                query = query.Where(r => EF.Functions.Like(r.ProduceType, $"%{pt}%"));
            }

            if (!string.IsNullOrWhiteSpace(destination))
            {
                var d = destination.Trim();
                query = query.Where(r => EF.Functions.Like(r.DestinationLocation, $"%{d}%") || EF.Functions.Like(r.MandiName, $"%{d}%"));
            }

            return await query
                .OrderBy(r => r.PreferredDate)
                .ToListAsync();
        }

        public async Task<List<TransportRequest>> GetFarmerRequestsAsync(string farmerId)
        {
            return await _context.TransportRequests
                .Where(r => r.FarmerId == farmerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<TransportRequest?> GetRequestByIdAsync(int id)
        {
            return await _context.TransportRequests
                .Include(r => r.Farmer)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<(bool Success, string Message, int RequestId)> CreateRequestAsync(TransportRequest request)
        {
            if (request.PreferredDate.Date < DateTime.UtcNow.Date)
            {
                return (false, "Preferred transportation date cannot be in the past.", 0);
            }

            if (request.RequiredCapacityKg <= 0)
            {
                return (false, "Required capacity must be greater than zero kg.", 0);
            }

            request.Status = "Open";
            request.CreatedAt = DateTime.UtcNow;

            _context.TransportRequests.Add(request);
            await _context.SaveChangesAsync();

            return (true, "Your transport request has been posted! Registered transporters in this route can now view it and schedule trips.", request.Id);
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(int id, string farmerId, string newStatus)
        {
            var req = await _context.TransportRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (req == null) return (false, "Request not found.");
            if (req.FarmerId != farmerId) return (false, "Unauthorized.");

            req.Status = newStatus;
            await _context.SaveChangesAsync();
            return (true, $"Request status updated to {newStatus}.");
        }

        public async Task<(bool Success, string Message)> DeleteRequestAsync(int id, string farmerId, bool isAdmin = false)
        {
            var req = await _context.TransportRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (req == null) return (false, "Request not found.");
            if (!isAdmin && req.FarmerId != farmerId) return (false, "Unauthorized.");

            _context.TransportRequests.Remove(req);
            await _context.SaveChangesAsync();
            return (true, "Transport request deleted.");
        }
    }
}

