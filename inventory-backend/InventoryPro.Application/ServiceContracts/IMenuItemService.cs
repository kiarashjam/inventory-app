using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface IMenuItemService
{
    Task<PaginatedResponseDto<MenuItemDto>> GetMenuItemsAsync(int orgId, int page = 1, int pageSize = 20, string? search = null, string? category = null, bool? isActive = null);
    Task<ServiceResponseDto<MenuItemDto>> GetMenuItemAsync(int orgId, int menuItemId);
    Task<ServiceResponseDto<MenuItemDto>> CreateMenuItemAsync(int orgId, CreateMenuItemDto dto);
    Task<ServiceResponseDto<MenuItemDto>> UpdateMenuItemAsync(int orgId, int menuItemId, UpdateMenuItemDto dto);
    Task<ServiceResponseDto> DeleteMenuItemAsync(int orgId, int menuItemId);
}
