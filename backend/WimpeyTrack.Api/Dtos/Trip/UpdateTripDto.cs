using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.Trip;

public class UpdateTripDto
{
    [Required]
    public int Id { get; set; }
    [Required]
    public int LocationId { get; set; }
    [Required]
    public int ReasonId { get; set; }
}