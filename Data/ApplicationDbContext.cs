
using JobAutoApply.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobAutoApply.API.Data;

public class ApplicationDbContext:DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options){}

    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Job>().HasIndex(
            j => new
            {
                j.Source,
                j.ExternalJobId
            }
        ).IsUnique();
    }


}
