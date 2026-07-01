using FlightOps.Models.Pages.Flights;

namespace FlightOps.Features.Flights.Queries;

public interface IFlightCalculationPreviewQuery
{
    Task<FlightCalculationPreviewModel?> BuildAsync(
        int originId,
        int destinationId,
        int aircraftId,
        DateTime departureTime);
}
