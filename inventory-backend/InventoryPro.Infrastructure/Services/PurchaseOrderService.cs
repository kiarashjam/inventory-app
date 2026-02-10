using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseOrderService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<PaginatedResponseDto<PurchaseOrderDto>> GetPurchaseOrdersAsync(int orgId, int page, int pageSize, string? status = null, int? supplierId = null)
    {
        var query = _context.PurchaseOrders
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .Where(p => p.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PurchaseOrderStatus>(status, true, out var statusEnum))
            query = query.Where(p => p.Status == statusEnum);

        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PurchaseOrderDto
            {
                Id = p.Id,
                OrderNumber = p.OrderNumber,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier.Name,
                Status = p.Status,
                OrderDate = p.OrderDate,
                ExpectedDeliveryDate = p.ExpectedDeliveryDate,
                TotalAmount = p.TotalAmount,
                ItemCount = p.Items.Count,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        return new PaginatedResponseDto<PurchaseOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ServiceResponseDto<PurchaseOrderDetailDto>> GetPurchaseOrderAsync(int orgId, int orderId)
    {
        var order = await _context.PurchaseOrders
            .Include(p => p.Supplier)
            .Include(p => p.Items)
            .ThenInclude(i => i.StockItem)
            .FirstOrDefaultAsync(p => p.Id == orderId && p.OrganizationId == orgId);

        if (order == null)
            return ServiceResponseDto<PurchaseOrderDetailDto>.Fail("Purchase order not found");

        var dto = new PurchaseOrderDetailDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            SupplierId = order.SupplierId,
            SupplierName = order.Supplier.Name,
            Status = order.Status,
            OrderDate = order.OrderDate,
            ExpectedDeliveryDate = order.ExpectedDeliveryDate,
            TotalAmount = order.TotalAmount,
            ItemCount = order.Items.Count,
            CreatedAt = order.CreatedAt,
            Notes = order.Notes,
            CreatedBy = order.CreatedBy,
            Items = order.Items.Select(i => new PurchaseOrderItemDto
            {
                Id = i.Id,
                StockItemId = i.StockItemId,
                StockItemName = i.StockItem.Name,
                OrderedQuantity = i.OrderedQuantity,
                ReceivedQuantity = i.ReceivedQuantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };

        return ServiceResponseDto<PurchaseOrderDetailDto>.Ok(dto);
    }

    public async Task<ServiceResponseDto<PurchaseOrderDto>> CreatePurchaseOrderAsync(int orgId, CreatePurchaseOrderDto dto, string userId)
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == dto.SupplierId && s.OrganizationId == orgId);
        if (supplier == null)
            return ServiceResponseDto<PurchaseOrderDto>.Fail("Supplier not found");

        if (dto.Items == null || !dto.Items.Any())
            return ServiceResponseDto<PurchaseOrderDto>.Fail("At least one item is required");

        var stockItemIds = dto.Items.Select(x => x.StockItemId).Distinct().ToList();
        var validItems = await _context.StockItems
            .Where(s => s.OrganizationId == orgId && stockItemIds.Contains(s.Id))
            .Select(s => s.Id)
            .ToListAsync();
        if (validItems.Count != stockItemIds.Count)
            return ServiceResponseDto<PurchaseOrderDto>.Fail("One or more stock items not found");

        var orderNumber = await GenerateOrderNumberAsync(orgId);
        var now = DateTime.UtcNow;

        decimal subTotal = 0;
        var orderItems = new List<PurchaseOrderItem>();
        foreach (var item in dto.Items)
        {
            var totalPrice = item.OrderedQuantity * item.UnitPrice;
            subTotal += totalPrice;
            orderItems.Add(new PurchaseOrderItem
            {
                StockItemId = item.StockItemId,
                OrderedQuantity = item.OrderedQuantity,
                ReceivedQuantity = 0,
                UnitPrice = item.UnitPrice,
                TotalPrice = totalPrice
            });
        }

        var order = new PurchaseOrder
        {
            OrganizationId = orgId,
            SupplierId = dto.SupplierId,
            OrderNumber = orderNumber,
            Status = PurchaseOrderStatus.Draft,
            OrderDate = now,
            ExpectedDeliveryDate = dto.ExpectedDeliveryDate,
            SubTotal = subTotal,
            TaxAmount = 0,
            ShippingCost = 0,
            TotalAmount = subTotal,
            Notes = dto.Notes,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = userId
        };

        _context.PurchaseOrders.Add(order);
        await _unitOfWork.SaveAsync();

        foreach (var oi in orderItems)
        {
            oi.PurchaseOrderId = order.Id;
            _context.PurchaseOrderItems.Add(oi);
        }
        await _unitOfWork.SaveAsync();

        return ServiceResponseDto<PurchaseOrderDto>.Ok(new PurchaseOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            SupplierId = order.SupplierId,
            SupplierName = supplier.Name,
            Status = order.Status,
            OrderDate = order.OrderDate,
            ExpectedDeliveryDate = order.ExpectedDeliveryDate,
            TotalAmount = order.TotalAmount,
            ItemCount = orderItems.Count,
            CreatedAt = order.CreatedAt
        });
    }

    public async Task<ServiceResponseDto<PurchaseOrderDto>> UpdatePurchaseOrderAsync(int orgId, int orderId, UpdatePurchaseOrderDto dto)
    {
        var order = await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.Id == orderId && p.OrganizationId == orgId);

        if (order == null)
            return ServiceResponseDto<PurchaseOrderDto>.Fail("Purchase order not found");

        if (order.Status != PurchaseOrderStatus.Draft)
            return ServiceResponseDto<PurchaseOrderDto>.Fail("Only draft orders can be updated");

        if (dto.Items != null && dto.Items.Any())
        {
            var stockItemIds = dto.Items.Select(x => x.StockItemId).Distinct().ToList();
            var validItems = await _context.StockItems
                .Where(s => s.OrganizationId == orgId && stockItemIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();
            if (validItems.Count != stockItemIds.Count)
                return ServiceResponseDto<PurchaseOrderDto>.Fail("One or more stock items not found");

            _context.PurchaseOrderItems.RemoveRange(order.Items);
            decimal subTotal = 0;
            foreach (var item in dto.Items)
            {
                var totalPrice = item.OrderedQuantity * item.UnitPrice;
                subTotal += totalPrice;
                _context.PurchaseOrderItems.Add(new PurchaseOrderItem
                {
                    PurchaseOrderId = order.Id,
                    StockItemId = item.StockItemId,
                    OrderedQuantity = item.OrderedQuantity,
                    ReceivedQuantity = 0,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = totalPrice
                });
            }
            order.SubTotal = subTotal;
            order.TotalAmount = subTotal;
        }

        order.ExpectedDeliveryDate = dto.ExpectedDeliveryDate ?? order.ExpectedDeliveryDate;
        order.Notes = dto.Notes ?? order.Notes;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();

        var detail = await GetPurchaseOrderAsync(orgId, order.Id);
        return detail.Success ? ServiceResponseDto<PurchaseOrderDto>.Ok(MapToSummary(detail.Data!)) : ServiceResponseDto<PurchaseOrderDto>.Fail(detail.Message);
    }

    public async Task<ServiceResponseDto<PurchaseOrderDto>> SubmitPurchaseOrderAsync(int orgId, int orderId)
    {
        var order = await _context.PurchaseOrders.FirstOrDefaultAsync(p => p.Id == orderId && p.OrganizationId == orgId);
        if (order == null)
            return ServiceResponseDto<PurchaseOrderDto>.Fail("Purchase order not found");

        if (order.Status != PurchaseOrderStatus.Draft)
            return ServiceResponseDto<PurchaseOrderDto>.Fail("Only draft orders can be submitted");

        order.Status = PurchaseOrderStatus.Submitted;
        order.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();

        var detail = await GetPurchaseOrderAsync(orgId, order.Id);
        return detail.Success ? ServiceResponseDto<PurchaseOrderDto>.Ok(MapToSummary(detail.Data!)) : ServiceResponseDto<PurchaseOrderDto>.Fail(detail.Message);
    }

    public async Task<ServiceResponseDto<PurchaseOrderDto>> CancelPurchaseOrderAsync(int orgId, int orderId, string? reason)
    {
        var order = await _context.PurchaseOrders.Include(p => p.Items).FirstOrDefaultAsync(p => p.Id == orderId && p.OrganizationId == orgId);
        if (order == null)
            return ServiceResponseDto<PurchaseOrderDto>.Fail("Purchase order not found");

        var fullyReceived = order.Items.Count > 0 && order.Items.All(i => i.ReceivedQuantity >= i.OrderedQuantity);
        if (fullyReceived)
            return ServiceResponseDto<PurchaseOrderDto>.Fail("Cannot cancel a fully received order");

        order.Status = PurchaseOrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(reason))
            order.Notes = (order.Notes ?? "") + "\n[Cancelled: " + reason + "]";
        await _unitOfWork.SaveAsync();

        var detail = await GetPurchaseOrderAsync(orgId, order.Id);
        return detail.Success ? ServiceResponseDto<PurchaseOrderDto>.Ok(MapToSummary(detail.Data!)) : ServiceResponseDto<PurchaseOrderDto>.Fail(detail.Message);
    }

    public async Task<ServiceResponseDto> ReceiveGoodsAsync(int orgId, int orderId, GoodsReceivingDto dto, string userId)
    {
        var order = await _context.PurchaseOrders
            .Include(p => p.Items)
            .ThenInclude(i => i.StockItem)
            .FirstOrDefaultAsync(p => p.Id == orderId && p.OrganizationId == orgId);

        if (order == null)
            return ServiceResponseDto.Fail("Purchase order not found");

        if (order.Status == PurchaseOrderStatus.Draft || order.Status == PurchaseOrderStatus.Cancelled)
            return ServiceResponseDto.Fail("Cannot receive goods for draft or cancelled order");

        if (dto.Items == null || !dto.Items.Any())
            return ServiceResponseDto.Fail("At least one receiving item is required");

        var receiving = new GoodsReceiving
        {
            PurchaseOrderId = orderId,
            OrganizationId = orgId,
            ReceivingDate = DateTime.UtcNow,
            ReceivedBy = userId,
            InvoiceNumber = dto.InvoiceNumber,
            Notes = dto.Notes,
            Status = GoodsReceivingStatus.Approved,
            CreatedAt = DateTime.UtcNow
        };
        _context.GoodsReceivings.Add(receiving);
        await _unitOfWork.SaveAsync();

        var poItemLookup = order.Items.ToDictionary(i => i.Id);

        foreach (var item in dto.Items)
        {
            if (!poItemLookup.TryGetValue(item.PurchaseOrderItemId, out var poItem))
                return ServiceResponseDto.Fail($"Purchase order item {item.PurchaseOrderItemId} not found");

            if (poItem.StockItemId != item.StockItemId)
                return ServiceResponseDto.Fail($"Stock item mismatch for PO item {item.PurchaseOrderItemId}");

            _context.GoodsReceivingItems.Add(new GoodsReceivingItem
            {
                GoodsReceivingId = receiving.Id,
                PurchaseOrderItemId = item.PurchaseOrderItemId,
                StockItemId = item.StockItemId,
                ReceivedQuantity = item.ReceivedQuantity,
                AcceptedQuantity = item.AcceptedQuantity,
                RejectedQuantity = item.RejectedQuantity,
                UnitPrice = item.UnitPrice,
                BatchNumber = item.BatchNumber,
                ExpirationDate = item.ExpirationDate
            });

            poItem.ReceivedQuantity += item.AcceptedQuantity;

            var stockItem = poItem.StockItem;
            var previousQty = stockItem.CurrentQuantity;
            var newQty = previousQty + item.AcceptedQuantity;
            stockItem.CurrentQuantity = newQty;
            stockItem.UpdatedAt = DateTime.UtcNow;

            _context.StockMovements.Add(new StockMovement
            {
                StockItemId = stockItem.Id,
                OrganizationId = orgId,
                MovementType = MovementType.Received,
                Quantity = item.AcceptedQuantity,
                PreviousQuantity = previousQty,
                NewQuantity = newQty,
                CostPerUnit = item.UnitPrice,
                TotalCost = item.AcceptedQuantity * item.UnitPrice,
                ReferenceType = nameof(GoodsReceiving),
                ReferenceId = receiving.Id,
                BatchNumber = item.BatchNumber,
                ExpirationDate = item.ExpirationDate,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
                Notes = dto.Notes
            });
        }

        var allReceived = order.Items.All(i => i.ReceivedQuantity >= i.OrderedQuantity);
        order.Status = allReceived ? PurchaseOrderStatus.FullyReceived : PurchaseOrderStatus.PartiallyReceived;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();
        return ServiceResponseDto.Ok("Goods received successfully");
    }

    private static PurchaseOrderDto MapToSummary(PurchaseOrderDetailDto d)
    {
        return new PurchaseOrderDto
        {
            Id = d.Id,
            OrderNumber = d.OrderNumber,
            SupplierId = d.SupplierId,
            SupplierName = d.SupplierName,
            Status = d.Status,
            OrderDate = d.OrderDate,
            ExpectedDeliveryDate = d.ExpectedDeliveryDate,
            TotalAmount = d.TotalAmount,
            ItemCount = d.ItemCount,
            CreatedAt = d.CreatedAt
        };
    }

    private async Task<string> GenerateOrderNumberAsync(int orgId)
    {
        var prefix = "PO-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-";
        var last = await _context.PurchaseOrders
            .Where(p => p.OrganizationId == orgId && p.OrderNumber.StartsWith(prefix))
            .OrderByDescending(p => p.OrderNumber)
            .Select(p => p.OrderNumber)
            .FirstOrDefaultAsync();

        var next = 1;
        if (last != null && last.Length > prefix.Length)
        {
            var suffix = last.Substring(prefix.Length);
            if (int.TryParse(suffix, out var n))
                next = n + 1;
        }
        return prefix + next.ToString("D3");
    }
}
