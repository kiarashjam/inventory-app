using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class StockCountService : IStockCountService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public StockCountService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResponseDto<StockCountDto>> GetStockCountsAsync(int orgId, int page, int pageSize, string? status = null)
    {
        var query = _context.StockCounts
            .Include(c => c.Items)
            .Where(c => c.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<StockCountStatus>(status, true, out var statusEnum))
            query = query.Where(c => c.Status == statusEnum);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CountDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new StockCountDto
            {
                Id = c.Id,
                CountDate = c.CountDate,
                Status = c.Status.ToString(),
                ItemCount = c.Items.Count,
                TotalVarianceValue = c.Items.Sum(i => i.VarianceValue),
                CreatedBy = c.CreatedBy,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponseDto<StockCountDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ServiceResponseDto<StockCountDetailDto>> GetStockCountAsync(int orgId, int countId)
    {
        var count = await _context.StockCounts
            .Include(c => c.Items)
            .ThenInclude(i => i.StockItem)
            .FirstOrDefaultAsync(c => c.Id == countId && c.OrganizationId == orgId);

        if (count == null)
            return ServiceResponseDto<StockCountDetailDto>.Fail("Stock count not found");

        var dto = MapToDetail(count);
        return ServiceResponseDto<StockCountDetailDto>.Ok(dto);
    }

    public async Task<ServiceResponseDto<StockCountDetailDto>> StartCountAsync(int orgId, CreateStockCountDto dto, string userId)
    {
        var query = _context.StockItems
            .Where(s => s.OrganizationId == orgId && s.IsActive);

        if (dto.CategoryId.HasValue)
            query = query.Where(s => s.CategoryId == dto.CategoryId.Value);

        if (dto.StorageLocationId.HasValue)
            query = query.Where(s => s.PrimaryStorageLocationId == dto.StorageLocationId.Value);

        var stockItems = await query.ToListAsync();
        if (!stockItems.Any())
            return ServiceResponseDto<StockCountDetailDto>.Fail("No stock items match the selected category or location");

        var now = DateTime.UtcNow;
        var count = new StockCount
        {
            OrganizationId = orgId,
            CountDate = now,
            Status = StockCountStatus.InProgress,
            Notes = dto.Notes,
            CreatedBy = userId,
            CreatedAt = now
        };
        _context.StockCounts.Add(count);
        await _unitOfWork.SaveAsync();

        foreach (var item in stockItems)
        {
            _context.StockCountItems.Add(new StockCountItem
            {
                StockCountId = count.Id,
                StockItemId = item.Id,
                ExpectedQuantity = item.CurrentQuantity,
                ActualQuantity = 0,
                Variance = 0,
                VarianceValue = 0,
                CountedAt = default
            });
        }
        await _unitOfWork.SaveAsync();

        var reloaded = await _context.StockCounts
            .Include(c => c.Items)
            .ThenInclude(i => i.StockItem)
            .FirstAsync(c => c.Id == count.Id);
        return ServiceResponseDto<StockCountDetailDto>.Ok(MapToDetail(reloaded));
    }

    public async Task<ServiceResponseDto<StockCountDetailDto>> SubmitCountItemsAsync(int orgId, int countId, List<StockCountItemDto> items, string userId)
    {
        var count = await _context.StockCounts
            .Include(c => c.Items)
            .ThenInclude(i => i.StockItem)
            .FirstOrDefaultAsync(c => c.Id == countId && c.OrganizationId == orgId);

        if (count == null)
            return ServiceResponseDto<StockCountDetailDto>.Fail("Stock count not found");

        if (count.Status != StockCountStatus.InProgress)
            return ServiceResponseDto<StockCountDetailDto>.Fail("Count is not in progress");

        var itemByStockItemId = count.Items.ToDictionary(i => i.StockItemId);
        var now = DateTime.UtcNow;

        foreach (var dto in items)
        {
            if (!itemByStockItemId.TryGetValue(dto.StockItemId, out var countItem))
                continue;

            countItem.ActualQuantity = dto.ActualQuantity;
            countItem.Variance = dto.ActualQuantity - countItem.ExpectedQuantity;
            countItem.VarianceValue = countItem.Variance * (countItem.StockItem?.AverageCostPrice ?? countItem.StockItem?.CostPrice ?? 0);
            countItem.Notes = dto.Notes;
            countItem.CountedBy = userId;
            countItem.CountedAt = now;
        }

        await _unitOfWork.SaveAsync();

        var reloaded = await _context.StockCounts
            .Include(c => c.Items)
            .ThenInclude(i => i.StockItem)
            .FirstAsync(c => c.Id == count.Id);
        return ServiceResponseDto<StockCountDetailDto>.Ok(MapToDetail(reloaded));
    }

    public async Task<ServiceResponseDto<StockCountDetailDto>> CompleteCountAsync(int orgId, int countId)
    {
        var count = await _context.StockCounts
            .Include(c => c.Items)
            .ThenInclude(i => i.StockItem)
            .FirstOrDefaultAsync(c => c.Id == countId && c.OrganizationId == orgId);

        if (count == null)
            return ServiceResponseDto<StockCountDetailDto>.Fail("Stock count not found");

        if (count.Status != StockCountStatus.InProgress)
            return ServiceResponseDto<StockCountDetailDto>.Fail("Count is not in progress");

        count.Status = StockCountStatus.Completed;
        count.CompletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();

        return ServiceResponseDto<StockCountDetailDto>.Ok(MapToDetail(count));
    }

    public async Task<ServiceResponseDto<StockCountDetailDto>> ApproveCountAsync(int orgId, int countId, string userId)
    {
        var count = await _context.StockCounts
            .Include(c => c.Items)
            .ThenInclude(i => i.StockItem)
            .FirstOrDefaultAsync(c => c.Id == countId && c.OrganizationId == orgId);

        if (count == null)
            return ServiceResponseDto<StockCountDetailDto>.Fail("Stock count not found");

        if (count.Status != StockCountStatus.Completed)
            return ServiceResponseDto<StockCountDetailDto>.Fail("Only completed counts can be approved");

        count.Status = StockCountStatus.Approved;
        count.ApprovedBy = userId;
        count.ApprovedAt = DateTime.UtcNow;

        foreach (var item in count.Items)
        {
            if (item.Variance == 0)
                continue;

            var stockItem = item.StockItem;
            if (stockItem == null)
                continue;

            var previousQty = stockItem.CurrentQuantity;
            var newQty = item.ActualQuantity;
            stockItem.CurrentQuantity = newQty;
            stockItem.UpdatedAt = DateTime.UtcNow;

            var costPerUnit = stockItem.AverageCostPrice > 0 ? stockItem.AverageCostPrice : stockItem.CostPrice;
            _context.StockMovements.Add(new StockMovement
            {
                StockItemId = stockItem.Id,
                OrganizationId = orgId,
                MovementType = MovementType.CountCorrection,
                Quantity = item.Variance,
                PreviousQuantity = previousQty,
                NewQuantity = newQty,
                CostPerUnit = costPerUnit,
                TotalCost = Math.Abs(item.Variance) * costPerUnit,
                Reason = "Stock count approval",
                ReferenceType = nameof(StockCount),
                ReferenceId = count.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                Notes = "Count correction"
            });
        }

        await _unitOfWork.SaveAsync();

        var reloaded = await _context.StockCounts
            .Include(c => c.Items)
            .ThenInclude(i => i.StockItem)
            .FirstAsync(c => c.Id == count.Id);
        return ServiceResponseDto<StockCountDetailDto>.Ok(MapToDetail(reloaded));
    }

    private static StockCountDetailDto MapToDetail(StockCount c)
    {
        return new StockCountDetailDto
        {
            Id = c.Id,
            CountDate = c.CountDate,
            Status = c.Status.ToString(),
            ItemCount = c.Items.Count,
            TotalVarianceValue = c.Items.Sum(i => i.VarianceValue),
            CreatedBy = c.CreatedBy,
            CreatedAt = c.CreatedAt,
            Notes = c.Notes,
            ApprovedBy = c.ApprovedBy,
            ApprovedAt = c.ApprovedAt,
            Items = c.Items.Select(i => new StockCountItemResultDto
            {
                Id = i.Id,
                StockItemId = i.StockItemId,
                StockItemName = i.StockItem?.Name ?? "",
                ExpectedQuantity = i.ExpectedQuantity,
                ActualQuantity = i.ActualQuantity,
                Variance = i.Variance,
                VarianceValue = i.VarianceValue,
                CountedBy = i.CountedBy,
                CountedAt = i.CountedAt
            }).ToList()
        };
    }
}
