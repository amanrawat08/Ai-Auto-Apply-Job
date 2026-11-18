using JobAutoApply.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobAutoApply.API.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobsController : ControllerBase
    {
        private readonly GreenhouseService _greenhouseservice;
        private readonly JobImportService _jobImportService ;

        public JobsController(GreenhouseService greenhouseService, JobImportService jobImportService)
        {
         _greenhouseservice = greenhouseService;
         _jobImportService = jobImportService;
        }

        [HttpGet("greenhouse/{boardToken}")]
        public async Task<IActionResult> GetGreenhouseJobs(string boardToken)
        {
            try
            {
                var jobs = await _greenhouseservice.GetJobAsync(boardToken);

                return Ok(jobs);
            }
            catch (System.Exception)
            {
                return StatusCode(502, "Unable to fetch the jobs from Greenhouse");
            }
        }


        [HttpPost("import/{boardToken}")]
        public async Task<IActionResult> ImportJobs(string boardToken)
        {
            var imported = await _jobImportService.ImportJobAsync(boardToken);

            return Ok(new
            {
                Message = "Imported Completed",
                ImportJobs = imported 
            });

        }

        [HttpPost("import-all")]
        public async Task<IActionResult> ImportAllJobs()
        {
            var result = await _jobImportService.ImportAllJobsAsync();
            return Ok(result);
        }
    }
}
