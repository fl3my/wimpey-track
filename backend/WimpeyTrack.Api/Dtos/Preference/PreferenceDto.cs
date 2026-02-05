using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Preference;

public class PreferenceDto
{
    [Required]
    public double MilesAdjustmentFactor { get; set; }
}