using JobAutoApply.API.DTOs;
using JobAutoApply.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobAutoApply.API.Controllers
{
    [ApiController]
    [Route("api/job-sources")]
    public class JobSourcesController : ControllerBase
    {
        private readonly JobSourceService _service;
        public JobSourcesController(JobSourceService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var source = await _service.GetAllAsync();
            return Ok(source);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateJobSourceDto dto)
        {
            if (!dto.Platform.Equals("Greenhouse", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Only green house is supported currently");
            }

            var source = await _service.CreateAsync(dto);
            if (source == null) return Conflict("Job source already exists");

            return StatusCode(201, source);
        }
    
        
    }
}
