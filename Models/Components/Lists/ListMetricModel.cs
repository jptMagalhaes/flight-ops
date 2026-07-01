namespace FlightOps.Models.Components.Lists;

public class ListMetricModel
{
    public required string Value { get; init; }
    public string? Unit { get; init; }
    public bool Emphasis { get; init; }
}
