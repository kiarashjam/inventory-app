using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface IRecipeService
{
    Task<ServiceResponseDto<RecipeDto>> GetRecipeAsync(int orgId, int menuItemId);
    Task<ServiceResponseDto> SetRecipeMappingsAsync(int orgId, int menuItemId, List<RecipeMappingDto> mappings);
    Task<ServiceResponseDto> DeleteMappingAsync(int orgId, int mappingId);
    Task<ServiceResponseDto<List<MenuItemDto>>> GetMenuItemsUsingStockItemAsync(int orgId, int stockItemId);
}
