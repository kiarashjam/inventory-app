using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface IStockCountService
{
    Task<PaginatedResponseDto<StockCountDto>> GetStockCountsAsync(int orgId, int page, int pageSize, string? status = null);
    Task<ServiceResponseDto<StockCountDetailDto>> GetStockCountAsync(int orgId, int countId);
    Task<ServiceResponseDto<StockCountDetailDto>> StartCountAsync(int orgId, CreateStockCountDto dto, string userId);
    Task<ServiceResponseDto<StockCountDetailDto>> SubmitCountItemsAsync(int orgId, int countId, List<StockCountItemDto> items, string userId);
    Task<ServiceResponseDto<StockCountDetailDto>> CompleteCountAsync(int orgId, int countId);
    Task<ServiceResponseDto<StockCountDetailDto>> ApproveCountAsync(int orgId, int countId, string userId);
}
