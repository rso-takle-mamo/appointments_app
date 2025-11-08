using Microsoft.AspNetCore.Mvc;
using ServiceCatalogService.Api.Dtos;
using ServiceCatalogService.Api.Extensions;
using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Database.Repositories.Interfaces;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Api.Controllers;

[Route("api/services/[controller]")]
[ApiController]
public class CategoriesController(ICategoryRepository categoryRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategories()
    {
        var categories = await categoryRepository.GetAllCategoriesAsync();
        var response = categories.Select(c => c.ToCategoryResponse());
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

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
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

        return NoContent();
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