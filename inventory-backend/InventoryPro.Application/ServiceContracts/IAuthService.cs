using InventoryPro.Application.Dto.Auth;
using InventoryPro.Application.Dto.Common;

namespace InventoryPro.Application.ServiceContracts;

public interface IAuthService
{
    Task<ServiceResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<ServiceResponseDto<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<ServiceResponseDto<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto);
    Task<ServiceResponseDto<UserDto>> GetCurrentUserAsync(string userId, int organizationId);
}
