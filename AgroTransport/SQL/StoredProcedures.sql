-- ====================================================================
-- AgroTransport Stored Procedures (Course: INT402 - Unit V SQL Server)
-- ====================================================================

USE AgroTransportDB;
GO

-- Stored Procedure: usp_GetAvailableTripsByRoute
-- Filters available trips matching source, destination, departure date, and minimum capacity
IF OBJECT_ID('dbo.usp_GetAvailableTripsByRoute', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GetAvailableTripsByRoute;
GO

CREATE PROCEDURE dbo.usp_GetAvailableTripsByRoute
    @Source NVARCHAR(100) = NULL,
    @Destination NVARCHAR(100) = NULL,
    @MinCapacity DECIMAL(10,2) = 0,
    @DepartureDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        t.Id,
        t.SourceLocation,
        t.DestinationLocation,
        t.MandiName,
        t.DepartureDate,
        t.DepartureTime,
        t.TotalCapacityKg,
        t.AvailableCapacityKg,
        t.TotalCost,
        t.CostPerKg,
        t.Description,
        t.TripStatus,
        v.RegistrationNumber AS VehicleRegistration,
        v.VehicleType,
        v.Model AS VehicleModel,
        u.FullName AS TransporterName,
        u.PhoneNumber AS TransporterPhone
    FROM dbo.TransportTrips t
    INNER JOIN dbo.Vehicles v ON t.VehicleId = v.Id
    INNER JOIN dbo.AspNetUsers u ON t.TransporterId = u.Id
    WHERE t.TripStatus = 'Scheduled'
      AND t.AvailableCapacityKg >= @MinCapacity
      AND (@Source IS NULL OR t.SourceLocation LIKE '%' + @Source + '%')
      AND (@Destination IS NULL OR t.DestinationLocation LIKE '%' + @Destination + '%' OR t.MandiName LIKE '%' + @Destination + '%')
      AND (@DepartureDate IS NULL OR CAST(t.DepartureDate AS DATE) = @DepartureDate)
    ORDER BY t.DepartureDate ASC, t.CostPerKg ASC;
END;
GO

