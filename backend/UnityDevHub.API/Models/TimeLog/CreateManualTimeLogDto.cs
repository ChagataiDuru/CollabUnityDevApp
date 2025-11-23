using System.ComponentModel.DataAnnotations;

namespace UnityDevHub.API.Models.TimeLog
{
    public class CreateManualTimeLogDto
    {
        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        public string? Description { get; set; }
    }
}
