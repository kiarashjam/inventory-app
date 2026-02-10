using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InventoryPro.Application.Dto.Auth;
using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InventoryPro.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly InventoryProDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<AppUser> userManager, InventoryProDbContext context, IConfiguration configuration)
    {
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
    }

    public async Task<ServiceResponseDto<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return ServiceResponseDto<AuthResponseDto>.Fail("Email already registered");

        var user = new AppUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return ServiceResponseDto<AuthResponseDto>.Fail(string.Join(", ", result.Errors.Select(e => e.Description)));

        Organization? org = null;
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Create organization
            org = new Organization
            {
                Name = dto.OrganizationName,
                Slug = dto.OrganizationName.ToLower().Replace(" ", "-"),
                Currency = dto.Currency ?? "CHF",
                Timezone = dto.Timezone ?? "Europe/Zurich",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.Organizations.Add(org);
            await _context.SaveChangesAsync();

            // Link user to organization as Owner
            var orgUser = new OrganizationUser
            {
                OrganizationId = org.Id,
                UserId = user.Id,
                Role = UserRole.Owner,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };
            _context.OrganizationUsers.Add(orgUser);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            await _userManager.DeleteAsync(user);
            throw;
        }

        var tokens = GenerateTokens(user, org!.Id, org.Name, UserRole.Owner);
        return ServiceResponseDto<AuthResponseDto>.Ok(new AuthResponseDto
        {
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = UserRole.Owner.ToString(),
                OrganizationId = org.Id,
                OrganizationName = org.Name
            }
        });
    }

    public async Task<ServiceResponseDto<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return ServiceResponseDto<AuthResponseDto>.Fail("Invalid email or password");

        if (!user.IsActive)
            return ServiceResponseDto<AuthResponseDto>.Fail("Account is deactivated");

        var orgUser = await _context.OrganizationUsers
            .Include(ou => ou.Organization)
            .FirstOrDefaultAsync(ou => ou.UserId == user.Id && ou.IsActive && ou.Organization.IsActive);

        if (orgUser == null)
            return ServiceResponseDto<AuthResponseDto>.Fail("User is not associated with any organization");

        var tokens = GenerateTokens(user, orgUser.OrganizationId, orgUser.Organization.Name, orgUser.Role);
        return ServiceResponseDto<AuthResponseDto>.Ok(new AuthResponseDto
        {
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = orgUser.Role.ToString(),
                OrganizationId = orgUser.OrganizationId,
                OrganizationName = orgUser.Organization.Name
            }
        });
    }

    public async Task<ServiceResponseDto<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto dto)
    {
        var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
        if (principal == null)
            return ServiceResponseDto<AuthResponseDto>.Fail("Invalid token");

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return ServiceResponseDto<AuthResponseDto>.Fail("Invalid token");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || !user.IsActive)
            return ServiceResponseDto<AuthResponseDto>.Fail("User not found or inactive");

        var orgId = int.Parse(principal.FindFirst("OrganizationId")?.Value ?? "0");
        var orgUser = await _context.OrganizationUsers
            .Include(ou => ou.Organization)
            .FirstOrDefaultAsync(ou => ou.UserId == user.Id && ou.OrganizationId == orgId && ou.IsActive);

        if (orgUser == null)
            return ServiceResponseDto<AuthResponseDto>.Fail("Invalid organization context");

        var tokens = GenerateTokens(user, orgUser.OrganizationId, orgUser.Organization.Name, orgUser.Role);
        return ServiceResponseDto<AuthResponseDto>.Ok(new AuthResponseDto
        {
            AccessToken = tokens.accessToken,
            RefreshToken = tokens.refreshToken,
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = orgUser.Role.ToString(),
                OrganizationId = orgUser.OrganizationId,
                OrganizationName = orgUser.Organization.Name
            }
        });
    }

    public async Task<ServiceResponseDto<UserDto>> GetCurrentUserAsync(string userId, int organizationId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ServiceResponseDto<UserDto>.Fail("User not found");

        var orgUser = await _context.OrganizationUsers
            .Include(ou => ou.Organization)
            .FirstOrDefaultAsync(ou => ou.UserId == userId && ou.OrganizationId == organizationId && ou.IsActive);

        if (orgUser == null)
            return ServiceResponseDto<UserDto>.Fail("User not associated with organization");

        return ServiceResponseDto<UserDto>.Ok(new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = orgUser.Role.ToString(),
            OrganizationId = orgUser.OrganizationId,
            OrganizationName = orgUser.Organization.Name
        });
    }

    private (string accessToken, string refreshToken) GenerateTokens(AppUser user, int orgId, string orgName, UserRole role)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim("OrganizationId", orgId.ToString()),
            new Claim("OrganizationName", orgName)
        };

        var accessToken = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60")),
            signingCredentials: credentials
        );

        var refreshToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));

        return (new JwtSecurityTokenHandler().WriteToken(accessToken), refreshToken);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            return null;

        return principal;
    }
}
