
using JobAutoApply.API.Interface;
using JobAutoApply.API.Services;
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
        private readonly IEnumerable<IJobProvider> _jobProviders;

        public JobsController(GreenhouseService greenhouseService, JobImportService jobImportService,
        LeverService leverService, AshbyService ashbyService, IEnumerable<IJobProvider> jobProviders)
        {
            _greenhouseservice = greenhouseService;
            _jobImportService = jobImportService;
            _leverService = leverService;
            _ashbyService = ashbyService;
            _jobProviders = jobProviders;
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
            var provider = _jobProviders.FirstOrDefault(
                p => p.PlatformName.Equals("Greenhouse", StringComparison.OrdinalIgnoreCase)
            );

            if (provider == null)
            {
                return NotFound(new
                {
                    message = "Greenhouse provider not registered"
                });
            }

            int imported = await _jobImportService.ImportJobsAsync(provider, boardToken);


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
                return StatusCode(502, new { message = "Unable to Fetch AshBy Jobs" });
            }

        }



        // provider ashby using interface
        [HttpGet("providers/{platform}/{boardToken}")]
        public async Task<IActionResult> TestAshbyProvider(string platform, string boardToken)
        {
            var provider = _jobProviders.FirstOrDefault(
                p => p.PlatformName.Equals(
                    platform, StringComparison.OrdinalIgnoreCase
                )
            );

            if (provider == null) return NotFound(new { message = $"{platform} provider not registered" });


            var jobs = await provider.GetJobsAsync(boardToken);

            return Ok(new { Platform = provider.PlatformName, boardToken = boardToken, TotalJobs = jobs.Count, Jobs = jobs });
        }


        [HttpPost("import/{platform}/{boardToken}")]
        public async Task<IActionResult> ImportFromProvider(string platform, string boardToken)
        {
            var provider = _jobProviders.FirstOrDefault(
                p => p.PlatformName.Equals(
                    platform, StringComparison.OrdinalIgnoreCase
                )
            );

            if (provider == null)
            {
                return NotFound(new
                {
                    message = $"Provider '{platform}' not found"
                });
            }

            int imported = await _jobImportService.ImportJobsAsync(
                provider, boardToken
            );

            return Ok(
                new { platform = provider.PlatformName, boardToken, imported }
            );
        }


        //[]
        [HttpPost("sync/{platform}/{boardToken}")]
        public async Task<IActionResult> SyncJobs(string platform, string boardToken)
        {
            var provider = _jobProviders.FirstOrDefault(
                p => p.PlatformName.Equals(platform, StringComparison.OrdinalIgnoreCase)
            );

            if (provider == null)
            {
                return NotFound(new
                {
                    message = $"Provider '{platform}' not registered"
                });
            }

            var result = await _jobImportService.SyncJobsAsync(provider,boardToken);
            return Ok(result);


        }
    }
}
