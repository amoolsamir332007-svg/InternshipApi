using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipApi.Models
{
    public class Opportunity
    {
      
            [Key]
            public int OpportunityID { get; set; }

            [Required(ErrorMessage = "Institution is required")]
            public int InstitutionID { get; set; }
            [ForeignKey(nameof(InstitutionID))]
            public Institution Institution { get; set; } = null!;

            [Required(ErrorMessage = "Title is required")]
            [MaxLength(200)]
            [MinLength(2)]
            public string Title { get; set; } = string.Empty;

            [MaxLength(500)]
            public string? Description { get; set; }

            [Required(ErrorMessage = "Capacity is required")]
            public int Capacity { get; set; }

            [Required]
            public DateTime StartDate { get; set; }
            [Required]
            public DateTime EndDate { get; set; }

            [MaxLength(300)]
            public string? Location { get; set; }

            [Column(TypeName = "nvarchar(50)")]
            public OpportunityStatus Status { get; set; } = OpportunityStatus.Open;

            public DateTime CreatedAt { get; set; } = DateTime.Now;

            public ICollection<Application> Applications { get; set; } = new List<Application>();
        public enum OpportunityStatus
        {
            Open = 1,
            Closed = 2,
            Cancelled = 3

        }

    }
   
}
