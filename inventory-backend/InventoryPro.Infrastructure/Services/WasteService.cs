using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class WasteService : IWasteService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public WasteService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResponseDto<WasteRecordDto>> GetWasteRecordsAsync(int orgId, int page, int pageSize, string? reason = null)
    {
        var query = _context.WasteRecords
            .Include(w => w.StockItem)
            .Where(w => w.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(reason) && Enum.TryParse<WasteReason>(reason, true, out var reasonEnum))
            query = query.Where(w => w.WasteReason == reasonEnum);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(w => w.RecordedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WasteRecordDto
            {
                Id = w.Id,
                StockItemId = w.StockItemId,
                StockItemName = w.StockItem.Name,
                Quantity = w.Quantity,
                CostPerUnit = w.CostPerUnit,
                TotalCost = w.TotalCost,
                WasteReason = w.WasteReason.ToString(),
                RecordedAt = w.RecordedAt,
                RecordedBy = w.RecordedBy,
                Notes = w.Notes
            })
            .ToListAsync();

        return new PaginatedResponseDto<WasteRecordDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ServiceResponseDto<WasteRecordDto>> CreateWasteRecordAsync(int orgId, CreateWasteRecordDto dto, string userId)
    {
        var stockItem = await _context.StockItems.FirstOrDefaultAsync(s => s.Id == dto.StockItemId && s.OrganizationId == orgId);
        if (stockItem == null)
            return ServiceResponseDto<WasteRecordDto>.Fail("Stock item not found");

        if (dto.Quantity <= 0)
            return ServiceResponseDto<WasteRecordDto>.Fail("Quantity must be greater than zero");

        if (!Enum.IsDefined(typeof(WasteReason), dto.WasteReason))
            return ServiceResponseDto<WasteRecordDto>.Fail("Invalid waste reason");

        var previousQty = stockItem.CurrentQuantity;
        var newQty = previousQty - dto.Quantity;
        if (newQty < 0)
            return ServiceResponseDto<WasteRecordDto>.Fail("Insufficient stock for this waste quantity");

        var costPerUnit = stockItem.AverageCostPrice > 0 ? stockItem.AverageCostPrice : stockItem.CostPrice;
        var totalCost = dto.Quantity * costPerUnit;
        var recordedAt = DateTime.UtcNow;

        var record = new WasteRecord
        {
            OrganizationId = orgId,
            StockItemId = dto.StockItemId,
            Quantity = dto.Quantity,
            CostPerUnit = costPerUnit,
            TotalCost = totalCost,
            WasteReason = (WasteReason)dto.WasteReason,
            RecordedAt = recordedAt,
            RecordedBy = userId,
            Notes = dto.Notes
        };
        _context.WasteRecords.Add(record);
        await _unitOfWork.SaveAsync();

        stockItem.CurrentQuantity = newQty;
        stockItem.UpdatedAt = DateTime.UtcNow;

        _context.StockMovements.Add(new StockMovement
        {
            StockItemId = stockItem.Id,
            OrganizationId = orgId,
            MovementType = MovementType.Wasted,
            Quantity = -dto.Quantity,
            PreviousQuantity = previousQty,
            NewQuantity = newQty,
            CostPerUnit = costPerUnit,
            TotalCost = totalCost,
            Reason = ((WasteReason)dto.WasteReason).ToString(),
            ReferenceType = nameof(WasteRecord),
            ReferenceId = record.Id,
            CreatedAt = recordedAt,
            CreatedBy = userId,
            Notes = dto.Notes
        });

        await _unitOfWork.SaveAsync();

        return ServiceResponseDto<WasteRecordDto>.Ok(new WasteRecordDto
        {
            Id = record.Id,
            StockItemId = record.StockItemId,
            StockItemName = stockItem.Name,
            Quantity = record.Quantity,
            CostPerUnit = record.CostPerUnit,
            TotalCost = record.TotalCost,
            WasteReason = record.WasteReason.ToString(),
            RecordedAt = record.RecordedAt,
            RecordedBy = record.RecordedBy,
            Notes = record.Notes
        });
    }

    public async Task<ServiceResponseDto<WasteSummaryDto>> GetWasteSummaryAsync(int orgId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.WasteRecords.Where(w => w.OrganizationId == orgId);

        if (startDate.HasValue)
            query = query.Where(w => w.RecordedAt >= startDate.Value);

        if (endDate.HasValue)
        {
            var end = endDate.Value.Date.AddDays(1);
            query = query.Where(w => w.RecordedAt < end);
        }

        var records = await query.ToListAsync();

        var byReason = records
            .GroupBy(w => w.WasteReason)
            .Select(g => new WasteByReasonDto
            {
                Reason = g.Key.ToString(),
                Count = g.Count(),
                TotalCost = g.Sum(w => w.TotalCost)
            })
            .ToList();

        return ServiceResponseDto<WasteSummaryDto>.Ok(new WasteSummaryDto
        {
            TotalRecords = records.Count,
            TotalCost = records.Sum(w => w.TotalCost),
            ByReason = byReason
        });
    }
}
