using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Preference;

public class CreatePreferenceDto
{
    [Required]
    [Range(0.5, 2, ErrorMessage = "Adjustment factor must be between 0.5 and 2")]
    public double MilesAdjustmentFactor { get; set; }
}