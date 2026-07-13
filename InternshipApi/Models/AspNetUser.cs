using InternshipApi.Models.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace InternshipApi.Models
{
    public class AspNetUser : IdentityUser
    {
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(200, ErrorMessage = "Full name can't be more than 200 characters")]
        [MinLength(2, ErrorMessage = "Full name can't be less than 2 characters")]
        public string FullName { get; set; } = string.Empty;
        public Student? Student { get; set; }
        public Institution? Institution { get; set; }
        public string? ProfileImagePath { get; set; }
        public AccountType AccountType { get; set; }
    }
}
