using InternshipApi.Data;
using InternshipApi.Data.Seed;

using InternshipApi.Models;
using InternshipApi.Models.Constants;
using InternshipApi.Models.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TasksApi.DTOs;

namespace InternshipApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration config;
        private readonly UserManager<AspNetUser> userManager;
        private readonly SignInManager<AspNetUser> signInManager;
        private readonly AppDbContext _dbContext;

        public AccountController(
            IConfiguration config,
            UserManager<AspNetUser> userManager,
            SignInManager<AspNetUser> signInManager,
            AppDbContext dbContext)
        {
            this.config = config;
            this.userManager = userManager;
            this.signInManager = signInManager;
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
        {
            try
            {
                var newUser = new AspNetUser
                {
                    Email = dto.Email,
                    UserName = dto.Email,
                    FullName = dto.FullName,
                    AccountType = dto.AccountType
                };

                var result = await userManager.CreateAsync(newUser, dto.Password);

                if (!result.Succeeded)
                    return StatusCode(500, result.Errors);

                // نحدد الـ Role بناءً على النوع اللي اختاره
                string role = dto.AccountType == AccountType.Student
                    ? Roles.Student
                    : Roles.Institution;

                await userManager.AddToRoleAsync(newUser, role);
                if (dto.AccountType == AccountType.Student)
                {
                    var student = new Student
                    {
                        Name = dto.FullName,
                        Level = dto.Level,
                        GPA = dto.GPA,
                        UserID = newUser.Id
                    };

                    _dbContext.Students.Add(student);
                }
                else if (dto.AccountType == AccountType.Institution)
                {
                    var institution = new Institution
                    {
                        Name = dto.FullName,
                        Address = dto.Address,
                        PhoneNumber = dto.PhoneNumber,
                        Email = dto.Email,
                        UserID = newUser.Id
                    };

                    _dbContext.Institutions.Add(institution);
                }

                await _dbContext.SaveChangesAsync();

                return Ok($"The user [{dto.Email}] is registered as {role}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var user = await userManager.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginDto.Email.ToLower());

            if (user is null)
                return Unauthorized("Invalid email or password");

            var signInResult = await signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!signInResult.Succeeded)
                return Unauthorized("Invalid email or password");

            var token = await GenerateTokenAsync(user);

            return Ok(new { Email = user.Email, Token = token });
        }

        private async Task<string> GenerateTokenAsync(AspNetUser user)
        {
            var roles = await userManager.GetRolesAsync(user);

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Email, user.Email),
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim("FullName", user.FullName)
    };

            // نضيف كل Role كـ Claim، عشان [Authorize(Roles = "Student")] يشتغل صح
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // نضيف InstitutionID أو StudentID حسب نوع الحساب
            if (roles.Contains("Institution"))
            {
                var institution = await _dbContext.Institutions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.UserID == user.Id);

                if (institution != null)
                {
                    claims.Add(new Claim("InstitutionID", institution.InstituationID.ToString()));
                }
            }
            else if (roles.Contains("Student"))
            {
                var student = await _dbContext.Students
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.UserID == user.Id);

                if (student != null)
                {
                    claims.Add(new Claim("StudentID", student.StudentID.ToString()));
                }
            }

            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(config["JWT:skey"]));

            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = signingCredentials,
                Issuer = config["JWT:iss"],
                Audience = config["JWT:aud"],
                Expires = DateTime.Now.AddDays(1),
                Subject = new ClaimsIdentity(claims)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}