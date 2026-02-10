using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class MenuItemService : IMenuItemService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public MenuItemService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResponseDto<MenuItemDto>> GetMenuItemsAsync(int orgId, int page = 1, int pageSize = 20, string? search = null, string? category = null, bool? isActive = null)
    {
        var query = _context.MenuItems.Where(m => m.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(m => m.Name.Contains(search) || (m.ExternalId != null && m.ExternalId.Contains(search)));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(m => m.Category != null && m.Category == category);

        if (isActive.HasValue)
            query = query.Where(m => m.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(m => m.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MenuItemDto
            {
                Id = m.Id,
                Name = m.Name,
                Category = m.Category,
                SellingPrice = m.SellingPrice,
                ExternalId = m.ExternalId,
                TheoreticalFoodCost = m.TheoreticalFoodCost,
                FoodCostPercent = m.FoodCostPercent,
                IsActive = m.IsActive,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponseDto<MenuItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ServiceResponseDto<MenuItemDto>> GetMenuItemAsync(int orgId, int menuItemId)
    {
        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == menuItemId && m.OrganizationId == orgId);

        if (menuItem == null)
            return ServiceResponseDto<MenuItemDto>.Fail("Menu item not found");

        return ServiceResponseDto<MenuItemDto>.Ok(MapToDto(menuItem));
    }

    public async Task<ServiceResponseDto<MenuItemDto>> CreateMenuItemAsync(int orgId, CreateMenuItemDto dto)
    {
        var menuItem = new MenuItem
        {
            OrganizationId = orgId,
            Name = dto.Name,
            Category = dto.Category,
            SellingPrice = dto.SellingPrice,
            ExternalId = dto.ExternalId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MenuItems.Add(menuItem);
        await _unitOfWork.SaveAsync();

        return ServiceResponseDto<MenuItemDto>.Ok(MapToDto(menuItem));
    }

    public async Task<ServiceResponseDto<MenuItemDto>> UpdateMenuItemAsync(int orgId, int menuItemId, UpdateMenuItemDto dto)
    {
        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == menuItemId && m.OrganizationId == orgId);

        if (menuItem == null)
            return ServiceResponseDto<MenuItemDto>.Fail("Menu item not found");

        menuItem.Name = dto.Name;
        menuItem.Category = dto.Category;
        menuItem.SellingPrice = dto.SellingPrice;
        menuItem.ExternalId = dto.ExternalId;
        if (dto.IsActive.HasValue)
            menuItem.IsActive = dto.IsActive.Value;
        menuItem.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();
        return ServiceResponseDto<MenuItemDto>.Ok(MapToDto(menuItem));
    }

    public async Task<ServiceResponseDto> DeleteMenuItemAsync(int orgId, int menuItemId)
    {
        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == menuItemId && m.OrganizationId == orgId);

        if (menuItem == null)
            return ServiceResponseDto.Fail("Menu item not found");

        menuItem.IsActive = false;
        menuItem.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();
        return ServiceResponseDto.Ok();
    }

    private static MenuItemDto MapToDto(MenuItem m)
    {
        return new MenuItemDto
        {
            Id = m.Id,
            Name = m.Name,
            Category = m.Category,
            SellingPrice = m.SellingPrice,
            ExternalId = m.ExternalId,
            TheoreticalFoodCost = m.TheoreticalFoodCost,
            FoodCostPercent = m.FoodCostPercent,
            IsActive = m.IsActive,
            CreatedAt = m.CreatedAt
        };
    }
}
