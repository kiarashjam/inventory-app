using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class StockItemService : IStockItemService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public StockItemService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResponseDto<StockItemDto>> GetStockItemsAsync(
        int orgId,
        int page = 1,
        int pageSize = 20,
        string? search = null,
        int? categoryId = null,
        bool? isActive = null,
        bool? lowStockOnly = null)
    {
        var query = _context.StockItems
            .Include(s => s.Category)
            .Include(s => s.PrimarySupplier)
            .Include(s => s.PrimaryStorageLocation)
            .Where(s => s.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.Name.Contains(search) ||
                (s.SKU != null && s.SKU.Contains(search)) ||
                (s.Description != null && s.Description.Contains(search)) ||
                (s.Barcode != null && s.Barcode.Contains(search)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(s => s.CategoryId == categoryId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        if (lowStockOnly == true)
        {
            query = query.Where(s => s.CurrentQuantity <= s.MinimumThreshold);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StockItemDto
            {
                Id = s.Id,
                Name = s.Name,
                SKU = s.SKU,
                Description = s.Description,
                CategoryId = s.CategoryId,
                CategoryName = s.Category.Name,
                BaseUnitOfMeasurement = s.BaseUnitOfMeasurement,
                CurrentQuantity = s.CurrentQuantity,
                MinimumThreshold = s.MinimumThreshold,
                ParLevel = s.ParLevel,
                MaximumCapacity = s.MaximumCapacity,
                CostPrice = s.CostPrice,
                AverageCostPrice = s.AverageCostPrice,
                PrimarySupplierId = s.PrimarySupplierId,
                PrimarySupplierName = s.PrimarySupplier != null ? s.PrimarySupplier.Name : null,
                Barcode = s.Barcode,
                PrimaryStorageLocationId = s.PrimaryStorageLocationId,
                PrimaryStorageLocationName = s.PrimaryStorageLocation != null ? s.PrimaryStorageLocation.Name : null,
                IsActive = s.IsActive,
                IsPerishable = s.IsPerishable,
                DefaultExpirationDays = s.DefaultExpirationDays,
                Notes = s.Notes,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();

        return new PaginatedResponseDto<StockItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ServiceResponseDto<StockItemDto>> GetStockItemAsync(int orgId, int stockItemId)
    {
        var stockItem = await _context.StockItems
            .Include(s => s.Category)
            .Include(s => s.PrimarySupplier)
            .Include(s => s.PrimaryStorageLocation)
            .FirstOrDefaultAsync(s => s.Id == stockItemId && s.OrganizationId == orgId);

        if (stockItem == null)
            return ServiceResponseDto<StockItemDto>.Fail("Stock item not found");

        var dto = MapToDto(stockItem);
        return ServiceResponseDto<StockItemDto>.Ok(dto);
    }

    public async Task<ServiceResponseDto<StockItemDto>> CreateStockItemAsync(int orgId, CreateStockItemDto dto, string userId)
    {
        // Validate category exists and belongs to organization
        var category = await _context.StockCategories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.OrganizationId == orgId);

        if (category == null)
            return ServiceResponseDto<StockItemDto>.Fail("Category not found");

        // Validate SKU uniqueness if provided
        if (!string.IsNullOrWhiteSpace(dto.SKU))
        {
            var existingSku = await _context.StockItems
                .AnyAsync(s => s.OrganizationId == orgId && s.SKU == dto.SKU);

            if (existingSku)
                return ServiceResponseDto<StockItemDto>.Fail("SKU already exists for this organization");
        }

        // Validate supplier if provided
        if (dto.PrimarySupplierId.HasValue)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == dto.PrimarySupplierId.Value && s.OrganizationId == orgId);

            if (supplier == null)
                return ServiceResponseDto<StockItemDto>.Fail("Supplier not found");
        }

        // Validate storage location if provided
        if (dto.PrimaryStorageLocationId.HasValue)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == dto.PrimaryStorageLocationId.Value && l.OrganizationId == orgId);

            if (location == null)
                return ServiceResponseDto<StockItemDto>.Fail("Storage location not found");
        }

        var stockItem = new StockItem
        {
            OrganizationId = orgId,
            Name = dto.Name,
            SKU = dto.SKU,
            Description = dto.Description,
            CategoryId = dto.CategoryId,
            BaseUnitOfMeasurement = dto.BaseUnitOfMeasurement,
            CurrentQuantity = dto.CurrentQuantity,
            MinimumThreshold = dto.MinimumThreshold,
            ParLevel = dto.ParLevel,
            MaximumCapacity = dto.MaximumCapacity,
            CostPrice = dto.CostPrice,
            AverageCostPrice = dto.CostPrice, // Initialize with cost price
            PrimarySupplierId = dto.PrimarySupplierId,
            Barcode = dto.Barcode,
            PrimaryStorageLocationId = dto.PrimaryStorageLocationId,
            IsActive = true,
            IsPerishable = dto.IsPerishable,
            DefaultExpirationDays = dto.DefaultExpirationDays,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.StockItems.Add(stockItem);
        await _unitOfWork.SaveAsync();

        // Check if stock is low and create alert
        if (stockItem.CurrentQuantity <= stockItem.MinimumThreshold)
        {
            await CreateLowStockAlertAsync(orgId, stockItem.Id, stockItem.Name, stockItem.CurrentQuantity, stockItem.MinimumThreshold);
        }

        var result = await GetStockItemAsync(orgId, stockItem.Id);
        return result;
    }

    public async Task<ServiceResponseDto<StockItemDto>> UpdateStockItemAsync(int orgId, int stockItemId, UpdateStockItemDto dto)
    {
        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.Id == stockItemId && s.OrganizationId == orgId);

        if (stockItem == null)
            return ServiceResponseDto<StockItemDto>.Fail("Stock item not found");

        // Validate category exists and belongs to organization
        var category = await _context.StockCategories
            .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && c.OrganizationId == orgId);

        if (category == null)
            return ServiceResponseDto<StockItemDto>.Fail("Category not found");

        // Validate SKU uniqueness if provided and changed
        if (!string.IsNullOrWhiteSpace(dto.SKU) && dto.SKU != stockItem.SKU)
        {
            var existingSku = await _context.StockItems
                .AnyAsync(s => s.OrganizationId == orgId && s.SKU == dto.SKU && s.Id != stockItemId);

            if (existingSku)
                return ServiceResponseDto<StockItemDto>.Fail("SKU already exists for this organization");
        }

        // Validate supplier if provided
        if (dto.PrimarySupplierId.HasValue)
        {
            var supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.Id == dto.PrimarySupplierId.Value && s.OrganizationId == orgId);

            if (supplier == null)
                return ServiceResponseDto<StockItemDto>.Fail("Supplier not found");
        }

        // Validate storage location if provided
        if (dto.PrimaryStorageLocationId.HasValue)
        {
            var location = await _context.StorageLocations
                .FirstOrDefaultAsync(l => l.Id == dto.PrimaryStorageLocationId.Value && l.OrganizationId == orgId);

            if (location == null)
                return ServiceResponseDto<StockItemDto>.Fail("Storage location not found");
        }

        var previousQuantity = stockItem.CurrentQuantity;

        stockItem.Name = dto.Name;
        stockItem.SKU = dto.SKU;
        stockItem.Description = dto.Description;
        stockItem.CategoryId = dto.CategoryId;
        stockItem.BaseUnitOfMeasurement = dto.BaseUnitOfMeasurement;
        stockItem.MinimumThreshold = dto.MinimumThreshold;
        stockItem.ParLevel = dto.ParLevel;
        stockItem.MaximumCapacity = dto.MaximumCapacity;
        stockItem.CostPrice = dto.CostPrice;
        stockItem.PrimarySupplierId = dto.PrimarySupplierId;
        stockItem.Barcode = dto.Barcode;
        stockItem.PrimaryStorageLocationId = dto.PrimaryStorageLocationId;
        stockItem.IsActive = dto.IsActive;
        stockItem.IsPerishable = dto.IsPerishable;
        stockItem.DefaultExpirationDays = dto.DefaultExpirationDays;
        stockItem.Notes = dto.Notes;
        stockItem.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();

        // Check if stock status changed and create/update alerts
        if (stockItem.CurrentQuantity <= stockItem.MinimumThreshold && previousQuantity > stockItem.MinimumThreshold)
        {
            await CreateLowStockAlertAsync(orgId, stockItem.Id, stockItem.Name, stockItem.CurrentQuantity, stockItem.MinimumThreshold);
        }

        var result = await GetStockItemAsync(orgId, stockItem.Id);
        return result;
    }

    public async Task<ServiceResponseDto> DeleteStockItemAsync(int orgId, int stockItemId)
    {
        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.Id == stockItemId && s.OrganizationId == orgId);

        if (stockItem == null)
            return ServiceResponseDto.Fail("Stock item not found");

        // Soft delete
        stockItem.IsActive = false;
        stockItem.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();
        return ServiceResponseDto.Ok("Stock item deleted successfully");
    }

    public async Task<ServiceResponseDto<StockMovementDto>> AdjustStockAsync(int orgId, int stockItemId, StockAdjustmentDto dto, string userId)
    {
        var stockItem = await _context.StockItems
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == stockItemId && s.OrganizationId == orgId);

        if (stockItem == null)
            return ServiceResponseDto<StockMovementDto>.Fail("Stock item not found");

        if (dto.Quantity == 0)
            return ServiceResponseDto<StockMovementDto>.Fail("Adjustment quantity cannot be zero");

        var previousQuantity = stockItem.CurrentQuantity;
        var newQuantity = previousQuantity + dto.Quantity;

        // Allow negative quantities for tracking purposes, but warn
        if (newQuantity < 0)
        {
            // Optionally: return error or allow negative
            // For now, we'll allow it but could add validation
        }

        // Create stock movement record
        var movement = new StockMovement
        {
            StockItemId = stockItemId,
            OrganizationId = orgId,
            MovementType = MovementType.Adjusted,
            Quantity = dto.Quantity,
            PreviousQuantity = previousQuantity,
            NewQuantity = newQuantity,
            CostPerUnit = stockItem.CostPrice,
            TotalCost = dto.Quantity * stockItem.CostPrice,
            Reason = dto.Reason,
            BatchNumber = dto.BatchNumber,
            ExpirationDate = dto.ExpirationDate,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            Notes = dto.Notes
        };

        _context.StockMovements.Add(movement);

        // Update stock item quantity
        stockItem.CurrentQuantity = newQuantity;
        stockItem.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();

        // Check if stock is low and create alert
        if (newQuantity <= stockItem.MinimumThreshold && previousQuantity > stockItem.MinimumThreshold)
        {
            await CreateLowStockAlertAsync(orgId, stockItem.Id, stockItem.Name, newQuantity, stockItem.MinimumThreshold);
        }

        // Map movement to DTO
        var movementDto = new StockMovementDto
        {
            Id = movement.Id,
            StockItemId = movement.StockItemId,
            StockItemName = stockItem.Name,
            MovementType = movement.MovementType,
            Quantity = movement.Quantity,
            PreviousQuantity = movement.PreviousQuantity,
            NewQuantity = movement.NewQuantity,
            CostPerUnit = movement.CostPerUnit,
            Reason = movement.Reason,
            ReferenceType = movement.ReferenceType,
            ReferenceId = movement.ReferenceId,
            CreatedAt = movement.CreatedAt,
            CreatedBy = movement.CreatedBy,
            Notes = movement.Notes
        };

        return ServiceResponseDto<StockMovementDto>.Ok(movementDto);
    }

    public async Task<PaginatedResponseDto<StockMovementDto>> GetStockMovementsAsync(int orgId, int stockItemId, int page = 1, int pageSize = 20)
    {
        var query = _context.StockMovements
            .Include(m => m.StockItem)
            .Where(m => m.OrganizationId == orgId && m.StockItemId == stockItemId);

        var totalCount = await query.CountAsync();

        var movements = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new StockMovementDto
            {
                Id = m.Id,
                StockItemId = m.StockItemId,
                StockItemName = m.StockItem.Name,
                MovementType = m.MovementType,
                Quantity = m.Quantity,
                PreviousQuantity = m.PreviousQuantity,
                NewQuantity = m.NewQuantity,
                CostPerUnit = m.CostPerUnit,
                Reason = m.Reason,
                ReferenceType = m.ReferenceType,
                ReferenceId = m.ReferenceId,
                CreatedAt = m.CreatedAt,
                CreatedBy = m.CreatedBy,
                Notes = m.Notes
            })
            .ToListAsync();

        return new PaginatedResponseDto<StockMovementDto>
        {
            Items = movements,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<StockItemDto>> GetLowStockItemsAsync(int orgId)
    {
        var items = await _context.StockItems
            .Include(s => s.Category)
            .Include(s => s.PrimarySupplier)
            .Include(s => s.PrimaryStorageLocation)
            .Where(s => s.OrganizationId == orgId &&
                        s.IsActive &&
                        s.CurrentQuantity <= s.MinimumThreshold)
            .OrderBy(s => s.CurrentQuantity)
            .Select(s => new StockItemDto
            {
                Id = s.Id,
                Name = s.Name,
                SKU = s.SKU,
                Description = s.Description,
                CategoryId = s.CategoryId,
                CategoryName = s.Category.Name,
                BaseUnitOfMeasurement = s.BaseUnitOfMeasurement,
                CurrentQuantity = s.CurrentQuantity,
                MinimumThreshold = s.MinimumThreshold,
                ParLevel = s.ParLevel,
                MaximumCapacity = s.MaximumCapacity,
                CostPrice = s.CostPrice,
                AverageCostPrice = s.AverageCostPrice,
                PrimarySupplierId = s.PrimarySupplierId,
                PrimarySupplierName = s.PrimarySupplier != null ? s.PrimarySupplier.Name : null,
                Barcode = s.Barcode,
                PrimaryStorageLocationId = s.PrimaryStorageLocationId,
                PrimaryStorageLocationName = s.PrimaryStorageLocation != null ? s.PrimaryStorageLocation.Name : null,
                IsActive = s.IsActive,
                IsPerishable = s.IsPerishable,
                DefaultExpirationDays = s.DefaultExpirationDays,
                Notes = s.Notes,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();

        return items;
    }

    private StockItemDto MapToDto(StockItem stockItem)
    {
        return new StockItemDto
        {
            Id = stockItem.Id,
            Name = stockItem.Name,
            SKU = stockItem.SKU,
            Description = stockItem.Description,
            CategoryId = stockItem.CategoryId,
            CategoryName = stockItem.Category?.Name ?? string.Empty,
            BaseUnitOfMeasurement = stockItem.BaseUnitOfMeasurement,
            CurrentQuantity = stockItem.CurrentQuantity,
            MinimumThreshold = stockItem.MinimumThreshold,
            ParLevel = stockItem.ParLevel,
            MaximumCapacity = stockItem.MaximumCapacity,
            CostPrice = stockItem.CostPrice,
            AverageCostPrice = stockItem.AverageCostPrice,
            PrimarySupplierId = stockItem.PrimarySupplierId,
            PrimarySupplierName = stockItem.PrimarySupplier?.Name,
            Barcode = stockItem.Barcode,
            PrimaryStorageLocationId = stockItem.PrimaryStorageLocationId,
            PrimaryStorageLocationName = stockItem.PrimaryStorageLocation?.Name,
            IsActive = stockItem.IsActive,
            IsPerishable = stockItem.IsPerishable,
            DefaultExpirationDays = stockItem.DefaultExpirationDays,
            Notes = stockItem.Notes,
            CreatedAt = stockItem.CreatedAt,
            UpdatedAt = stockItem.UpdatedAt
        };
    }

    private async Task CreateLowStockAlertAsync(int orgId, int stockItemId, string stockItemName, decimal currentQuantity, decimal minimumThreshold)
    {
        // Check if alert already exists and is not dismissed
        var existingAlert = await _context.StockAlerts
            .FirstOrDefaultAsync(a => a.OrganizationId == orgId &&
                                     a.StockItemId == stockItemId &&
                                     a.AlertType == AlertType.LowStock &&
                                     !a.IsDismissed);

        if (existingAlert != null)
        {
            // Update existing alert
            existingAlert.Message = $"{stockItemName} is low on stock. Current: {currentQuantity}, Minimum: {minimumThreshold}";
            existingAlert.CreatedAt = DateTime.UtcNow;
            existingAlert.IsRead = false;
        }
        else
        {
            // Create new alert
            var alert = new StockAlert
            {
                OrganizationId = orgId,
                StockItemId = stockItemId,
                AlertType = AlertType.LowStock,
                Message = $"{stockItemName} is low on stock. Current: {currentQuantity}, Minimum: {minimumThreshold}",
                IsRead = false,
                IsDismissed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.StockAlerts.Add(alert);
        }

        await _unitOfWork.SaveAsync();
    }
}
