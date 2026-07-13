

using InternshipApi.Data;
using InternshipApi.Models;
using Microsoft.EntityFrameworkCore;


namespace InternshipApi.Repositories
{
    public class StudentsRepository
    {
        // Field
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        // create constructor  
        public StudentsRepository(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private IQueryable<Student> StudentDetails()
        {
            return _context.Students.AsQueryable();
               
        }
        // Get all student With Details
        public async Task<List<Student>> GetAllStudents()
        {
            return await StudentDetails().AsNoTracking().ToListAsync();

        }
        public async Task<Student?> GetStudentByUserId(string userId)
        {

            return await StudentDetails().FirstOrDefaultAsync(s => s.UserID == userId);
        }
        // get student By Id 
        public async Task<Student?> GetStudentById(int studentId)
        {

            return await StudentDetails().FirstOrDefaultAsync(s => s.StudentID == studentId);
        }
        // Search student
        public async Task<List<Student>> SearchStudent
            (string? name = null, string? department = null, string? skill = null, string? specialty = null)
        {
            var query = StudentDetails().AsNoTracking();

            // Search by Name
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(s =>
                    EF.Functions.Like(s.Name, $"%{name}%"));
            }

           

            return await query.ToListAsync();
        }

        //add student 
        public async Task AddStudent(Student newStudent)
        {
            _context.Students.Add(newStudent);
            await _context.SaveChangesAsync();
        }

        // update student 
        public async Task UpdateStudent(Student Student)
        {
            _context.Students.Update(Student);
            await _context.SaveChangesAsync();

        }

        //delete student BY id 
        public async Task<bool> DeleteStudentById(int id)
        {
            var Student = await StudentDetails().FirstOrDefaultAsync(s => s.StudentID == id);
            if (Student == null) return false;
  
            _context.Students.Remove(Student);
            await _context.SaveChangesAsync();
            DeleteImage(Student.ProfileImagePath);
            DeleteCvFile(Student.CVPath);
            return true;
        }


   
        
       
        public async Task AddAsync(Student student)
        {

            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
        }

        // delete  all Student Skills By student id 
      
        public async Task AddApplication(Application newTrainingApplication)
        {
            _context.Applications.Add(newTrainingApplication);
            await _context.SaveChangesAsync();
        }

        //Tracking the status of specific Training Application using Application id
        public async Task<Application?> GetApplicationById(int id)
        {
            return await _context.Applications.AsNoTracking().
              Include(tp => tp.Opportunity).
              FirstOrDefaultAsync(tp => tp.ApplicationID == id);
        }

        // get all Training Application for student by student Id
        public IQueryable<Application> GetAllApplicationByStudentId(int studentId)
        {
            return _context.Applications.AsNoTracking().
                Include(tp => tp.Opportunity).ThenInclude(to => to.Institution).
                                                      Where(tp => tp.StudentID == studentId);
        }
        // get IQueryable all Training Application for student by student Id use in Paginated List
        public IQueryable<Application> GetTrainingApplications(int studentId)
        {
            return _context.Applications.
                Include(tp => tp.Opportunity).ThenInclude(t => t.Institution).
                Where(tp => tp.StudentID == studentId);
        }

        // Get all  Training Terms for all Opportunities
       
        public async Task<List<Opportunity>> GetOpportunityByTitle(string strSearch)
        {
            return await _context.Opportunities.AsNoTracking().Where(to => EF.Functions.Like(to.Title, $"%{strSearch}%")).ToListAsync();

        }
      
        //Get All Opportunity
        public async Task<List<Opportunity>> GetAllOpportunity()
        {
            return await _context.Opportunities.ToListAsync();
        }

      
        private void DeleteImage(string? imgPath)
        {
            if (string.IsNullOrEmpty(imgPath)) return;
            var filePath = Path.Combine(_environment.WebRootPath, imgPath.TrimStart('/'));
            if (File.Exists(filePath)) { File.Delete(filePath); }
        }
        // Delete Cv File
        private void DeleteCvFile(string? CvPath)
        {
            if (string.IsNullOrEmpty(CvPath)) return;
            var filePath = Path.Combine(_environment.WebRootPath, CvPath.TrimStart('/'));
            if (File.Exists(filePath)) { File.Delete(filePath); }
        }

        // View the evaluation for a specific opportunity based on the opportunity name and student ID
       
      
        // Get students By Status 
     
    
      
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}