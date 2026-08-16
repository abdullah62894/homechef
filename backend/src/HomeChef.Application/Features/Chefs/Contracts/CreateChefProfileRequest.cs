using System.ComponentModel.DataAnnotations;

namespace HomeChef.Application.Features.Chefs.Contracts;

public sealed class CreateChefProfileRequest
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

    [StringLength(250, ErrorMessage = "Address must be at most 250 characters.")]
    public string? Address { get; set; }

    [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
    public double? Latitude { get; set; }

    [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
    public double? Longitude { get; set; }

    /// <summary>Cuisine tags. Cleaned and limited to 10 by the service.</summary>
    public string[]? Cuisines { get; set; }
}