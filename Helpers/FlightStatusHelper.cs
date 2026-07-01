using FlightOps.Enums;
using FlightOps.Resources;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;

namespace FlightOps.Helpers;

public static class FlightStatusHelper
{
    public static string GetDisplayName(FlightStatus status, IStringLocalizer<SharedResources> localizer) =>
        localizer[$"FlightStatus.{status}"];

    public static SelectList ToSelectList(
        IEnumerable<FlightStatus> statuses,
        FlightStatus selected,
        IStringLocalizer<SharedResources> localizer) =>
        new(
            statuses.Select(s => new { Value = (int)s, Text = GetDisplayName(s, localizer) }),
            "Value",
            "Text",
            (int)selected);
}
