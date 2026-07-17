using InternshipApi.Models;
using InternshipApi.Repositories;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<IEnumerable<Opportunity>>> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required.");
            }

            var results = await _repository.SearchAsync(query);

            return Ok(results);
        }
    }
}