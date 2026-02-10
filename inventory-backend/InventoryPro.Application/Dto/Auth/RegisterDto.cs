namespace InventoryPro.Application.Dto.Auth;

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string? Currency { get; set; }
    public string? Timezone { get; set; }
}
