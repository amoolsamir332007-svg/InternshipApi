using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipApi.Models
{
    public class Institution
    {
        [Key]
        public int InstituationID { get; set; }
        [Required(ErrorMessage = "Institution name is required")]
        [MaxLength(200, ErrorMessage = "Institution name can't be more than 200 characters")]
        [MinLength(2, ErrorMessage = "Institution name can't be less than 2 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Type can't be more than 200 characters")]
        [StringLength(100, MinimumLength = 2)]
       public string? Address { get; set; }
        [MaxLength(50, ErrorMessage = "Phone number can't be more than 200 characters")]
        public string? PhoneNumber { get; set; }
        [MaxLength(150, ErrorMessage = "Email can't be more than 200 characters")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
        [MaxLength(200, ErrorMessage = "Content person name can't be more than 200 characters")]
      public ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();

        //UserID FK
        [Required(ErrorMessage = "User account is required")]
        [ForeignKey(nameof(UserID))]
        public string UserID { get; set; } = string.Empty;
        public AspNetUser User { get; set; } = null!;

    }
}
