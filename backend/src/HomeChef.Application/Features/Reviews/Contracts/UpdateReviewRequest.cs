using System.ComponentModel.DataAnnotations;

namespace HomeChef.Application.Features.Reviews.Contracts;

public sealed class UpdateReviewRequest
{
    [Required(ErrorMessage = "Rating is required.")]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Review comment is required.")]
    [StringLength(1000, MinimumLength = 3, ErrorMessage = "Comment must be between 3 and 1000 characters.")]
    public string Comment { get; set; } = string.Empty;
}
