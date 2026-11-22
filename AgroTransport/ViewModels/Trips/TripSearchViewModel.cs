using System;
using System.Collections.Generic;
using AgroTransport.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgroTransport.ViewModels.Trips
{
    public class TripSearchViewModel
    {
        public string? Source { get; set; }
        public string? Destination { get; set; }
        public string? Mandi { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string? VehicleType { get; set; }
        public decimal? MinCapacity { get; set; }

        public List<TransportTrip> Trips { get; set; } = new List<TransportTrip>();
        public List<SelectListItem> VehicleTypeList { get; set; } = new List<SelectListItem>();
    }
}

