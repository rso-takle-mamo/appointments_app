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
    /// **CUSTOMERS:**
    /// - Can access category of any service from any tenant
    ///
    /// **PROVIDERS:**
    /// - Can only access category of services from their own tenant
    /// </remarks>
    /// <param name="serviceId">The unique identifier of the service</param>
    /// <returns>Category information associated with the service</returns>
    /// <response code="200">Successfully retrieved category information</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">Provider trying to access service from different tenant</response>
    /// <response code="404">Service not found or service has no assigned category</response>
    [HttpGet("/api/services/{serviceId}/category")]
    [Authorize]
    public async Task<ActionResult<CategoryResponse>> GetServiceCategory(Guid serviceId)
    {
        var category = await categoryRepository.GetCategoryByServiceIdAsync(serviceId);
        if (category == null)
        {
            throw new NotFoundException("Category", $"No category found for service {serviceId}");
        }

        if (!userContextService.IsCustomer())
        {
            userContextService.ValidateTenantAccess(category.TenantId, "Service");
        }

        return Ok(category.ToCategoryResponse());
    }

    
    /// <summary>
    /// Get categories with filtering
    /// </summary>
    /// <remarks>
    /// **CUSTOMERS:**
    /// - tenantId query parameter is REQUIRED
    ///
    /// **PROVIDERS:**
    /// - Access ONLY categories from their own tenant
    /// - Cannot specify tenantId parameter (rejected with authorization error)
    /// </remarks>
    /// <param name="tenantId">Tenant ID for customers (required), ignored for providers</param>
    /// <returns>List of categories based on user role and filters</returns>
    /// <response code="200">Categories retrieved successfully (may be empty if tenant has no categories)</response>
    /// <response code="400">Invalid request (missing tenantId for customers, etc.)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User not authorized or provider attempted to specify tenantId</response>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetCategories([FromQuery] Guid? tenantId)
    {
        if (userContextService.IsCustomer())
        {
            if (!tenantId.HasValue)
            {
                return BadRequest("TenantId is required for customers");
            }

            var categories = await categoryRepository.GetCategoriesByTenantIdAsync(tenantId.Value);
            return Ok(categories.Select(c => c.ToCategoryResponse()));
        }
        // Provider
        else
        {
            if (tenantId.HasValue)
            {
                throw new AuthorizationException("Category", "filter", "Providers cannot specify tenantId parameter. Tenant access is automatically enforced from your authentication token.");
            }

            userContextService.ValidateProviderAccess();
            var providerTenantId = userContextService.GetTenantId();
            var categories = await categoryRepository.GetCategoriesByTenantIdAsync(providerTenantId);
            return Ok(categories.Select(c => c.ToCategoryResponse()));
        }
    }

    /// <summary>
    /// Get category by id
    /// </summary>
    /// <remarks>
    /// **Providers only**: Deletes a category within the provider's tenant.
    /// </remarks>
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
    /// Update category
    /// </summary>
    /// <remarks>
    /// **Providers only**: Updates a category within the provider's tenant.
    /// </remarks>
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
    /// Delete category
    /// </summary>
    /// <remarks>
    /// **Providers only**: Deletes a category within the provider's tenant.
    /// </remarks>
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