using JobAutoApply.API.Data;
using JobAutoApply.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobAutoApply.API.Controllers
{
    [ApiController]
    [Route("api/job-database")]
    public class JobDatabaseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public JobDatabaseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Job>>> GetJobs()
        {
            var jobs = await _context.Jobs.AsNoTracking().ToListAsync();
            return Ok(jobs);
        }

            }
}
