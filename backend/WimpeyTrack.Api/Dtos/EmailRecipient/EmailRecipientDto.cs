using System.ComponentModel.DataAnnotations;

namespace WimpeyTrack.Api.Dtos.EmailRecipient;

public class EmailRecipientDto
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    public string FirstName{ get; set; } =string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string Email{ get; set; } = string.Empty;
}