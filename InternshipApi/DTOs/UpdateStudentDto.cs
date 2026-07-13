using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace InternshipApi.DTOs
{
   
        public class UpdateStudentDto
        {
            [MaxLength(50, ErrorMessage = "Student Name can't be more than 50 characters")]
            public string? Name { get; set; }

            [MaxLength(50, ErrorMessage = "Level can't be more than 50 characters")]
            public string? Level { get; set; }

            [Phone(ErrorMessage = "Invalid phone number format")]
            public string? PhoneNumber { get; set; }

            [Range(0, 4.0, ErrorMessage = "GPA must be between 0 and 4.0")]
            public decimal? GPA { get; set; }

            [MaxLength(1000, ErrorMessage = "Bio can't be more than 1000 characters")]
            public string? Bio { get; set; }
        }
    
}
