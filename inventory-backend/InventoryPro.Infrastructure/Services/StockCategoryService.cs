using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class StockCategoryService : IStockCategoryService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public StockCategoryService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<StockCategoryDto>> GetCategoriesAsync(int orgId)
    {
        var categories = await _context.StockCategories
            .Include(c => c.ParentCategory)
            .Where(c => c.OrganizationId == orgId)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => new StockCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                DisplayOrder = c.DisplayOrder,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategory != null ? c.ParentCategory.Name : null,
                IsActive = c.IsActive,
                SubCategories = new List<StockCategoryDto>() // Will be populated if needed
            })
            .ToListAsync();

        // Build hierarchy
        var categoryMap = categories.ToDictionary(c => c.Id);
        var rootCategories = new List<StockCategoryDto>();

        foreach (var category in categories)
        {
            if (category.ParentCategoryId.HasValue && categoryMap.TryGetValue(category.ParentCategoryId.Value, out var parent))
            {
                if (parent.SubCategories == null)
                    parent.SubCategories = new List<StockCategoryDto>();
                parent.SubCategories.Add(category);
            }
            else
            {
                rootCategories.Add(category);
            }
        }

        return rootCategories;
    }

    public async Task<ServiceResponseDto<StockCategoryDto>> CreateCategoryAsync(int orgId, CreateStockCategoryDto dto)
    {
        // Validate parent category if provided
        if (dto.ParentCategoryId.HasValue)
        {
            var parentCategory = await _context.StockCategories
                .FirstOrDefaultAsync(c => c.Id == dto.ParentCategoryId.Value && c.OrganizationId == orgId);

            if (parentCategory == null)
                return ServiceResponseDto<StockCategoryDto>.Fail("Parent category not found");

            // Prevent circular references - ensure parent is not a child of this category
            // (This is a simple check; for deeper hierarchies, you might need recursive validation)
        }

        // Check for duplicate name at the same level
        var existingCategory = await _context.StockCategories
            .FirstOrDefaultAsync(c => c.OrganizationId == orgId &&
                                     c.Name == dto.Name &&
                                     c.ParentCategoryId == dto.ParentCategoryId);

        if (existingCategory != null)
            return ServiceResponseDto<StockCategoryDto>.Fail("Category with this name already exists at this level");

        var category = new Domain.Entities.StockCategory
        {
            OrganizationId = orgId,
            Name = dto.Name,
            DisplayOrder = dto.DisplayOrder,
            ParentCategoryId = dto.ParentCategoryId,
            IsActive = true
        };

        _context.StockCategories.Add(category);
        await _unitOfWork.SaveAsync();

        var result = await GetCategoryByIdAsync(orgId, category.Id);
        return ServiceResponseDto<StockCategoryDto>.Ok(result);
    }

    public async Task<ServiceResponseDto<StockCategoryDto>> UpdateCategoryAsync(int orgId, int categoryId, UpdateStockCategoryDto dto)
    {
        var category = await _context.StockCategories
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.OrganizationId == orgId);

        if (category == null)
            return ServiceResponseDto<StockCategoryDto>.Fail("Category not found");

        // Validate parent category if provided
        if (dto.ParentCategoryId.HasValue)
        {
            if (dto.ParentCategoryId.Value == categoryId)
                return ServiceResponseDto<StockCategoryDto>.Fail("Category cannot be its own parent");

            var parentCategory = await _context.StockCategories
                .FirstOrDefaultAsync(c => c.Id == dto.ParentCategoryId.Value && c.OrganizationId == orgId);

            if (parentCategory == null)
                return ServiceResponseDto<StockCategoryDto>.Fail("Parent category not found");

            // Prevent circular references - check if the parent is a descendant of this category
            var isDescendant = await IsDescendantCategoryAsync(orgId, dto.ParentCategoryId.Value, categoryId);
            if (isDescendant)
                return ServiceResponseDto<StockCategoryDto>.Fail("Cannot set parent category: would create circular reference");
        }

        // Check for duplicate name at the same level (excluding current category)
        var existingCategory = await _context.StockCategories
            .FirstOrDefaultAsync(c => c.OrganizationId == orgId &&
                                     c.Name == dto.Name &&
                                     c.ParentCategoryId == dto.ParentCategoryId &&
                                     c.Id != categoryId);

        if (existingCategory != null)
            return ServiceResponseDto<StockCategoryDto>.Fail("Category with this name already exists at this level");

        category.Name = dto.Name;
        category.DisplayOrder = dto.DisplayOrder;
        category.ParentCategoryId = dto.ParentCategoryId;
        category.IsActive = dto.IsActive;

        await _unitOfWork.SaveAsync();

        var result = await GetCategoryByIdAsync(orgId, category.Id);
        return ServiceResponseDto<StockCategoryDto>.Ok(result);
    }

    public async Task<ServiceResponseDto> DeleteCategoryAsync(int orgId, int categoryId)
    {
        var category = await _context.StockCategories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.OrganizationId == orgId);

        if (category == null)
            return ServiceResponseDto.Fail("Category not found");

        // Check if category has subcategories
        if (category.SubCategories.Any(c => c.IsActive))
            return ServiceResponseDto.Fail("Cannot delete category with active subcategories");

        // Check if category is used by stock items
        var hasStockItems = await _context.StockItems
            .AnyAsync(s => s.CategoryId == categoryId && s.OrganizationId == orgId && s.IsActive);

        if (hasStockItems)
            return ServiceResponseDto.Fail("Cannot delete category that is assigned to active stock items");

        // Soft delete
        category.IsActive = false;

        await _unitOfWork.SaveAsync();
        return ServiceResponseDto.Ok("Category deleted successfully");
    }

    private async Task<StockCategoryDto> GetCategoryByIdAsync(int orgId, int categoryId)
    {
        var category = await _context.StockCategories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.OrganizationId == orgId);

        if (category == null)
            return null!;

        return new StockCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            DisplayOrder = category.DisplayOrder,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            IsActive = category.IsActive,
            SubCategories = new List<StockCategoryDto>()
        };
    }

    private async Task<bool> IsDescendantCategoryAsync(int orgId, int potentialParentId, int categoryId)
    {
        var current = await _context.StockCategories
            .FirstOrDefaultAsync(c => c.Id == potentialParentId && c.OrganizationId == orgId);

        if (current == null)
            return false;

        // Traverse up the parent chain
        while (current.ParentCategoryId.HasValue)
        {
            if (current.ParentCategoryId.Value == categoryId)
                return true;

            current = await _context.StockCategories
                .FirstOrDefaultAsync(c => c.Id == current.ParentCategoryId.Value && c.OrganizationId == orgId);

            if (current == null)
                break;
        }

        return false;
    }
}
