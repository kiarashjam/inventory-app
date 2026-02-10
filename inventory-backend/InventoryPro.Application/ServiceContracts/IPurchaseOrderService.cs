using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface IPurchaseOrderService
{
    Task<PaginatedResponseDto<PurchaseOrderDto>> GetPurchaseOrdersAsync(int orgId, int page, int pageSize, string? status = null, int? supplierId = null);
    Task<ServiceResponseDto<PurchaseOrderDetailDto>> GetPurchaseOrderAsync(int orgId, int orderId);
    Task<ServiceResponseDto<PurchaseOrderDto>> CreatePurchaseOrderAsync(int orgId, CreatePurchaseOrderDto dto, string userId);
    Task<ServiceResponseDto<PurchaseOrderDto>> UpdatePurchaseOrderAsync(int orgId, int orderId, UpdatePurchaseOrderDto dto);
    Task<ServiceResponseDto<PurchaseOrderDto>> SubmitPurchaseOrderAsync(int orgId, int orderId);
    Task<ServiceResponseDto<PurchaseOrderDto>> CancelPurchaseOrderAsync(int orgId, int orderId, string? reason);
    Task<ServiceResponseDto> ReceiveGoodsAsync(int orgId, int orderId, GoodsReceivingDto dto, string userId);
}
