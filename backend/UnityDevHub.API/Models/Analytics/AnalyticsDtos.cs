namespace UnityDevHub.API.Models.Analytics
{
    public class CompletionRateDto
    {
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public double RatePercentage { get; set; }
    }

    public class HeatmapPointDto
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
