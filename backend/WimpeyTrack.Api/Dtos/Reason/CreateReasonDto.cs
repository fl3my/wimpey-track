using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Reason;

public class CreateReasonDto
{
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}