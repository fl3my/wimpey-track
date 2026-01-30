
using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Trip;

public class CreateTripDto
{
    [Required]
    public int LocationId { get; set; }
    [Required]
    public int ReasonId { get; set; }
}