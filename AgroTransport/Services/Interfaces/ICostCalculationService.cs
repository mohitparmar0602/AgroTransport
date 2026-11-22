using System;

namespace AgroTransport.Services.Interfaces
{
    /// <summary>
    /// Service responsible for calculating transportation costs and savings in pooling.
    /// Demonstrates Unit II Business Logic & Math operations.
    /// </summary>
    public interface ICostCalculationService
    {
        decimal CalculateCostPerKg(decimal totalCost, decimal totalCapacityKg);
        decimal CalculateFarmerShare(decimal requestedCapacityKg, decimal costPerKg);
        decimal CalculateEstimatedSavings(decimal requestedCapacityKg, decimal totalCapacityKg, decimal totalCost);
    }
}

