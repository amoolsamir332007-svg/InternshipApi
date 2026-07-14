using InternshipApi.Models;
using InternshipApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace InternshipApi.Controllers
{
    public class OpportunitiesController : Controller
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

            if (!results.Any())
            {
                return NotFound($"No opportunities found matching '{query}'.");
            }

            return Ok(results);
        }
    }
}