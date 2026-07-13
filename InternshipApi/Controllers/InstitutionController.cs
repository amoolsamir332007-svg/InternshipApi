

using InternshipApi.DTOs;
using InternshipApi.Models;
using InternshipApi.Repositories;
using InternshipApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using System.Reflection.PortableExecutable;
using System.Security.Claims;
using static InternshipApi.Models.Application;
using static InternshipApi.Models.Opportunity;

namespace InternshipApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Institution")]
    public class InstitutionController : ControllerBase
    {
        private readonly TrainingInstitutionRepository _institutionRepo;
        private readonly TrainingOpportunityRepository _opportunityRepo;
        private readonly ApplicationRepository _applicationsRepo;

        public InstitutionController(
            TrainingInstitutionRepository institutionRepo,
            TrainingOpportunityRepository opportunityRepo,
            ApplicationRepository applicationsRepo)
        {
            _institutionRepo = institutionRepo;
            _opportunityRepo = opportunityRepo;
            _applicationsRepo = applicationsRepo;
        }

        private int GetInstitutionId()
        {
            var claim = User.FindFirstValue("InstitutionID");
            if (!int.TryParse(claim, out int id))
                throw new UnauthorizedAccessException("Institution ID not found in token");

            return id;
        }
        [HttpPost("opportunities")]
        public async Task<IActionResult> CreateOpportunity([FromBody] CreateOpportunityDto dto)
        {
            if (dto.EndDate <= dto.StartDate)
                return BadRequest("End date must be after start date");

            var opportunity = new Opportunity
            {
                Title = dto.Title,
                Description = dto.Description,
                Capacity = dto.Capacity,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Location = dto.Location,
                InstitutionID = GetInstitutionId(),   // من التوكن، مش من الفرونت
                Status = OpportunityStatus.Open,
                CreatedAt = DateTime.Now
            };

            await _opportunityRepo.AddAsync(opportunity);

            return Ok(opportunity);
        }
       

        // ============ Profile ============
        // (زي ما هي - GetMyProfile, UpdateProfile)
        // ...

        // ============ Opportunities ============
        // (زي ما هي - CreateOpportunity, GetMyOpportunities, GetOpportunityById,
        //  UpdateOpportunity, ToggleStatus, DeleteOpportunity)
        // ...

        // ============ Applications ============

        // GET: api/institution/applications
        [HttpGet("applications")]
        public async Task<IActionResult> GetAllApplications()
        {
            var applications = await _applicationsRepo
                .GetByInstitutionIdQueryable(GetInstitutionId())
                .ToListAsync();

            return Ok(applications);
        }

        // GET: api/institution/opportunities/{opportunityId}/applications
        [HttpGet("opportunities/{opportunityId}/applications")]
        public async Task<IActionResult> GetApplicationsForOpportunity(int opportunityId)
        {
            var opportunity = await _opportunityRepo.GetByIdModifyAsync(opportunityId);
            if (opportunity == null) return NotFound("Opportunity not found");

            if (opportunity.InstitutionID != GetInstitutionId())
                return Forbid();

            var applications = await _applicationsRepo
                .GetByOpportunityIdQueryable(opportunityId)
                .ToListAsync();

            return Ok(applications);
        }

        // PATCH: api/institution/applications/{id}/decision
        [HttpPatch("applications/{id}/decision")]
        public async Task<IActionResult> DecideApplication(int id, [FromBody] DecisionDto dto)
        {
            var application = await _applicationsRepo.GetByIdModifyAsync(id);
            if (application == null) return NotFound("Application not found");

            // تحقق أمان: هل الطلب ده تبع فرصة تبع نفس المؤسسة؟
            if (application.Opportunity.InstitutionID != GetInstitutionId())
                return Forbid();

            if (application.Status != ApplicationStatus.Submitted)
                return BadRequest("This application has already been decided");

            if (dto.Decision == ApplicationStatus.Approved)
            {
                int acceptedCount = await _applicationsRepo
                    .CountApprovedByOpportunityIdAsync(application.OpportunityID);

                if (acceptedCount >= application.Opportunity.Capacity)
                    return BadRequest("Opportunity is already full");

                await _applicationsRepo.UpdateStatusAsync(id, ApplicationStatus.Approved);
            }
            else if (dto.Decision == ApplicationStatus.Rejected)
            {
                await _applicationsRepo.UpdateStatusAsync(id, ApplicationStatus.Rejected);
            }
            else
            {
                return BadRequest("Invalid decision value");
            }

            return Ok(new { Message = "Decision saved" });
        }

        // ============ Profile ============

        // GET: api/institution/profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            var institution = await _institutionRepo.GetByIdAsync(GetInstitutionId());
            if (institution == null) return NotFound("Institution profile not found");

            return Ok(institution);
        }

        // PUT: api/institution/profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateInstitutionDto dto)
        {
            var institution = await _institutionRepo.GetByIdModifyAsync(GetInstitutionId());
            if (institution == null) return NotFound("Institution profile not found");

            institution.Name = dto.Name ?? institution.Name;
            institution.Address = dto.Address ?? institution.Address;
            institution.PhoneNumber = dto.PhoneNumber ?? institution.PhoneNumber;
            institution.Email = dto.Email ?? institution.Email;

            await _institutionRepo.UpdateAsync(institution);

            return Ok(institution);
        }
    }
}