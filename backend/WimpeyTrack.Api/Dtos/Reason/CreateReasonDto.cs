using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Reason;

public class CreateReasonDto
{
    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
    public string Name { get; set; } = string.Empty;
}