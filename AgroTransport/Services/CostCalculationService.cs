using System;
using AgroTransport.Services.Interfaces;

namespace AgroTransport.Services
{
    /// <summary>
    /// Implementation of transportation cost sharing mathematics.
    /// Demonstrates Unit II Encapsulation and Unit IV Dependency Injection.
    /// </summary>
    public class CostCalculationService : ICostCalculationService
    {
        public decimal CalculateCostPerKg(decimal totalCost, decimal totalCapacityKg)
        {
            if (totalCapacityKg <= 0)
                throw new ArgumentException("Vehicle capacity must be greater than zero.", nameof(totalCapacityKg));

            if (totalCost <= 0)
                throw new ArgumentException("Total cost must be greater than zero.", nameof(totalCost));

            // Proportional cost per kilogram rounded to 2 decimal places
            return Math.Round(totalCost / totalCapacityKg, 2, MidpointRounding.AwayFromZero);
        }

        public decimal CalculateFarmerShare(decimal requestedCapacityKg, decimal costPerKg)
        {
            if (requestedCapacityKg <= 0)
                return 0;

            return Math.Round(requestedCapacityKg * costPerKg, 2, MidpointRounding.AwayFromZero);
        }

        public decimal CalculateEstimatedSavings(decimal requestedCapacityKg, decimal totalCapacityKg, decimal totalCost)
        {
            // If the farmer booked the whole vehicle independently, they would have paid totalCost.
            // Under pooling, they pay only their proportional share.
            decimal costPerKg = CalculateCostPerKg(totalCost, totalCapacityKg);
            decimal pooledCost = CalculateFarmerShare(requestedCapacityKg, costPerKg);
            decimal soloHireCost = totalCost;

            return Math.Max(0, soloHireCost - pooledCost);
        }
    }
}

