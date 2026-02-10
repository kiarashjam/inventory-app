using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface IPosConnectionService
{
    Task<ServiceResponseDto<List<PosConnectionDto>>> GetConnectionsAsync(int orgId);
    Task<ServiceResponseDto<PosConnectionCreatedDto>> CreateConnectionAsync(int orgId, CreatePosConnectionDto dto);
    Task<ServiceResponseDto<PosConnectionDto>> ToggleConnectionAsync(int orgId, int connectionId);
    Task<ServiceResponseDto> DeleteConnectionAsync(int orgId, int connectionId);
    Task<ServiceResponseDto<PosConnectionCreatedDto>> RegenerateApiKeyAsync(int orgId, int connectionId);
}
