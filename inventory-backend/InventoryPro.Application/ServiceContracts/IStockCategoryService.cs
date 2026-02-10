using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface IStockCategoryService
{
    Task<List<StockCategoryDto>> GetCategoriesAsync(int orgId);
    Task<ServiceResponseDto<StockCategoryDto>> CreateCategoryAsync(int orgId, CreateStockCategoryDto dto);
    Task<ServiceResponseDto<StockCategoryDto>> UpdateCategoryAsync(int orgId, int categoryId, UpdateStockCategoryDto dto);
    Task<ServiceResponseDto> DeleteCategoryAsync(int orgId, int categoryId);
}
