namespace HomeChef.Domain.Chefs;

/// <summary>
/// A chef's active/opening hours for a specific day of the week.
/// A chef can have multiple time windows per day (e.g. breakfast 8-11, lunch 12-15, dinner 18-22).
/// </summary>
public class ChefAvailability
{
    public Guid Id { get; set; }

    public Guid ChefProfileId { get; set; }

    public ChefProfile ChefProfile { get; set; } = null!;

    /// <summary>Day of the week (0=Sunday, 6=Saturday).</summary>
    public int DayOfWeek { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>Opening time (time-of-day, UTC).</summary>
    public TimeOnly OpenTime { get; set; }

    /// <summary>Closing time (time-of-day, UTC).</summary>
    public TimeOnly CloseTime { get; set; }

    /// <summary>Optional label: "Breakfast", "Lunch", "Dinner", or null for general.</summary>
    public string? Label { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
