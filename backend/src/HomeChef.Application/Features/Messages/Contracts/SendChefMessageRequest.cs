using System.ComponentModel.DataAnnotations;

namespace HomeChef.Application.Features.Messages.Contracts;

public sealed class SendChefMessageRequest
{
    [Required(ErrorMessage = "Chef is required.")]
    public Guid ChefProfileId { get; set; }

    [Required(ErrorMessage = "Message body is required.")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Message must be between 1 and 2000 characters.")]
    public string Body { get; set; } = string.Empty;
}
