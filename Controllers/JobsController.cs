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
        private readonly JobImportService _jobImportService;
        private readonly LeverService _leverService;
        private readonly AshbyService _ashbyService;

        public JobsController(GreenhouseService greenhouseService, JobImportService jobImportService,
        LeverService leverService, AshbyService ashbyService)
        {
            _greenhouseservice = greenhouseService;
            _jobImportService = jobImportService;
            _leverService = leverService;
            _ashbyService = ashbyService;
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

        // Lever
        [HttpPost("lever/{companyToken}")]
        public async Task<IActionResult> GetLeverJobs(string companyToken)
        {
            try
            {
                var jobs = await _leverService.GetJobsAsync(companyToken);
                 
                return Ok(jobs);
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new
                {
                    message = "Unable to fetch Lever jobs."
                });
            }
        }


        //Ashby
        [HttpGet("ashby/{boardName}")]
        public async Task<IActionResult> GetAshbyJobs(string boardName)
        {
            try
            {
                var response = await _ashbyService.GetJobsAsync(boardName);
                return Ok(response);
            }
            catch (HttpRequestException)
            {
                return StatusCode(502, new{message = "Unable to Fetch AshBy Jobs"});
            }
            
        }
    }
}
