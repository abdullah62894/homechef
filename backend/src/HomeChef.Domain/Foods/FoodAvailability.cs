using HomeChef.Domain.Meals;

namespace HomeChef.Domain.Foods;

/// <summary>
/// Availability schedule for a specific food item.
/// Determines when a dish can be ordered based on day, time, and meal category.
/// A dish with IsAllDay=true is available whenever the chef is open.
/// </summary>
public class FoodAvailability
{
    public Guid Id { get; set; }

    public Guid FoodItemId { get; set; }

    public FoodItem FoodItem { get; set; } = null!;

    /// <summary>Days this dish is available (0=Sunday, 6=Saturday). Stored as int array.</summary>
    public int[] AvailableDays { get; set; } = [];

    /// <summary>Start time of the availability window (time-of-day, UTC).</summary>
    public TimeOnly StartTime { get; set; }

    /// <summary>End time of the availability window (time-of-day, UTC).</summary>
    public TimeOnly EndTime { get; set; }

    /// <summary>If true, available all day whenever the chef is open.</summary>
    public bool IsAllDay { get; set; }

    /// <summary>Optional link to a meal category for this schedule.</summary>
    public Guid? MealCategoryId { get; set; }

    public MealCategory? MealCategory { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
