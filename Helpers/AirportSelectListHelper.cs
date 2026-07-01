using FlightOps.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FlightOps.Helpers;

public static class AirportSelectListHelper
{
    public static string FormatLabel(Airport airport) =>
        $"{airport.IATA} - {airport.City}";

    public static SelectList ToSelectList(IEnumerable<Airport> airports, int? selectedId = null)
    {
        List<SelectListItem> items = airports
            .Select(airport => new SelectListItem
            {
                Value = airport.Id.ToString(),
                Text = FormatLabel(airport),
                Selected = selectedId.HasValue && airport.Id == selectedId.Value
            })
            .ToList();

        return new SelectList(items, "Value", "Text", selectedId?.ToString());
    }
}
