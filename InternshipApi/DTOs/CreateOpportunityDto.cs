using System.ComponentModel.DataAnnotations;

namespace InternshipApi.DTOs
{
    public class CreateOpportunityDto
    {

        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200)]
        [MinLength(2)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        [MaxLength(300)]
        public string? Location { get; set; }
    }
}
