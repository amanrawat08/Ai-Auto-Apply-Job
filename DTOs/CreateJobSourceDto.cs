using System.ComponentModel.DataAnnotations;

namespace JobAutoApply.API.DTOs;

public class CreateJobSourceDto
{
    [Required]
    public string CompanyName { get; set; } = "";

    [Required]
    public string Platform { get; set; } = "";

    [Required]
    public string BoardToken { get; set; } = "";
}
