using InternshipApi.Data;
using InternshipApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using static InternshipApi.Models.Application;

namespace InternshipApi.Repositories
{
    public class ApplicationRepository
    {
        private readonly AppDbContext _dbContext;

        public ApplicationRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Queryable

        public IQueryable<Application> GetAllQueryable()
        {
            return _dbContext.Applications
                .Include(a => a.Student)
                .Include(a => a.Opportunity)
                    .ThenInclude(o => o.Institution)
                .AsNoTracking();
        }

        public IQueryable<Application> GetByStudentIdQueryable(int studentId)
        {
            return _dbContext.Applications
                .Include(a => a.Opportunity)
                    .ThenInclude(o => o.Institution)
                .Where(a => a.StudentID == studentId)
                .AsNoTracking();
        }

        public IQueryable<Application> GetByOpportunityIdQueryable(int opportunityId)
        {
            return _dbContext.Applications
                .Include(a => a.Student)
                .Where(a => a.OpportunityID == opportunityId)
                .AsNoTracking();
        }

        public IQueryable<Application> GetByInstitutionIdQueryable(int institutionId)
        {
            return _dbContext.Applications
                .Include(a => a.Student)
                .Include(a => a.Opportunity)
                .Where(a => a.Opportunity.InstitutionID == institutionId)
                .AsNoTracking();
        }

        #endregion

        #region Get

        public async Task<List<Application>> GetAllAsync()
        {
            return await GetAllQueryable().ToListAsync();
        }

        public async Task<Application?> GetByIdAsync(int applicationId)
        {
            return await _dbContext.Applications
                .Include(a => a.Student)
                .Include(a => a.Opportunity)
                    .ThenInclude(o => o.Institution)
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);
        }

        // Tracked version - use for Update/Delete operations
        public async Task<Application?> GetByIdModifyAsync(int applicationId)
        {
            return await _dbContext.Applications
                .Include(a => a.Opportunity)
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId);
        }

        #endregion

        #region CRUD

        public async Task AddAsync(Application application)
        {
            await _dbContext.Applications.AddAsync(application);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Application application)
        {
            _dbContext.Applications.Update(application);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int applicationId)
        {
            var application = await GetByIdModifyAsync(applicationId);

            if (application == null)
                throw new Exception($"Application {applicationId} Not Found");

            _dbContext.Applications.Remove(application);
            await _dbContext.SaveChangesAsync();
        }

        #endregion

        #region Status

        public async Task<bool> UpdateStatusAsync(int applicationId, ApplicationStatus newStatus)
        {
            var application = await GetByIdModifyAsync(applicationId);

            if (application == null)
                return false;

            application.Status = newStatus;
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> WithdrawAsync(int applicationId, int studentId)
        {
            var application = await _dbContext.Applications
                .FirstOrDefaultAsync(a => a.ApplicationID == applicationId && a.StudentID == studentId);

            if (application == null)
                return false;

            application.Status = ApplicationStatus.Withdrawn;
            await _dbContext.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Checks

        public async Task<bool> HasAppliedAsync(int studentId, int opportunityId)
        {
            return await _dbContext.Applications
                .AnyAsync(a => a.StudentID == studentId && a.OpportunityID == opportunityId);
        }

        public async Task<bool> ExistsAsync(int applicationId)
        {
            return await _dbContext.Applications
                .AnyAsync(a => a.ApplicationID == applicationId);
        }

        #endregion

        #region Statistics

        public async Task<int> CountByOpportunityIdAsync(int opportunityId)
        {
            return await _dbContext.Applications
                .CountAsync(a => a.OpportunityID == opportunityId);
        }

        public async Task<int> CountApprovedByOpportunityIdAsync(int opportunityId)
        {
            return await _dbContext.Applications
                .CountAsync(a => a.OpportunityID == opportunityId
                               && a.Status == ApplicationStatus.Approved);
        }

        public async Task<int> CountByStudentIdAsync(int studentId)
        {
            return await _dbContext.Applications
                .CountAsync(a => a.StudentID == studentId);
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _dbContext.Applications.CountAsync();
        }

        #endregion
    }
}