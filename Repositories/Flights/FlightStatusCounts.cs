namespace FlightOps.Repositories.Flights;

public sealed record FlightStatusCounts(
    int ActiveFlights,
    int UpcomingDeparturesCount,
    int CancelledTodayCount);
