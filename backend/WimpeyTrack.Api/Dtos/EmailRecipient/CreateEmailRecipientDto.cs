using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.EmailRecipient;

public class CreateEmailRecipientDto
{
    [Required]
    public string FirstName { get; set; } =string.Empty;
    
    [Required]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}