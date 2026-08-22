using HomeChef.Application.Common;
using HomeChef.Application.Features.Chefs.Contracts;
using HomeChef.Application.Features.Images.Contracts;

namespace HomeChef.Application.Features.Chefs;

public interface IChefService
{
    /// <summary>Lists public chef profiles with optional filtering and proximity search.</summary>
    Task<PagedResult<ChefListItemDto>> ListAsync(
        ChefQueryFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a public chef profile by id.</summary>
    Task<ChefProfileDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns the calling chef's own profile.</summary>
    Task<ChefProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Creates a chef profile for the calling chef.</summary>
    Task<ChefProfileDto> CreateAsync(
        Guid userId,
        CreateChefProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Updates the calling chef's own profile.</summary>
    Task<ChefProfileDto> UpdateAsync(
        Guid userId,
        UpdateChefProfileRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Sets the calling chef's profile photo from an optimized upload.</summary>
    Task<ChefProfileDto> SetMyPhotoAsync(
        Guid userId,
        ImageUploadResult image,
        CancellationToken cancellationToken = default);

    /// <summary>Removes the calling chef's profile photo.</summary>
    Task<ChefProfileDto> ClearMyPhotoAsync(Guid userId, CancellationToken cancellationToken = default);
}