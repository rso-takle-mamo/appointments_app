using Microsoft.AspNetCore.Mvc;
using ServiceCatalogService.Api.Extensions;
using ServiceCatalogService.Api.Interfaces;
using ServiceCatalogService.Api.Models.DTOs;
using ServiceCatalogService.Api.Models.Entities;
using ServiceCatalogService.Api.Services;

namespace ServiceCatalogService.Api.Controllers;

[Route("api/services/[controller]")]
[ApiController]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategories()
    {
        var categories = await categoryService.GetAllCategoriesAsync();
        var response = categories.Select(c => c.ToCategoryResponse());
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResponse>> GetCategory(Guid id)
    {
        var category = await categoryService.GetCategoryByIdAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category.ToCategoryResponse());
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var category = await categoryService.CreateCategoryAsync(request);
        var response = category.ToCategoryResponse();
        return CreatedAtAction(nameof(GetCategory), new { id = response.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryRequest request)
    {
        var success = await categoryService.UpdateCategoryAsync(id, request);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        var success = await categoryService.DeleteCategoryAsync(id);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}