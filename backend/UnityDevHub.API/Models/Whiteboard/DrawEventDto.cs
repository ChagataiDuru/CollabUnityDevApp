namespace UnityDevHub.API.Models.Whiteboard;

public class DrawEventDto
{
    public double PrevX { get; set; }
    public double PrevY { get; set; }
    public double CurrX { get; set; }
    public double CurrY { get; set; }
    public string Color { get; set; } = "#000000";
    public double LineWidth { get; set; } = 2;
    public string Type { get; set; } = "draw"; // "draw" or "clear"
}
