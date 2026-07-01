using FlightOps.Models.Pages.Home;

namespace FlightOps.Features.Home.Queries;

public interface IOperationsDashboardQuery
{
    Task<OperationsDashboardModel> BuildAsync();
}
