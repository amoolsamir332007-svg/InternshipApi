using InternshipApi.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace TasksApi.DTOs
{
    public class RegisterDTO
    {

        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Password { get; set; }
        [Required]
        public string? FullName { get; set; }
        public AccountType AccountType { get; set; }
        [MaxLength(50, ErrorMessage = "Level can't be more than 50 characters")]
        public string? Level { get; set; }

        [Range(0, 4.0, ErrorMessage = "GPA must be between 0 and 4.0")]
        public decimal? GPA { get; set; }

        // ===== Institution-only fields (optional here, required only if AccountType == Institution) =====
        [MaxLength(100, ErrorMessage = "Address can't be more than 100 characters")]
        public string? Address { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [MaxLength(50, ErrorMessage = "Phone number can't be more than 50 characters")]
        public string? PhoneNumber { get; set; }
    }
}
