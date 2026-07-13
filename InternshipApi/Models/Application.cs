using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipApi.Models
{
    public class Application
    {

     
            [Key]
            public int ApplicationID { get; set; }

            [Required(ErrorMessage = "Training opportunity is required")]
            public int OpportunityID { get; set; }
            [ForeignKey(nameof(OpportunityID))]
            public Opportunity Opportunity { get; set; } = null!;

            [Required(ErrorMessage = "Student is required")]
            public int StudentID { get; set; }
            [ForeignKey(nameof(StudentID))]
            public Student Student { get; set; } = null!;

            public DateTime AppliedAt { get; set; } = DateTime.Now;

            [MaxLength(1000, ErrorMessage = "Notes can't be more than 1000 characters")]
            public string? Notes { get; set; }

            public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;
        public enum ApplicationStatus
        {
      
            Submitted,
    
            Approved,
          

            Rejected,
          
            Withdrawn
        }

    }
}
