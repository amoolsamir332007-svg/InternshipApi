using InternshipApi.DTOs;

using InternshipApi.Models;
using InternshipApi.Repositories;
using InternshipApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static InternshipApi.Models.Application;


namespace InternshipApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentController : ControllerBase
    {
        private readonly StudentsRepository _repo;
        private readonly CloudinaryService _cloudinary;
        private readonly ApplicationRepository _applicationsRepo;
        private readonly TrainingOpportunityRepository _opprepo;

        public StudentController(StudentsRepository repo, CloudinaryService cloudinary, ApplicationRepository applicationsRepo, TrainingOpportunityRepository opprepo)
        {
            _repo = repo;
            _cloudinary = cloudinary;
            _applicationsRepo = applicationsRepo;
            _opprepo = opprepo;
        }

        // Helper: يجيب الـ UserID من التوكن
        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        private int GetStudentId()
        {
            var claim = User.FindFirstValue("StudentID");   // already in the token
            if (!int.TryParse(claim, out int id))
                throw new UnauthorizedAccessException("Student ID not found in token");
            return id;
        }

        // ============ Profile ============

        // GET: api/student/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var student = await _repo.GetStudentByUserId(GetUserId());
            if (student == null) return NotFound("Student profile not found");

            return Ok(student);
        }

        // PUT: api/student/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateStudentDto dto)
        {
            var student = await _repo.GetStudentByUserId(GetUserId());
            if (student == null) return NotFound("Student profile not found");

            student.Name = dto.Name ?? student.Name;
            student.Level = dto.Level ?? student.Level;
            student.PhoneNumber = dto.PhoneNumber ?? student.PhoneNumber;
            student.GPA = dto.GPA ?? student.GPA;
            student.Bio = dto.Bio ?? student.Bio;

            await _repo.UpdateStudent(student);

            return Ok(student);
        }

        // POST: api/student/profile/image
        // POST: api/student/profile/image
        [HttpPost("profile/image")]
        public async Task<IActionResult> UploadProfileImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var student = await _repo.GetStudentByUserId(GetUserId());
            if (student == null) return NotFound("Student profile not found");

            string imageUrl;
            try
            {
                imageUrl = await _cloudinary.UploadImageAsync(file, "profiles");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            student.ProfileImagePath = imageUrl;   // دلوقتي بيتخزن رابط Cloudinary كامل
            await _repo.UpdateStudent(student);

            return Ok(new { student.ProfileImagePath });
        }

        // POST: api/student/profile/cv
        [HttpPost("profile/cv")]
        public async Task<IActionResult> UploadCv(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                return BadRequest("Only PDF files are allowed");

            var student = await _repo.GetStudentByUserId(GetUserId());
            if (student == null) return NotFound("Student profile not found");

            string cvUrl;
            try
            {
                cvUrl = await _cloudinary.UploadRawFileAsync(file, "cvs");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            student.CVPath = cvUrl;
            await _repo.UpdateStudent(student);

            return Ok(new { student.CVPath });
        }

        // ============ Opportunities ============
        [AllowAnonymous]
        // GET: api/student/opportunities
        [HttpGet("opportunities")]
        public async Task<IActionResult> GetAllOpportunities()
        {
            var opportunities = await _opprepo.GetAllQueryable()
        .Select(o => new
        {
            o.OpportunityID,
            o.Title,
            o.Description,
            o.Capacity,
            o.StartDate,
            o.EndDate,
            o.Location,
            o.Status,
            o.CreatedAt,
            Institution = new
            {
                o.Institution.InstituationID,
                o.Institution.Name,
                o.Institution.Address,
                o.Institution.Email,
                o.Institution.PhoneNumber
            }
        })
        .ToListAsync();
            return Ok(opportunities);
        }
        
        // GET: api/student/opportunities/search?title=...
        [HttpGet("opportunities/search")]
        public async Task<IActionResult> SearchOpportunities([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return BadRequest("Search title is required");

            var opportunities = await _repo.GetOpportunityByTitle(title);
            return Ok(opportunities);
        }

        // ============ Applications ============

        // POST: api/student/applications
        [HttpPost("applications")]
        public async Task<IActionResult> Apply([FromBody] CreateApplicationDto dto)
        {
            try
            {
                var student = await _repo.GetStudentByUserId(GetUserId());
                if (student == null) return NotFound("Student profile not found");

                bool alreadyApplied = await _repo.GetAllApplicationByStudentId(student.StudentID)
                    .AnyAsync(a => a.OpportunityID == dto.OpportunityId);

                if (alreadyApplied)
                    return BadRequest("You already applied to this opportunity");

                var application = new Application
                {
                    StudentID = student.StudentID,
                    OpportunityID = dto.OpportunityId,
                    Notes = dto.Notes,
                    Status = ApplicationStatus.Submitted,
                    AppliedAt = DateTime.UtcNow
                };

                await _repo.AddApplication(application);

                return Ok(application);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, inner = ex.InnerException?.Message });
            }
        }

        // GET: api/student/applications
        [HttpGet("applications")]
        public async Task<IActionResult> GetMyApplications()
        {
            var student = await _repo.GetStudentByUserId(GetUserId());
            if (student == null) return NotFound("Student profile not found");

            var applications = await _repo.GetAllApplicationByStudentId(student.StudentID)
                .ToListAsync();

            return Ok(applications);
        }

        // GET: api/student/applications/{id}
        [HttpGet("applications/{id}")]
        public async Task<IActionResult> GetApplicationById(int id)
        {
            var student = await _repo.GetStudentByUserId(GetUserId());
            if (student == null) return NotFound("Student profile not found");

            var application = await _repo.GetApplicationById(id);

            if (application == null) return NotFound("Application not found");

            // تحقق أمان: هل الطلب ده تبع الطالب اللي عامل login؟
            if (application.StudentID != student.StudentID)
                return Forbid();

            return Ok(application);
        }
        [HttpPatch("applications/{id}/withdraw")]
       
        public async Task<IActionResult> WithdrawApplication(int id)
        {
            var application = await _applicationsRepo.WithdrawAsync(id, GetStudentId());

            if (application == null)
                return NotFound("Application not found or doesn't belong to you");

            // رجّع بس البيانات المحتاجها الفرونت (تجنب الـ circular reference)
            return Ok(new
            {
                application.ApplicationID,
                application.Status,
                application.OpportunityID,
                Message = "Application withdrawn"
            });
        }
    }
}