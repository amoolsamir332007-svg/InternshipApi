

using InternshipApi.Data;
using InternshipApi.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipApi.Repository
{
    //Training Institution Repository
    public class TrainingInstitutionRepository
    {
        private readonly AppDbContext _dbContext;

        public TrainingInstitutionRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // This is Queryable [Use In Pagination]
        public IQueryable<Institution> GetAllQueryable() => _dbContext
            .Institutions.Include(ti => ti.Opportunities)
            .AsNoTracking();
            


        // CRUD Methode ....

        public async Task<List<Institution>> GetAllAsync() =>await _dbContext
            .Institutions
            .AsNoTracking()
            .ToListAsync();

        
        public async Task<List<Institution>> GetAllAsyncWithOpportunity()=> await _dbContext
            .Institutions
            .Include(ti=>ti.Opportunities)
            .AsNoTracking()
            .ToListAsync();

        public async Task<Institution?> GetByIdAsync(int? InstituationID)
        {
            return await _dbContext
                .Institutions
                .Include(ti => ti.Opportunities)
                .FirstOrDefaultAsync(ti => ti.InstituationID == InstituationID);
        }
          
        //To Performance in Soft Delete { Take The University Without Include}
        public async Task<Institution?> GetByIdModifyAsync(int InstituationID)
        {
            return await _dbContext
                .Institutions
                .FirstOrDefaultAsync(ti => ti.InstituationID == InstituationID);
        }



      

       


        public async Task AddAsync(Institution trainingInstitution)
        {
           

            await _dbContext.Institutions.AddAsync(trainingInstitution);

            await _dbContext.SaveChangesAsync();

        }

        public async Task UpdateAsync(Institution trainingInstitution)
        {
            _dbContext.Institutions.Update(trainingInstitution);

            await _dbContext.SaveChangesAsync();

        }

        

        // Full Delete { I Will Delete The institution From The DataBase }
        public async Task FullDeleteInstitution(int InstituationID)
        {
            var instituation = await GetByIdModifyAsync(InstituationID);

            if (instituation is not null)
            {

                _dbContext.Institutions.Remove(instituation);
                await _dbContext.SaveChangesAsync();
            }
            else
            {
                throw new Exception($"Unable To Find the Instituation {InstituationID}");
            }
        }


         public async Task<bool> ExistsAny(int InstituationID)
         {
              return await _dbContext
                .Institutions
                .AnyAsync(u => u.InstituationID == InstituationID);
         }


        // Filtring Methode .....
        public async Task<IEnumerable<Institution?>> GetBySearchAsync(string query)
        {
            if (String.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<Institution?>();

            return await _dbContext.Institutions
                .Where(u => EF.Functions.Like(u.Name, $"%{query}%"))
                .AsNoTracking()
                .ToListAsync();

        }


       

        public async Task<int> GetTotalCountAsync()
        {
            // All TrainingInstitution In The Database 
            return await _dbContext
                .Institutions
                .CountAsync();
        }

       

       
        //Get Applications for Institution
        public IQueryable<Application> GetApplicationsForInstitution(int? InstituationID)
        {
            return _dbContext.Applications
                .Include(a => a.Student)
          
                .Include(a => a.Opportunity)
                .Where(a => a.Opportunity.InstitutionID == InstituationID)
                .AsNoTracking().AsQueryable();
        }
        public IQueryable<Opportunity> GetTrainingOpportunitiesQueryable(int? InstituationID)
        {
            return _dbContext.Opportunities
                
                .Where(t=>t.InstitutionID== InstituationID)
                .AsNoTracking().AsQueryable();
        }
        
       
        public IQueryable<Opportunity> GetTrainingsForInstitution(int institutionId)
        {
            return _dbContext.Opportunities
              
                .Where(x => x.InstitutionID == institutionId)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking();
        }
      
        

      
    }
}

