-- ====================================================================
-- AgroTransport Database Views (Course: INT402 - Unit V SQL Server)
-- ====================================================================

USE AgroTransportDB;
GO

-- 1. View: vw_ActiveTransportationTrips
-- Joins TransportTrips with Vehicles and AspNetUsers to present a unified active trip view
IF OBJECT_ID('dbo.vw_ActiveTransportationTrips', 'V') IS NOT NULL
    DROP VIEW dbo.vw_ActiveTransportationTrips;
GO

CREATE VIEW dbo.vw_ActiveTransportationTrips
AS
SELECT 
    t.Id AS TripId,
    u.FullName AS TransporterName,
    u.Email AS TransporterEmail,
    v.RegistrationNumber AS VehicleRegistration,
    v.VehicleType AS VehicleType,
    t.SourceLocation,
    t.DestinationLocation,
    t.MandiName,
    t.DepartureDate,
    t.DepartureTime,
    t.TotalCapacityKg,
    t.AvailableCapacityKg,
    t.TotalCost,
    t.CostPerKg,
    t.TripStatus,
    COUNT(CASE WHEN b.Status = 'Accepted' THEN b.Id END) AS ActiveBookingsCount
FROM dbo.TransportTrips t
INNER JOIN dbo.AspNetUsers u ON t.TransporterId = u.Id
INNER JOIN dbo.Vehicles v ON t.VehicleId = v.Id
LEFT JOIN dbo.Bookings b ON t.Id = b.TripId
WHERE t.TripStatus = 'Scheduled'
GROUP BY 
    t.Id, u.FullName, u.Email, v.RegistrationNumber, v.VehicleType,
    t.SourceLocation, t.DestinationLocation, t.MandiName,
    t.DepartureDate, t.DepartureTime, t.TotalCapacityKg,
    t.AvailableCapacityKg, t.TotalCost, t.CostPerKg, t.TripStatus;
GO

-- 2. View: vw_TripBookingSummary
-- Aggregates total booked capacity, accepted counts, and revenue collected per trip
IF OBJECT_ID('dbo.vw_TripBookingSummary', 'V') IS NOT NULL
    DROP VIEW dbo.vw_TripBookingSummary;
GO

CREATE VIEW dbo.vw_TripBookingSummary
AS
SELECT 
    t.Id AS TripId,
    (t.SourceLocation + ' to ' + t.DestinationLocation + ' (' + t.MandiName + ')') AS Route,
    u.FullName AS TransporterName,
    t.TotalCapacityKg,
    ISNULL(SUM(CASE WHEN b.Status = 'Accepted' THEN b.RequestedQuantityKg ELSE 0 END), 0) AS TotalBookedKg,
    t.AvailableCapacityKg,
    COUNT(b.Id) AS TotalBookingsCount,
    COUNT(CASE WHEN b.Status = 'Accepted' THEN 1 END) AS AcceptedBookingsCount,
    ISNULL(SUM(CASE WHEN b.Status = 'Accepted' THEN b.TotalCost ELSE 0 END), 0) AS TotalCollectedRevenue,
    t.TotalCost AS TripTotalCost
FROM dbo.TransportTrips t
INNER JOIN dbo.AspNetUsers u ON t.TransporterId = u.Id
LEFT JOIN dbo.Bookings b ON t.Id = b.TripId
GROUP BY 
    t.Id, t.SourceLocation, t.DestinationLocation, t.MandiName,
    u.FullName, t.TotalCapacityKg, t.AvailableCapacityKg, t.TotalCost;
GO

