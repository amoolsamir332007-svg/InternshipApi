using System.ComponentModel.DataAnnotations;

namespace InternshipApi.DTOs
{
    public class CreateApplicationDto
    {
        public int OpportunityId { get; set; }

        [MaxLength(1000, ErrorMessage = "Notes can't be more than 1000 characters")]
        public string? Notes { get; set; }
    }
}
