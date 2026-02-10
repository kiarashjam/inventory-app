using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class SupplierStockItemService : ISupplierStockItemService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public SupplierStockItemService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponseDto<List<SupplierStockItemDto>>> GetItemsBySupplierAsync(int orgId, int supplierId)
    {
        // Validate supplier belongs to org
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == supplierId && s.OrganizationId == orgId);

        if (supplier == null)
            return ServiceResponseDto<List<SupplierStockItemDto>>.Fail("Supplier not found");

        var items = await _context.SupplierStockItems
            .Where(ss => ss.SupplierId == supplierId)
            .Include(ss => ss.StockItem)
            .Where(ss => ss.StockItem != null && ss.StockItem.OrganizationId == orgId)
            .Select(ss => new SupplierStockItemDto
            {
                Id = ss.Id,
                SupplierId = ss.SupplierId,
                SupplierName = supplier.Name,
                StockItemId = ss.StockItemId,
                StockItemName = ss.StockItem != null ? ss.StockItem.Name : string.Empty,
                SupplierSKU = ss.SupplierSKU,
                UnitPrice = ss.UnitPrice,
                MinimumOrderQuantity = ss.MinimumOrderQuantity,
                IsPreferred = ss.IsPreferred,
                LastOrderDate = ss.LastOrderDate
            })
            .ToListAsync();

        return ServiceResponseDto<List<SupplierStockItemDto>>.Ok(items);
    }

    public async Task<ServiceResponseDto<List<SupplierStockItemDto>>> GetSuppliersByItemAsync(int orgId, int stockItemId)
    {
        // Validate stock item belongs to org
        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.Id == stockItemId && s.OrganizationId == orgId);

        if (stockItem == null)
            return ServiceResponseDto<List<SupplierStockItemDto>>.Fail("Stock item not found");

        var suppliers = await _context.SupplierStockItems
            .Where(ss => ss.StockItemId == stockItemId)
            .Include(ss => ss.Supplier)
            .Where(ss => ss.Supplier != null && ss.Supplier.OrganizationId == orgId)
            .Select(ss => new SupplierStockItemDto
            {
                Id = ss.Id,
                SupplierId = ss.SupplierId,
                SupplierName = ss.Supplier != null ? ss.Supplier.Name : string.Empty,
                StockItemId = ss.StockItemId,
                StockItemName = stockItem.Name,
                SupplierSKU = ss.SupplierSKU,
                UnitPrice = ss.UnitPrice,
                MinimumOrderQuantity = ss.MinimumOrderQuantity,
                IsPreferred = ss.IsPreferred,
                LastOrderDate = ss.LastOrderDate
            })
            .ToListAsync();

        return ServiceResponseDto<List<SupplierStockItemDto>>.Ok(suppliers);
    }

    public async Task<ServiceResponseDto<SupplierStockItemDto>> LinkAsync(int orgId, CreateSupplierStockItemDto dto)
    {
        // Validate supplier belongs to org
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == dto.SupplierId && s.OrganizationId == orgId);

        if (supplier == null)
            return ServiceResponseDto<SupplierStockItemDto>.Fail("Supplier not found");

        // Validate stock item belongs to org
        var stockItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.Id == dto.StockItemId && s.OrganizationId == orgId);

        if (stockItem == null)
            return ServiceResponseDto<SupplierStockItemDto>.Fail("Stock item not found");

        // Check for duplicate link
        var existingLink = await _context.SupplierStockItems
            .FirstOrDefaultAsync(ss => ss.SupplierId == dto.SupplierId && ss.StockItemId == dto.StockItemId);

        if (existingLink != null)
            return ServiceResponseDto<SupplierStockItemDto>.Fail("Link between supplier and stock item already exists");

        // If IsPreferred is true, unset IsPreferred on other links for the same StockItem
        if (dto.IsPreferred)
        {
            var otherLinks = await _context.SupplierStockItems
                .Where(ss => ss.StockItemId == dto.StockItemId && ss.IsPreferred)
                .ToListAsync();

            foreach (var otherLink in otherLinks)
            {
                otherLink.IsPreferred = false;
            }
        }

        var link = new Domain.Entities.SupplierStockItem
        {
            SupplierId = dto.SupplierId,
            StockItemId = dto.StockItemId,
            SupplierSKU = dto.SupplierSKU,
            UnitPrice = dto.UnitPrice,
            MinimumOrderQuantity = dto.MinimumOrderQuantity,
            IsPreferred = dto.IsPreferred
        };

        _context.SupplierStockItems.Add(link);
        await _unitOfWork.SaveAsync();

        var result = new SupplierStockItemDto
        {
            Id = link.Id,
            SupplierId = link.SupplierId,
            SupplierName = supplier.Name,
            StockItemId = link.StockItemId,
            StockItemName = stockItem.Name,
            SupplierSKU = link.SupplierSKU,
            UnitPrice = link.UnitPrice,
            MinimumOrderQuantity = link.MinimumOrderQuantity,
            IsPreferred = link.IsPreferred,
            LastOrderDate = link.LastOrderDate
        };

        return ServiceResponseDto<SupplierStockItemDto>.Ok(result);
    }

    public async Task<ServiceResponseDto<SupplierStockItemDto>> UpdateAsync(int orgId, int linkId, UpdateSupplierStockItemDto dto)
    {
        var link = await _context.SupplierStockItems
            .Include(ss => ss.Supplier)
            .Include(ss => ss.StockItem)
            .FirstOrDefaultAsync(ss => ss.Id == linkId);

        if (link == null)
            return ServiceResponseDto<SupplierStockItemDto>.Fail("Link not found");

        // Validate supplier and stock item belong to org
        if (link.Supplier == null || link.Supplier.OrganizationId != orgId)
            return ServiceResponseDto<SupplierStockItemDto>.Fail("Supplier not found");

        if (link.StockItem == null || link.StockItem.OrganizationId != orgId)
            return ServiceResponseDto<SupplierStockItemDto>.Fail("Stock item not found");

        // If IsPreferred is being set to true, unset IsPreferred on other links for the same StockItem
        if (dto.IsPreferred && !link.IsPreferred)
        {
            var otherLinks = await _context.SupplierStockItems
                .Where(ss => ss.StockItemId == link.StockItemId && ss.Id != linkId && ss.IsPreferred)
                .ToListAsync();

            foreach (var otherLinkItem in otherLinks)
            {
                otherLinkItem.IsPreferred = false;
            }
        }

        link.SupplierSKU = dto.SupplierSKU;
        link.UnitPrice = dto.UnitPrice;
        link.MinimumOrderQuantity = dto.MinimumOrderQuantity;
        link.IsPreferred = dto.IsPreferred;

        await _unitOfWork.SaveAsync();

        var result = new SupplierStockItemDto
        {
            Id = link.Id,
            SupplierId = link.SupplierId,
            SupplierName = link.Supplier.Name,
            StockItemId = link.StockItemId,
            StockItemName = link.StockItem.Name,
            SupplierSKU = link.SupplierSKU,
            UnitPrice = link.UnitPrice,
            MinimumOrderQuantity = link.MinimumOrderQuantity,
            IsPreferred = link.IsPreferred,
            LastOrderDate = link.LastOrderDate
        };

        return ServiceResponseDto<SupplierStockItemDto>.Ok(result);
    }

    public async Task<ServiceResponseDto> UnlinkAsync(int orgId, int linkId)
    {
        var link = await _context.SupplierStockItems
            .Include(ss => ss.Supplier)
            .Include(ss => ss.StockItem)
            .FirstOrDefaultAsync(ss => ss.Id == linkId);

        if (link == null)
            return ServiceResponseDto.Fail("Link not found");

        // Validate supplier and stock item belong to org
        if (link.Supplier == null || link.Supplier.OrganizationId != orgId)
            return ServiceResponseDto.Fail("Supplier not found");

        if (link.StockItem == null || link.StockItem.OrganizationId != orgId)
            return ServiceResponseDto.Fail("Stock item not found");

        _context.SupplierStockItems.Remove(link);
        await _unitOfWork.SaveAsync();

        return ServiceResponseDto.Ok("Link removed successfully");
    }
}
