using Hangfire;
using JobAutoApply.API.Data;
using JobAutoApply.API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// HTTP Client import services
builder.Services.AddHttpClient<GreenhouseService>();
builder.Services.AddHttpClient<LeverService>();
builder.Services.AddHttpClient<AshbyService>();

// Application Services
builder.Services.AddScoped<JobImportService>();
builder.Services.AddScoped<JobSourceService>();

// SQL Server + Entity Framework Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString(
            "DefaultConnection"
        )
    )
);

// Hangfire Configuration
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")!
    );
});

// Hangfire Background Server
builder.Services.AddHangfireServer();

var app = builder.Build();

var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();

recurringJobManager.AddOrUpdate<JobImportService>(
    "import-all-jobs",
    service => service.ImportAllJobsAsync(),
    "*/30 * * * *",
    new RecurringJobOptions());

// Hangfire Dashboard (local development only)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();