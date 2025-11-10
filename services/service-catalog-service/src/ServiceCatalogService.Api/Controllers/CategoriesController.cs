using Microsoft.AspNetCore.Mvc;
using ServiceCatalogService.Api.Responses;
using ServiceCatalogService.Api.Extensions;
using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Database.Models;
using ServiceCatalogService.Database.Repositories.Interfaces;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Api.Controllers;

[Route("api/services/categories")]
[ApiController]
public class CategoriesController(ICategoryRepository categoryRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<CategoryResponse>>> GetCategories(
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 100,
        [FromQuery] Guid? tenantId = null)
    {
        if (offset < 0)
        {
            throw new ArgumentException("Offset must be greater than or equal to 0");
        }

        if (limit is < 1 or > 100)
        {
            throw new ArgumentException("Limit must be between 1 and 100");
        }

        var parameters = new PaginationParameters
        {
            Offset = offset,
            Limit = limit
        };

        var (categories, totalCount) = await categoryRepository.GetCategoriesAsync(parameters, tenantId);

        var response = new PaginatedResponse<CategoryResponse>
        {
            Offset = offset,
            Limit = limit,
            TotalCount = totalCount,
            Data = categories.Select(c => c.ToCategoryResponse()).ToList()
        };

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> GetCategory(Guid id)
    {
        var category = await categoryRepository.GetCategoryByIdAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category.ToCategoryResponse());
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var category = request.ToEntity();
        await categoryRepository.CreateCategoryAsync(category);
        var response = category.ToCategoryResponse();
        return CreatedAtAction(nameof(GetCategory), new { id = response.Id }, response);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var updateRequest = new UpdateCategory
        {
            Description = request.Description,
            Name = request.Name,
        };
        var success = await categoryRepository.UpdateCategoryAsync(id, updateRequest);

        if (!success)
        {
            return NotFound();
        }

        var updatedCategory = await categoryRepository.GetCategoryByIdAsync(id);
        return Ok(updatedCategory!.ToCategoryResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var success = await categoryRepository.DeleteCategoryAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}