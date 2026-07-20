using InternshipApi.Models;
using InternshipApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InternshipApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpportunitiesController : ControllerBase
    {
        private readonly TrainingOpportunityRepository _repository;

        public OpportunitiesController(TrainingOpportunityRepository repository)
        {
            _repository = repository;
        }

        // GET: api/opportunities/search?query=something
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<object>>> Search([FromQuery] string query)
        {
            IQueryable<Opportunity> queryable = string.IsNullOrWhiteSpace(query)
         ? _repository.GetAllQueryable()
         : _repository.SearchAsQueryable(query);
            var results = await queryable
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

            return Ok(results);

        }
    }
}
