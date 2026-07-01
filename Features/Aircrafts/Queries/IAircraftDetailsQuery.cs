using FlightOps.Models.Pages.Aircraft;

namespace FlightOps.Features.Aircrafts.Queries;

public interface IAircraftDetailsQuery
{
    Task<AircraftDetailModel?> BuildAsync(int aircraftId);
}
