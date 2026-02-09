using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WimpeyTrack.Api.Data;
using WimpeyTrack.Api.Dtos.EmailRecipient;
using WimpeyTrack.Api.Models;

namespace WimpeyTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailRecipientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmailRecipientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmailRecipientDto>>> GetEmails()
    {
        var emails = await _context.EmailRecipients.Select(e => new EmailRecipientDto()
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
        }).ToListAsync();
        
        return emails;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmailRecipientDto>> GetEmailRecipient(Guid id)
    {
        var  email = await _context.EmailRecipients.FindAsync(id);
        if (email == null)
            return NotFound();

        var dto = new EmailRecipientDto()
        {
            FirstName = email.FirstName,
            LastName = email.LastName,
            Email = email.Email,
        };

        return Ok(dto);
    }
    
    [HttpPost]
    public async Task<ActionResult<EmailRecipientDto>> PostEmailRecipient(CreateEmailRecipientDto dto)
    {
        var email = new EmailRecipient()
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
        };

        _context.EmailRecipients.Add(email);
        await _context.SaveChangesAsync();

        var emailDto = new EmailRecipientDto()
        {
            Id = email.Id,
            FirstName = email.FirstName,
            LastName = email.LastName,
            Email = email.Email,
        };
        
        return CreatedAtAction("GetEmailRecipient", new { id = emailDto.Id }, emailDto);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmailRecipient(Guid id)
    {
        var email = await _context.EmailRecipients.FindAsync(id);
        if (email == null)
            return NotFound();
        
        _context.EmailRecipients.Remove(email);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}