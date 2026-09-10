using HomeChef.Application.Features.Chefs;
using HomeChef.Application.Features.Foods;
using HomeChef.Domain.Chefs;
using HomeChef.Domain.Foods;

namespace HomeChef.Application.Features.Foods;

public interface IAvailabilityEngine
{
    Task<bool> IsChefOpenNowAsync(ChefProfile chef);
    Task<bool> IsFoodAvailableNowAsync(FoodItem food, ChefProfile chef);
    Task<List<FoodItem>> GetAvailableFoodsAsync(Guid chefId, List<FoodItem> foods);
    double CalculateDistanceKm(double lat1, double lng1, double lat2, double lng2);
    bool IsWithinDeliveryRadius(ChefProfile chef, double customerLat, double customerLng);
}

public sealed class AvailabilityEngine : IAvailabilityEngine
{
    private readonly IChefAvailabilityRepository _chefAvailRepo;
    private readonly IFoodAvailabilityRepository _foodAvailRepo;

    public AvailabilityEngine(IChefAvailabilityRepository chefAvailRepo, IFoodAvailabilityRepository foodAvailRepo)
    {
        _chefAvailRepo = chefAvailRepo;
        _foodAvailRepo = foodAvailRepo;
    }

    public async Task<bool> IsChefOpenNowAsync(ChefProfile chef)
    {
        var now = DateTime.UtcNow;
        var currentTime = TimeOnly.FromDateTime(now);
        var currentDay = (int)now.DayOfWeek;

        var windows = await _chefAvailRepo.ListByChefAndDayAsync(chef.Id, currentDay);
        return windows.Any(w => currentTime >= w.OpenTime && currentTime <= w.CloseTime);
    }

    public async Task<bool> IsFoodAvailableNowAsync(FoodItem food, ChefProfile chef)
    {
        if (!food.IsAvailable || chef.ApprovalStatus != ChefApprovalStatus.Approved)
            return false;

        if (!await IsChefOpenNowAsync(chef))
            return false;

        var schedules = await _foodAvailRepo.ListByFoodAsync(food.Id);
        if (schedules.Count == 0) return true;

        var now = DateTime.UtcNow;
        var currentTime = TimeOnly.FromDateTime(now);
        var currentDay = (int)now.DayOfWeek;

        return schedules.Any(s =>
            s.IsAllDay ||
            (s.AvailableDays.Contains(currentDay) && currentTime >= s.StartTime && currentTime <= s.EndTime));
    }

    public async Task<List<FoodItem>> GetAvailableFoodsAsync(Guid chefId, List<FoodItem> foods)
    {
        var now = DateTime.UtcNow;
        var currentTime = TimeOnly.FromDateTime(now);
        var currentDay = (int)now.DayOfWeek;

        var chefOpen = (await _chefAvailRepo.ListByChefAndDayAsync(chefId, currentDay))
            .Any(w => currentTime >= w.OpenTime && currentTime <= w.CloseTime);

        if (!chefOpen) return [];

        var foodIds = foods.Select(f => f.Id).ToList();
        var schedules = await _foodAvailRepo.ListByFoodsAsync(foodIds);

        return foods.Where(food =>
        {
            if (!food.IsAvailable) return false;
            var foodSchedules = schedules.Where(s => s.FoodItemId == food.Id).ToList();
            if (foodSchedules.Count == 0) return true;
            return foodSchedules.Any(s =>
                s.IsAllDay ||
                (s.AvailableDays.Contains(currentDay) && currentTime >= s.StartTime && currentTime <= s.EndTime));
        }).ToList();
    }

    public double CalculateDistanceKm(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6371.0;
        var dLat = ToRadians(lat2 - lat1);
        var dLng = ToRadians(lng2 - lng1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    public bool IsWithinDeliveryRadius(ChefProfile chef, double customerLat, double customerLng)
    {
        if (chef.Latitude == null || chef.Longitude == null || chef.DeliveryRadiusKm == null)
            return false;

        var distance = CalculateDistanceKm(chef.Latitude.Value, chef.Longitude.Value, customerLat, customerLng);
        return distance <= chef.DeliveryRadiusKm.Value;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}
