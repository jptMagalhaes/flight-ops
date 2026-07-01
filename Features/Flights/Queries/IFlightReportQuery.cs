using FlightOps.Models.Pages.Flights;

namespace FlightOps.Features.Flights.Queries;

public interface IFlightReportQuery
{
    Task<FlightReportDetailModel?> BuildAsync(int flightId);
}
