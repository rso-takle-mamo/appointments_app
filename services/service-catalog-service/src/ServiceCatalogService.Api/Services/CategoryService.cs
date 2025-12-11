using ServiceCatalogService.Api.Requests;
using ServiceCatalogService.Api.Responses;
using ServiceCatalogService.Api.Extensions;
using ServiceCatalogService.Api.Exceptions;
using ServiceCatalogService.Api.Models;
using ServiceCatalogService.Api.Services.Interfaces;
using ServiceCatalogService.Database.Repositories.Interfaces;
using ServiceCatalogService.Database.UpdateModels;

namespace ServiceCatalogService.Api.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<CategoryResponse?> GetServiceCategoryAsync(Guid serviceId, bool isCustomer, Guid? userTenantId)
    {
        var category = await categoryRepository.GetCategoryByServiceIdAsync(serviceId);

        if (category == null)
        {
            throw new NotFoundException("Category", $"No category found for service {serviceId}");
        }

        if (isCustomer) return category.ToCategoryResponse();

        // Provider validation
        return category.TenantId != userTenantId ? throw new AuthorizationException("Category", "access", "Access denied. Category belongs to a different tenant.") : category.ToCategoryResponse();
    }

    public async Task<IEnumerable<CategoryResponse>> GetCategoriesAsync(Guid? tenantId, bool isCustomer, Guid? userTenantId)
    {
        if (isCustomer)
        {
            if (!tenantId.HasValue)
            {
                throw new ValidationException("TenantId is required for customers",
                    [new ValidationError { Field = "tenantId", Message = "TenantId is required for customers" }]);
            }

            var categories = await categoryRepository.GetCategoriesByTenantIdAsync(tenantId.Value);
            return categories.Select(c => c.ToCategoryResponse());
        }
        else // Provider
        {
            if (tenantId.HasValue)
            {
                throw new AuthorizationException("Category", "filter", "Providers cannot specify tenantId parameter. Tenant access is automatically enforced from your authentication token.");
            }

            if (!userTenantId.HasValue)
            {
                throw new AuthorizationException("Category", "access", "Provider must have a valid tenant ID");
            }

            var categories = await categoryRepository.GetCategoriesByTenantIdAsync(userTenantId.Value);
            return categories.Select(c => c.ToCategoryResponse());
        }
    }

    public async Task<(IEnumerable<CategoryResponse>, int TotalCount)> GetCategoriesAsync(Guid? tenantId, bool isCustomer, Guid? userTenantId, int offset, int limit)
    {
        if (isCustomer)
        {
            if (!tenantId.HasValue)
            {
                throw new ValidationException("TenantId is required for customers",
                    [new ValidationError { Field = "tenantId", Message = "TenantId is required for customers" }]);
            }

            var (categories, totalCount) = await categoryRepository.GetCategoriesByTenantIdAsync(tenantId.Value, offset, limit);
            return (categories.Select(c => c.ToCategoryResponse()), totalCount);
        }
        else // Provider
        {
            if (tenantId.HasValue)
            {
                throw new AuthorizationException("Category", "filter", "Providers cannot specify tenantId parameter. Tenant access is automatically enforced from your authentication token.");
            }

            if (!userTenantId.HasValue)
            {
                throw new AuthorizationException("Category", "access", "Provider must have a valid tenant ID");
            }

            var (categories, totalCount) = await categoryRepository.GetCategoriesByTenantIdAsync(userTenantId.Value, offset, limit);
            return (categories.Select(c => c.ToCategoryResponse()), totalCount);
        }
    }

    public async Task<CategoryResponse?> GetCategoryByIdAsync(Guid categoryId, Guid userTenantId)
    {
        var category = await categoryRepository.GetCategoryByIdAsync(categoryId);

        if (category == null)
        {
            throw new NotFoundException("Category", categoryId);
        }

        // Tenant validation
        return category.TenantId != userTenantId ? throw new AuthorizationException("Category", "access", "Access denied. Category belongs to a different tenant.") : category.ToCategoryResponse();
    }

    public async Task<CategoryResponse> CreateCategoryAsync(CreateCategoryRequest request, Guid tenantId)
    {
        var category = request.ToEntity(tenantId);
        category.Id = Guid.NewGuid();

        var existingCategory = await categoryRepository.GetCategoryByNameAndTenantAsync(category.Name, tenantId);
        if (existingCategory != null)
        {
            throw new ConflictException("DuplicateCategoryName", $"A category with name '{category.Name}' already exists in this tenant.");
        }

        await categoryRepository.CreateCategoryAsync(category);
        return category.ToCategoryResponse();
    }

    public async Task<CategoryResponse> UpdateCategoryAsync(Guid categoryId, UpdateCategoryRequest request, Guid userTenantId)
    {
        var existingCategory = await categoryRepository.GetCategoryByIdAsync(categoryId);

        if (existingCategory == null)
        {
            throw new NotFoundException("Category", categoryId);
        }

        // Tenant validation
        if (existingCategory.TenantId != userTenantId)
        {
            throw new AuthorizationException("Category", "update", "Access denied. Cannot update category from different tenant.");
        }

        // Check for duplicate name if name is being changed
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
        return updatedCategory!.ToCategoryResponse();
    }

    public async Task<bool> DeleteCategoryAsync(Guid categoryId, Guid userTenantId)
    {
        var existingCategory = await categoryRepository.GetCategoryByIdAsync(categoryId);

        if (existingCategory == null)
        {
            throw new NotFoundException("Category", categoryId);
        }

        // Tenant validation
        if (existingCategory.TenantId != userTenantId)
        {
            throw new AuthorizationException("Category", "delete", "Access denied. Cannot delete category from different tenant.");
        }

        var success = await categoryRepository.DeleteCategoryAsync(categoryId);

        return !success ? throw new DatabaseOperationException("Delete", "Category", "Failed to delete category") : true;
    }
}