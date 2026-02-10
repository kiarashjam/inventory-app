using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface ISupplierService
{
    Task<List<SupplierDto>> GetSuppliersAsync(int orgId, string? search = null, bool? isActive = null);
    Task<ServiceResponseDto<SupplierDto>> GetSupplierAsync(int orgId, int supplierId);
    Task<ServiceResponseDto<SupplierDto>> CreateSupplierAsync(int orgId, CreateSupplierDto dto);
    Task<ServiceResponseDto<SupplierDto>> UpdateSupplierAsync(int orgId, int supplierId, UpdateSupplierDto dto);
    Task<ServiceResponseDto> DeleteSupplierAsync(int orgId, int supplierId);
}
