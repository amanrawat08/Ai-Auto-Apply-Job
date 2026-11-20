namespace JobAutoApply.API.DTOs;

public class JobSyncResultDto
{
    public string Platform { get; set; } = "";
    public string BoardToken { get; set; } = "";
    public int Fetched { get; set; }
    public int Inserted { get; set; }
    public int Updated { get; set; }
    public int Reactivated { get; set; }
    public int Closed { get; set; }
    public DateTime SyncedAt { get; set; }
}
