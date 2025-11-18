using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ServiceCatalogService.Api.Responses;
using ServiceCatalogService.Api.Extensions;
using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Api.Services;
using ServiceCatalogService.Api.Exceptions;
using ServiceCatalogService.Database.Repositories.Interfaces;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Api.Controllers;

[Route("api/categories")]
[ApiController]
public class CategoriesController(ICategoryRepository categoryRepository, IUserContextService userContextService) : ControllerBase
{
    /// <summary>
    /// Get category of a specific service
    /// </summary>
    /// <remarks>
    /// **Customers only**: Retrieves the category associated with a specific service.
    /// </remarks>
    /// <param name="serviceId">The unique identifier of the service</param>
    /// <returns>Category information associated with the service</returns>
    /// <response code="200">Successfully retrieved category information</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User is not a Customer (Providers not allowed)</response>
    /// <response code="404">Service not found or service has no assigned category</response>
    [HttpGet("/api/services/{serviceId}/category")]
    [Authorize]
    public async Task<ActionResult<CategoryResponse>> GetServiceCategory(Guid serviceId)
    {
        if (!userContextService.IsCustomer())
        {
            throw new AuthorizationException("Category", "read", "Access denied. Only customers can access service category information.");
        }

        var category = await categoryRepository.GetCategoryByServiceIdAsync(serviceId);
        return category == null ? throw new NotFoundException("Category", $"No category found for service {serviceId}") : Ok(category.ToCategoryResponse());
    }

    /// <summary>
    /// Get all categories for a specific tenant
    /// </summary>
    /// <remarks>
    /// **Customers only**: Retrieves all categories belonging to a specific tenant/organization.
    /// </remarks>
    /// <param name="tenantId">The unique identifier of the tenant/organization</param>
    /// <returns>List of all categories belonging to the specified tenant</returns>
    /// <response code="200">Successfully retrieved categories list</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User is not a Customer (Providers not allowed)</response>
    [HttpGet("/api/tenants/{tenantId}/categories")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetTenantCategories(Guid tenantId)
    {
        if (!userContextService.IsCustomer())
        {
            throw new AuthorizationException("Category", "read", "Access denied. Only customers can access tenant categories.");
        }

        var categories = await categoryRepository.GetCategoriesByTenantIdAsync(tenantId);
        return Ok(categories.Select(c => c.ToCategoryResponse()));
    }

    /// <summary>
    /// Get all categories belonging to the authenticated provider
    /// </summary>
    /// <remarks>
    /// **Providers only**: Retrieves all categories managed by the authenticated provider.
    /// </remarks>
    /// <returns>List of categories belonging to the authenticated provider's tenant</returns>
    /// <response code="200">Successfully retrieved provider's categories</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User is not a Provider (Customers not allowed)</response>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetProviderCategories()
    {
        userContextService.ValidateProviderAccess();
        var tenantId = userContextService.GetTenantId();
        var categories = await categoryRepository.GetCategoriesByTenantIdAsync(tenantId);
        return Ok(categories.Select(c => c.ToCategoryResponse()));
    }

    /// <summary>
    /// Get category by ID - Providers only
    /// </summary>
    [HttpGet("{categoryId:guid}")]
    [Authorize]
    public async Task<ActionResult<CategoryResponse>> GetCategory(Guid categoryId)
    {
        userContextService.ValidateProviderAccess();
        var category = await categoryRepository.GetCategoryByIdAsync(categoryId);

        if (category == null)
        {
            throw new NotFoundException("Category", categoryId);
        }

        userContextService.ValidateTenantAccess(category.TenantId, "Category");
        return Ok(category.ToCategoryResponse());
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <remarks>
    /// **Providers only**: Creates a new category within the provider's tenant.
    /// </remarks>
    /// <param name="request">Category creation details including name and description</param>
    /// <returns>Created category information with location header</returns>
    /// <response code="201">Category successfully created</response>
    /// <response code="400">Invalid request data (missing name, etc.)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User is not a Provider</response>
    /// <response code="409">Category name already exists in this tenant</response>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        userContextService.ValidateProviderAccess();
        var tenantId = userContextService.GetTenantId();
        var category = request.ToEntity(tenantId);
        category.Id = Guid.NewGuid();

        var existingCategory = await categoryRepository.GetCategoryByNameAndTenantAsync(category.Name, tenantId);
        if (existingCategory != null)
        {
            throw new ConflictException("DuplicateCategoryName", $"A category with name '{category.Name}' already exists in this tenant.");
        }

        await categoryRepository.CreateCategoryAsync(category);
        var response = category.ToCategoryResponse();
        return CreatedAtAction(nameof(GetCategory), new { categoryId = response.Id }, response);
    }

    /// <summary>
    /// Update category - Providers only (names must remain unique within tenant)
    /// </summary>
    [HttpPatch("{categoryId:guid}")]
    [Authorize]
    public async Task<ActionResult<CategoryResponse>> UpdateCategory(Guid categoryId, [FromBody] UpdateCategoryRequest request)
    {
        userContextService.ValidateProviderAccess();
        var existingCategory = await categoryRepository.GetCategoryByIdAsync(categoryId);

        if (existingCategory == null)
        {
            throw new NotFoundException("Category", categoryId);
        }

        userContextService.ValidateTenantAccess(existingCategory.TenantId, "Category");

        if (!string.IsNullOrEmpty(request.Name) && request.Name != existingCategory.Name)
        {
            var duplicateCategory = await categoryRepository.GetCategoryByNameAndTenantAsync(request.Name, existingCategory.TenantId);
            if (duplicateCategory != null && duplicateCategory.Id != categoryId)
            {
                throw new ConflictException("DuplicateCategoryName", $"A category with name '{request.Name}' already exists in this tenant.");
            }
        }

        var updateRequest = new UpdateCategory
        {
            Name = request.Name ?? existingCategory.Name,
            Description = request.Description ?? existingCategory.Description
        };

        var success = await categoryRepository.UpdateCategoryAsync(categoryId, updateRequest);
        if (!success)
        {
            throw new DatabaseOperationException("Update", "Category", "Failed to update category");
        }

        var updatedCategory = await categoryRepository.GetCategoryByIdAsync(categoryId);
        return Ok(updatedCategory!.ToCategoryResponse());
    }

    /// <summary>
    /// Delete category - Providers only (services will have null CategoryId)
    /// </summary>
    [HttpDelete("{categoryId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategory(Guid categoryId)
    {
        userContextService.ValidateProviderAccess();
        var existingCategory = await categoryRepository.GetCategoryByIdAsync(categoryId);

        if (existingCategory == null)
        {
            throw new NotFoundException("Category", categoryId);
        }

        userContextService.ValidateTenantAccess(existingCategory.TenantId, "Category");
        var success = await categoryRepository.DeleteCategoryAsync(categoryId);

        return !success ? throw new DatabaseOperationException("Delete", "Category", "Failed to delete category") : NoContent();
    }
}