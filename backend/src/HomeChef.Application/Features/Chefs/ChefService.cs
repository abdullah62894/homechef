using HomeChef.Application.Common;
using HomeChef.Application.Common.Errors;
using HomeChef.Application.Common.Exceptions;
using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Domain.Chefs;

namespace HomeChef.Application.Features.Chefs;

public sealed class ChefService : IChefService
{
    private const int MaxCuisines = 10;
    private const int MaxCuisineLength = 50;

    private readonly IChefProfileRepository _repository;

    public ChefService(IChefProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ChefListItemDto>> ListAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await _repository.ListAsync(page, pageSize, cancellationToken);
        var hasMore = page * pageSize < total;

        return new PagedResult<ChefListItemDto>(
            items.Select(ToListItem).ToList(),
            page,
            pageSize,
            total,
            hasMore);
    }

    public async Task<ChefProfileDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        return ToDto(profile);
    }

    public async Task<ChefProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        return ToDto(profile);
    }

    public async Task<ChefProfileDto> CreateAsync(
        Guid userId,
        CreateChefProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _repository.GetByUserIdAsync(userId, cancellationToken) is not null)
        {
            throw new BusinessException(ErrorCodes.ChefProfileExists, "A chef profile already exists for this account.");
        }

        var now = DateTime.UtcNow;

        var profile = new ChefProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            DisplayName = request.DisplayName.Trim(),
            Bio = request.Bio.Trim(),
            City = request.City.Trim(),
            Area = string.IsNullOrWhiteSpace(request.Area) ? null : request.Area.Trim(),
            Cuisines = NormalizeCuisines(request.Cuisines),
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };

        await _repository.AddAsync(profile, cancellationToken);

        return ToDto(profile);
    }

    public async Task<ChefProfileDto> UpdateAsync(
        Guid userId,
        UpdateChefProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var profile = await _repository.GetByUserIdAsync(userId, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.ChefProfileNotFound, "Chef profile was not found.");

        profile.DisplayName = request.DisplayName.Trim();
        profile.Bio = request.Bio.Trim();
        profile.City = request.City.Trim();
        profile.Area = string.IsNullOrWhiteSpace(request.Area) ? null : request.Area.Trim();
        profile.Cuisines = NormalizeCuisines(request.Cuisines);
        profile.UpdatedAtUtc = DateTime.UtcNow;

        await _repository.UpdateAsync(profile, cancellationToken);

        return ToDto(profile);
    }

    private static string[] NormalizeCuisines(string[]? cuisines)
    {
        return (cuisines ?? [])
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Select(c => c.Length > MaxCuisineLength ? c[..MaxCuisineLength] : c)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
            .Take(MaxCuisines)
            .ToArray();
    }

    private static ChefListItemDto ToListItem(ChefProfile profile)
    {
        return new ChefListItemDto
        {
            Id = profile.Id,
            DisplayName = profile.DisplayName,
            Bio = profile.Bio,
            City = profile.City,
            Area = profile.Area,
            Cuisines = profile.Cuisines,
            PhotoUrl = profile.PhotoUrl,
        };
    }

    private static ChefProfileDto ToDto(ChefProfile profile)
    {
        return new ChefProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            DisplayName = profile.DisplayName,
            Bio = profile.Bio,
            City = profile.City,
            Area = profile.Area,
            Cuisines = profile.Cuisines,
            PhotoUrl = profile.PhotoUrl,
            CreatedAtUtc = profile.CreatedAtUtc,
            UpdatedAtUtc = profile.UpdatedAtUtc,
        };
    }
}