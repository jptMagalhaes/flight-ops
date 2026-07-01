namespace FlightOps.Models.Components.Kpi;

public sealed class KpiCardModel
{
    public required string Label { get; init; }
    public required string Value { get; init; }
    public string? Unit { get; init; }
    public string? Subtitle { get; init; }
    public string? IconClass { get; init; }
    public string Variant { get; init; } = "simple";
}
