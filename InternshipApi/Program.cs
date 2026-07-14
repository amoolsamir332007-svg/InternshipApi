using InternshipApi.Data;
using InternshipApi.Data.Seed;
using InternshipApi.Models;
using InternshipApi.Repositories;
using InternshipApi.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InternshipApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<AspNetUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            // ============ JWT Authentication ============
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["JWT:iss"],
                    ValidAudience = builder.Configuration["JWT:aud"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["JWT:skey"]
                            ?? throw new Exception("JWT:skey is missing")
                        ))
                };
            });

            builder.Services.AddScoped<StudentsRepository>();
            builder.Services.AddScoped<TrainingInstitutionRepository>();
            builder.Services.AddScoped<TrainingOpportunityRepository>();
            builder.Services.AddScoped<ApplicationRepository>();

            // ============ CORS ============
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.SetIsOriginAllowed(origin =>
                            origin.StartsWith("http://localhost:") ||
                            origin == "https://your-frontend.onrender.com" // غيّرها بالدومين الحقيقي بتاعك
                          )
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // ============ Port binding for Render ============
            var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
            app.Urls.Clear();
            app.Urls.Add($"http://0.0.0.0:{port}");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // ============ CORS middleware ============
            // لازم يكون قبل UseAuthentication و UseAuthorization
            app.UseCors("AllowFrontend");

            app.UseAuthentication();   // لازم قبل Authorization
            app.UseAuthorization();

            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                await RoleSeeder.SeedRolesAsync(roleManager);
            }

            await app.RunAsync();
        }
    }
}