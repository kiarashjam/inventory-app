using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface ISupplierStockItemService
{
    Task<ServiceResponseDto<List<SupplierStockItemDto>>> GetItemsBySupplierAsync(int orgId, int supplierId);
    Task<ServiceResponseDto<List<SupplierStockItemDto>>> GetSuppliersByItemAsync(int orgId, int stockItemId);
    Task<ServiceResponseDto<SupplierStockItemDto>> LinkAsync(int orgId, CreateSupplierStockItemDto dto);
    Task<ServiceResponseDto<SupplierStockItemDto>> UpdateAsync(int orgId, int linkId, UpdateSupplierStockItemDto dto);
    Task<ServiceResponseDto> UnlinkAsync(int orgId, int linkId);
}
