using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface IWasteService
{
    Task<PaginatedResponseDto<WasteRecordDto>> GetWasteRecordsAsync(int orgId, int page, int pageSize, string? reason = null);
    Task<ServiceResponseDto<WasteRecordDto>> CreateWasteRecordAsync(int orgId, CreateWasteRecordDto dto, string userId);
    Task<ServiceResponseDto<WasteSummaryDto>> GetWasteSummaryAsync(int orgId, DateTime? startDate, DateTime? endDate);
}
