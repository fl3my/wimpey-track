
using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos;

public class UpdateReasonDto
{
    public int Id { get; set; }
    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}