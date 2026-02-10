using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface IStorageLocationService
{
    Task<List<StorageLocationDto>> GetLocationsAsync(int orgId);
    Task<ServiceResponseDto<StorageLocationDto>> CreateLocationAsync(int orgId, CreateStorageLocationDto dto);
    Task<ServiceResponseDto<StorageLocationDto>> UpdateLocationAsync(int orgId, int locationId, UpdateStorageLocationDto dto);
    Task<ServiceResponseDto> DeleteLocationAsync(int orgId, int locationId);
}
