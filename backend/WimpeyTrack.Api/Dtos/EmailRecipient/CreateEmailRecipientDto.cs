using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.EmailRecipient;

public class CreateEmailRecipientDto
{
    [Required]
    public string FirstName { get; set; }
    
    [Required]
    public string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}