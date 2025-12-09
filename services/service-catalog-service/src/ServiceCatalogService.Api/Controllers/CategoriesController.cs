using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ServiceCatalogService.Api.Responses;
using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Api.Services;
using ServiceCatalogService.Api.Services.Interfaces;

namespace ServiceCatalogService.Api.Controllers;

[Route("api/categories")]
[ApiController]
public class CategoriesController(ICategoryService categoryService, IUserContextService userContextService) : ControllerBase
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
    [HttpGet("/api/services/{serviceId}/category")]
    [Authorize]
    public async Task<ActionResult<CategoryResponse>> GetServiceCategory(Guid serviceId)
    {
        var isCustomer = userContextService.IsCustomer();
        Guid? userTenantId = isCustomer ? null : userContextService.GetTenantId();

        var category = await categoryService.GetServiceCategoryAsync(serviceId, isCustomer, userTenantId);

        return Ok(category);
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
    /// <param name="tenantId">Tenant ID for customers (required), forbidden for providers</param>
    /// <param name="pagination">Pagination parameters (offset and limit)</param>
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PaginatedResponse<CategoryResponse>>> GetCategories([FromQuery] Guid? tenantId, [FromQuery] PaginationRequest pagination)
    {
        var isCustomer = userContextService.IsCustomer();
        Guid? userTenantId = isCustomer ? null : userContextService.GetTenantId();

        if (!isCustomer)
        {
            userContextService.ValidateProviderAccess();
        }

        var (categories, totalCount) = await categoryService.GetCategoriesAsync(tenantId, isCustomer, userTenantId, pagination.Offset, pagination.Limit);

        var response = new PaginatedResponse<CategoryResponse>
        {
            Offset = pagination.Offset,
            Limit = pagination.Limit,
            TotalCount = totalCount,
            Data = categories.ToList()
        };

        return Ok(response);
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
        var userTenantId = userContextService.GetTenantId();

        var category = await categoryService.GetCategoryByIdAsync(categoryId, userTenantId);

        return Ok(category);
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    /// <remarks>
    /// **Providers only**: Creates a new category within the provider's tenant.
    /// </remarks>
    /// <param name="request">Category creation details including name and description</param>
    /// <response code="201">Category successfully created</response>
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<CategoryResponse>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        userContextService.ValidateProviderAccess();
        var tenantId = userContextService.GetTenantId();

        var response = await categoryService.CreateCategoryAsync(request, tenantId);

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
        var userTenantId = userContextService.GetTenantId();

        var response = await categoryService.UpdateCategoryAsync(categoryId, request, userTenantId);

        return Ok(response);
    }

    /// <summary>
    /// Delete category
    /// </summary>
    /// <remarks>
    /// **Providers only**: Deletes a category within the provider's tenant.
    /// </remarks>
    /// <response code="204">Category successfully deleted</response>
    [HttpDelete("{categoryId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteCategory(Guid categoryId)
    {
        userContextService.ValidateProviderAccess();
        var userTenantId = userContextService.GetTenantId();

        await categoryService.DeleteCategoryAsync(categoryId, userTenantId);

        return NoContent();
    }
}