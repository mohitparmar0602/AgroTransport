using System.Collections.Generic;
using AgroTransport.Models;

namespace AgroTransport.ViewModels.Trips
{
    public class TripDetailsViewModel
    {
        public TransportTrip Trip { get; set; } = null!;
        public List<Booking> ConfirmedBookings { get; set; } = new List<Booking>();
        public bool CanBook { get; set; }
        public bool IsOwner { get; set; }
        public Booking? CurrentUserBooking { get; set; }
        public decimal OccupiedCapacityKg => Trip.TotalCapacityKg - Trip.AvailableCapacityKg;
        public decimal OccupiedPercentage => Trip.TotalCapacityKg > 0 ? (OccupiedCapacityKg / Trip.TotalCapacityKg) * 100 : 0;
    }
}

