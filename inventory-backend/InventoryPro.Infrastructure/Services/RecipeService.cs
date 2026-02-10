using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class RecipeService : IRecipeService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public RecipeService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponseDto<RecipeDto>> GetRecipeAsync(int orgId, int menuItemId)
    {
        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == menuItemId && m.OrganizationId == orgId);
        if (menuItem == null)
            return ServiceResponseDto<RecipeDto>.Fail("Menu item not found");

        var mappings = await _context.MenuItemStockMappings
            .Include(m => m.StockItem)
            .Where(m => m.MenuItemId == menuItemId)
            .ToListAsync();

        var ingredients = new List<RecipeIngredientDto>();
        decimal totalCost = 0;
        foreach (var map in mappings)
        {
            if (map.StockItem == null) continue;
            var costPerUnit = map.StockItem.AverageCostPrice;
            var effectiveQty = map.QuantityRequired * (1 + map.WastePercentage / 100m);
            var subTotal = effectiveQty * costPerUnit;
            totalCost += subTotal;
            ingredients.Add(new RecipeIngredientDto
            {
                MappingId = map.Id,
                StockItemId = map.StockItemId,
                StockItemName = map.StockItem.Name,
                QuantityRequired = map.QuantityRequired,
                UnitOfMeasurement = map.UnitOfMeasurement,
                WastePercentage = map.WastePercentage,
                CostPerUnit = costPerUnit,
                SubTotal = subTotal,
                Notes = map.Notes
            });
        }

        var foodCostPercent = menuItem.SellingPrice > 0 ? (totalCost / menuItem.SellingPrice) * 100 : 0;

        return ServiceResponseDto<RecipeDto>.Ok(new RecipeDto
        {
            MenuItemId = menuItem.Id,
            MenuItemName = menuItem.Name,
            SellingPrice = menuItem.SellingPrice,
            TotalCost = totalCost,
            FoodCostPercent = foodCostPercent,
            Ingredients = ingredients
        });
    }

    public async Task<ServiceResponseDto> SetRecipeMappingsAsync(int orgId, int menuItemId, List<RecipeMappingDto> mappings)
    {
        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == menuItemId && m.OrganizationId == orgId);
        if (menuItem == null)
            return ServiceResponseDto.Fail("Menu item not found");

        var existing = await _context.MenuItemStockMappings.Where(m => m.MenuItemId == menuItemId).ToListAsync();
        _context.MenuItemStockMappings.RemoveRange(existing);

        foreach (var dto in mappings ?? new List<RecipeMappingDto>())
        {
            var stockItem = await _context.StockItems
                .FirstOrDefaultAsync(s => s.Id == dto.StockItemId && s.OrganizationId == orgId);
            if (stockItem == null)
                return ServiceResponseDto.Fail($"Stock item {dto.StockItemId} not found");

            _context.MenuItemStockMappings.Add(new MenuItemStockMapping
            {
                MenuItemId = menuItemId,
                StockItemId = dto.StockItemId,
                QuantityRequired = dto.QuantityRequired,
                UnitOfMeasurement = dto.UnitOfMeasurement,
                WastePercentage = dto.WastePercentage,
                Notes = dto.Notes
            });
        }

        await _unitOfWork.SaveAsync();
        await RecalculateMenuItemCosts(menuItemId);
        return ServiceResponseDto.Ok();
    }

    public async Task<ServiceResponseDto> DeleteMappingAsync(int orgId, int mappingId)
    {
        var mapping = await _context.MenuItemStockMappings
            .Include(m => m.MenuItem)
            .FirstOrDefaultAsync(m => m.Id == mappingId && m.MenuItem != null && m.MenuItem.OrganizationId == orgId);
        if (mapping == null)
            return ServiceResponseDto.Fail("Mapping not found");

        var menuItemId = mapping.MenuItemId;
        _context.MenuItemStockMappings.Remove(mapping);
        await _unitOfWork.SaveAsync();
        await RecalculateMenuItemCosts(menuItemId);
        return ServiceResponseDto.Ok();
    }

    public async Task<ServiceResponseDto<List<MenuItemDto>>> GetMenuItemsUsingStockItemAsync(int orgId, int stockItemId)
    {
        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.Id == stockItemId && s.OrganizationId == orgId);
        if (stockItem == null)
            return ServiceResponseDto<List<MenuItemDto>>.Fail("Stock item not found");

        var menuItemIds = await _context.MenuItemStockMappings
            .Where(m => m.StockItemId == stockItemId)
            .Select(m => m.MenuItemId)
            .Distinct()
            .ToListAsync();

        var menuItems = await _context.MenuItems
            .Where(m => m.OrganizationId == orgId && menuItemIds.Contains(m.Id))
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

        return ServiceResponseDto<List<MenuItemDto>>.Ok(menuItems);
    }

    private async Task RecalculateMenuItemCosts(int menuItemId)
    {
        var menuItem = await _context.MenuItems.FindAsync(menuItemId);
        if (menuItem == null) return;

        var mappings = await _context.MenuItemStockMappings
            .Include(m => m.StockItem)
            .Where(m => m.MenuItemId == menuItemId)
            .ToListAsync();

        decimal totalCost = 0;
        foreach (var map in mappings)
        {
            if (map.StockItem == null) continue;
            var effectiveQty = map.QuantityRequired * (1 + map.WastePercentage / 100m);
            totalCost += effectiveQty * map.StockItem.AverageCostPrice;
        }

        menuItem.TheoreticalFoodCost = totalCost;
        menuItem.FoodCostPercent = menuItem.SellingPrice > 0 ? (totalCost / menuItem.SellingPrice) * 100 : null;
        menuItem.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();
    }
}
