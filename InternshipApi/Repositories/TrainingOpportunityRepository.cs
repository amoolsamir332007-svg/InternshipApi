using InternshipApi.Data;
using InternshipApi.Models;
using Microsoft.EntityFrameworkCore;


namespace InternshipApi.Repositories
{
    public class TrainingOpportunityRepository
    {
        private readonly AppDbContext _dbContext;

        public TrainingOpportunityRepository(
            AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        #region Queryable

        public IQueryable<Opportunity>
            GetAllQueryable()
        {
            return _dbContext.Opportunities
                .Include(x => x.Institution)
               
                .AsNoTracking();
        }

        public IQueryable<Opportunity>
            GetInstitutionOpportunitiesQueryable(
                int institutionId)
        {
            return _dbContext.Opportunities
                .Include(x => x.Institution)
          
                .Where(x =>
                    x.InstitutionID == institutionId)
                .AsNoTracking();
        }

        #endregion

        #region Get

        public async Task<List<Opportunity>>
            GetAllAsync()
        {
            return await GetAllQueryable()
                .ToListAsync();
        }

        public async Task<Opportunity?>
            GetByIdAsync(int opportunityId)
        {
            return await _dbContext
                .Opportunities
                .Include(x => x.Institution)
           
                .FirstOrDefaultAsync(x =>
                    x.OpportunityID == opportunityId);
        }

        public async Task<Opportunity?>
            GetByIdModifyAsync(int opportunityId)
        {
            return await _dbContext
                .Opportunities
                .FirstOrDefaultAsync(x =>
                    x.OpportunityID == opportunityId);
        }

        public async Task<Opportunity?>
            GetDetailsAsync(int opportunityId)
        {
            return await _dbContext
                .Opportunities

                .Include(x => x.Institution)

        

                .AsNoTracking()

                .FirstOrDefaultAsync(x => x.OpportunityID == opportunityId);
        }

        #endregion

        #region CRUD

        public async Task AddAsync(
            Opportunity opportunity)
        {
            await _dbContext
                .Opportunities
                .AddAsync(opportunity);

            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(
            Opportunity opportunity)
        {
            _dbContext
                .Opportunities
                .Update(opportunity);

            await _dbContext.SaveChangesAsync();
        }

        #endregion

        #region Delete

       
        public async Task DeleteAsync(
            int opportunityId)
        {
            var opportunity =
                await GetByIdModifyAsync(opportunityId);

            if (opportunity == null)
            {
                throw new Exception(
                    $"Opportunity {opportunityId} Not Found");
            }

            _dbContext
                .Opportunities
                .Remove(opportunity);

            await _dbContext.SaveChangesAsync();
        }

        #endregion

        #region Status

        public async Task<bool>
            ToggleStatusAsync(int opportunityId)
        {
            var opportunity =
                await GetByIdModifyAsync(opportunityId);

            if (opportunity == null)
                return false;

            opportunity.Status =
                opportunity.Status ==
                Opportunity.OpportunityStatus.Open

                ? Opportunity.OpportunityStatus.Closed

                : Opportunity.OpportunityStatus.Open;

            await _dbContext.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Search

        public async Task<IEnumerable<Opportunity>>
            SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<Opportunity>();

            return await _dbContext
                .Opportunities
                .Where(x =>
                    EF.Functions.Like(
                        x.Title,
                        $"%{query}%"))

                .AsNoTracking()
                .ToListAsync();
        }

        #endregion

        #region Statistics

        public async Task<int>
            GetTotalCountAsync()
        {
            return await _dbContext
                .Opportunities
                .CountAsync();
        }

        public async Task<int>
            GetOpenCountAsync()
        {
            return await _dbContext
                .Opportunities
                .CountAsync(x =>
                    x.Status ==
                    Opportunity.OpportunityStatus.Open);
        }

        public async Task<int>
            GetClosedCountAsync()
        {
            return await _dbContext
                .Opportunities
                .CountAsync(x =>
                    x.Status ==
                    Opportunity.OpportunityStatus.Closed);
        }

        public async Task<int>
            GetCancelledCountAsync()
        {
            return await _dbContext
                .Opportunities
                .CountAsync(x =>
                    x.Status ==
                    Opportunity.OpportunityStatus.Cancelled);
        }

        #endregion

        #region Exists

        public async Task<bool>
            ExistsAsync(int opportunityId)
        {
            return await _dbContext
                .Opportunities
                .AnyAsync(x =>
                    x.OpportunityID == opportunityId);
        }

        #endregion
    }
}