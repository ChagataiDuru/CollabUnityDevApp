namespace UnityDevHub.API.Models.Sprint
{
    public class BurndownDataDto
    {
        public DateTime Date { get; set; }
        public int RemainingTasks { get; set; }
        public int IdealRemainingTasks { get; set; }
    }
}
