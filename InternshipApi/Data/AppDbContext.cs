using InternshipApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InternshipApi.Data
{
    public class AppDbContext : IdentityDbContext<AspNetUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Institution> Institutions { get; set; }
        public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<Application> Applications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // required for Identity tables

            // ============ Student ============
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(s => s.UserID).IsUnique(); // one-to-one enforced

                entity.HasOne(s => s.User)
                    .WithOne(u => u.Student)
                    .HasForeignKey<Student>(s => s.UserID)
                    .OnDelete(DeleteBehavior.Cascade); // delete AspNetUser -> delete Student

                entity.Property(s => s.GPA)
                    .HasColumnType("decimal(3,2)"); // e.g. 3.75
            });

            // ============ Institution ============
            modelBuilder.Entity<Institution>(entity =>
            {
                entity.HasIndex(i => i.UserID).IsUnique(); // one-to-one enforced

                entity.HasOne(i => i.User)
                    .WithOne(u => u.Institution)
                    .HasForeignKey<Institution>(i => i.UserID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ============ Opportunity ============
            modelBuilder.Entity<Opportunity>(entity =>
            {
                entity.HasOne(o => o.Institution)
                    .WithMany(i => i.Opportunities)
                    .HasForeignKey(o => o.InstitutionID)
                    .OnDelete(DeleteBehavior.Cascade); // delete Institution -> delete its Opportunities

                entity.Property(o => o.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);
            });

            // ============ Application ============
            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasOne(a => a.Opportunity)
                    .WithMany(o => o.Applications)
                    .HasForeignKey(a => a.OpportunityID)
                    .OnDelete(DeleteBehavior.Cascade); // delete Opportunity -> delete its Applications

                entity.HasOne(a => a.Student)
                    .WithMany(s => s.Applications)
                    .HasForeignKey(a => a.StudentID)
                    .OnDelete(DeleteBehavior.Restrict); // avoid multiple cascade paths

                entity.Property(a => a.Status)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                // one student can't apply twice to the same opportunity
                entity.HasIndex(a => new { a.StudentID, a.OpportunityID }).IsUnique();
            });
        }   // ← يقفل OnModelCreating
    }       // ← يقفل AppDbContext
}           // ← يقفل namespace (ده اللي كان ناقص)