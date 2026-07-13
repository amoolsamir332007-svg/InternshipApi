using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipApi.Models
{
    public class Student
    {
        [Key]
        public int StudentID { get; set; }
        [Required(ErrorMessage = "Student Name is required")]
        [MaxLength(50, ErrorMessage = "Student Name can't be more than 200 characters")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Student number is required")]
        [MaxLength(50, ErrorMessage = "Student number can't be more than 50 characters")]
        public string? Level { get; set; }
        public string? PhoneNumber { get; set; }

        public decimal? GPA { get; set; }
        [MaxLength(1000, ErrorMessage = "Bio can't be more than 1000 characters")]
        public string? Bio { get; set; }
        [MaxLength(500, ErrorMessage = "CV path can't be more than 500 characters")]
        public string? CVPath { get; set; }
        public string? ProfileImagePath { get; set; }
   
 
        //UserID FK
        [Required(ErrorMessage = "User account is required")]
        public string UserID { get; set; }
        [ForeignKey(nameof(UserID))]
        public AspNetUser User { get; set; }



        public ICollection<Application> Applications { get; set; } = new List<Application>();

       



    }
}
