using System.ComponentModel.DataAnnotations;
using UnityDevHub.API.Data.Entities;

namespace UnityDevHub.API.Models.Sprint
{
    public class UpdateSprintDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Goal { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        public SprintStatus Status { get; set; }
    }
}
