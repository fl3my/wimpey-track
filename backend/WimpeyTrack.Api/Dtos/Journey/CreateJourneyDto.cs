using System.ComponentModel.DataAnnotations;
using WimpeyTrack.Api.Dtos.Trip;

namespace WimpeyTrack.Api.Dtos.Journey;

public class CreateJourneyDto
{
    [Required]
    [Range(typeof(DateOnly), "2024-04-06", "2034-04-06")]
    public DateOnly Date { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Journey must have at least 1 trip")]
    [MaxLength(10, ErrorMessage = "Journey cannot have more than 10 trips")]
     
    public List<CreateTripDto> Trips { get; set; } = new();
 }