using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;

namespace InventoryPro.Application.ServiceContracts;

public interface IOrganizationService
{
    Task<ServiceResponseDto<OrganizationDto>> GetOrganizationAsync(int orgId);
    Task<ServiceResponseDto<OrganizationDto>> UpdateOrganizationAsync(int orgId, UpdateOrganizationDto dto);
}
