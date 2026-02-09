using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.EmailRecipient;

public class EmailRecipientDto
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string FirstName{ get; set; }
    [Required]
    public string LastName { get; set; }
    [Required]
    public string Email{ get; set; }
}