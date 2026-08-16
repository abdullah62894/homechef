using System.ComponentModel.DataAnnotations;

namespace HomeChef.Application.Features.Chefs.Contracts;

public sealed class UpdateChefProfileRequest
{
    [Required(ErrorMessage = "Display name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Display name must be between 2 and 100 characters.")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bio is required.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Bio must be between 10 and 2000 characters.")]
    public string Bio { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "City must be between 2 and 100 characters.")]
    public string City { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Area must be at most 100 characters.")]
    public string? Area { get; set; }

    /// <summary>Cuisine tags. Cleaned and limited to 10 by the service.</summary>
    public string[]? Cuisines { get; set; }
}