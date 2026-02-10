using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface IStockItemService
{
    Task<PaginatedResponseDto<StockItemDto>> GetStockItemsAsync(int orgId, int page = 1, int pageSize = 20, string? search = null, int? categoryId = null, bool? isActive = null, bool? lowStockOnly = null);
    Task<ServiceResponseDto<StockItemDto>> GetStockItemAsync(int orgId, int stockItemId);
    Task<ServiceResponseDto<StockItemDto>> CreateStockItemAsync(int orgId, CreateStockItemDto dto, string userId);
    Task<ServiceResponseDto<StockItemDto>> UpdateStockItemAsync(int orgId, int stockItemId, UpdateStockItemDto dto);
    Task<ServiceResponseDto> DeleteStockItemAsync(int orgId, int stockItemId);
    Task<ServiceResponseDto<StockMovementDto>> AdjustStockAsync(int orgId, int stockItemId, StockAdjustmentDto dto, string userId);
    Task<PaginatedResponseDto<StockMovementDto>> GetStockMovementsAsync(int orgId, int stockItemId, int page = 1, int pageSize = 20);
    Task<List<StockItemDto>> GetLowStockItemsAsync(int orgId);
}
