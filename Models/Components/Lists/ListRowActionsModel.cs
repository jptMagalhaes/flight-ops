namespace FlightOps.Models.Components.Lists;

public class ListRowActionsModel
{
    public int Id { get; set; }
    public string DetailsAction { get; set; } = "Details";
    public bool ShowEdit { get; set; } = true;
    public bool ShowDelete { get; set; } = true;
    public string DeleteModalId { get; set; } = "";
}
