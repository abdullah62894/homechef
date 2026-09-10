using HomeChef.Api.Common;
using HomeChef.Application.Features.Meals;
using HomeChef.Domain.Meals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeChef.Api.Controllers;

[ApiController]
[Route("api/meals")]
public sealed class MealsController : ControllerBase
{
    private readonly IMealCategoryService _mealCategoryService;

    public MealsController(IMealCategoryService mealCategoryService) => _mealCategoryService = mealCategoryService;

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<MealCategory>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
        => Ok(new ApiResponse<IReadOnlyList<MealCategory>>(await _mealCategoryService.ListAsync()));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<MealCategory>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var category = await _mealCategoryService.GetByIdAsync(id);
        return category is null ? NotFound() : Ok(new ApiResponse<MealCategory>(category));
    }
}
