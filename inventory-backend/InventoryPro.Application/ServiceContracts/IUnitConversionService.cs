using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface IUnitConversionService
{
    Task<ServiceResponseDto<List<UnitConversionDto>>> GetConversionsAsync(int orgId);
    Task<ServiceResponseDto<UnitConversionDto>> CreateConversionAsync(int orgId, CreateUnitConversionDto dto);
    Task<ServiceResponseDto> DeleteConversionAsync(int orgId, int conversionId);
    Task<ServiceResponseDto<decimal>> ConvertAsync(int orgId, int fromUnit, int toUnit, decimal quantity);
}
