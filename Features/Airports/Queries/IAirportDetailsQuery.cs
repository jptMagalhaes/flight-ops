using FlightOps.Models.Pages.Airports;

namespace FlightOps.Features.Airports.Queries;

public interface IAirportDetailsQuery
{
    Task<AirportDetailModel?> BuildAsync(int airportId);
}
