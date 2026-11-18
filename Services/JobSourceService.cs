using System;
using JobAutoApply.API.Data;
using JobAutoApply.API.DTOs;
using JobAutoApply.API.Models;
using Microsoft.EntityFrameworkCore;

namespace JobAutoApply.API.Services;

public class JobSourceService
{
    private readonly ApplicationDbContext _context;
    public JobSourceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<JobSource>> GetAllAsync()
    {
        return await _context.JobSources.AsNoTracking().ToListAsync();
    }

    public async Task<JobSource?> CreateAsync(CreateJobSourceDto dto)
    {
        var exists = await _context.JobSources.AnyAsync(
            x=>x.BoardToken == dto.BoardToken && x.Platform == dto.Platform
        );

        if(exists) return null;

        var source = new JobSource
        {
             BoardToken = dto.BoardToken,
             CompanyName = dto.CompanyName,
             Platform = dto.Platform,
             IsActive = true
        };

        _context.JobSources.Add(source);
        await _context.SaveChangesAsync();
        return source;
    }
}
