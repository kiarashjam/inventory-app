using System.Security.Cryptography;
using System.Text;
using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class PosConnectionService : IPosConnectionService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public PosConnectionService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponseDto<List<PosConnectionDto>>> GetConnectionsAsync(int orgId)
    {
        var list = await _context.PosConnections
            .Where(p => p.OrganizationId == orgId)
            .OrderBy(p => p.PosSystemName)
            .ToListAsync();

        var dtos = list.Select(MapToDto).ToList();
        return ServiceResponseDto<List<PosConnectionDto>>.Ok(dtos);
    }

    public async Task<ServiceResponseDto<PosConnectionCreatedDto>> CreateConnectionAsync(int orgId, CreatePosConnectionDto dto)
    {
        var apiKey = GenerateApiKey();
        var apiKeyHash = HashApiKey(apiKey);
        var webhookSecret = GenerateWebhookSecret(16);

        var entity = new PosConnection
        {
            OrganizationId = orgId,
            PosSystemName = dto.PosSystemName.Trim(),
            ApiKeyHash = apiKeyHash,
            WebhookSecret = webhookSecret,
            IsActive = true,
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PosConnections.Add(entity);
        await _unitOfWork.SaveAsync();

        return ServiceResponseDto<PosConnectionCreatedDto>.Ok(new PosConnectionCreatedDto
        {
            Id = entity.Id,
            PosSystemName = entity.PosSystemName,
            IsActive = entity.IsActive,
            ApiKey = apiKey,
            WebhookSecret = webhookSecret,
            CreatedAt = entity.CreatedAt
        });
    }

    public async Task<ServiceResponseDto<PosConnectionDto>> ToggleConnectionAsync(int orgId, int connectionId)
    {
        var conn = await _context.PosConnections
            .FirstOrDefaultAsync(p => p.Id == connectionId && p.OrganizationId == orgId);

        if (conn == null)
            return ServiceResponseDto<PosConnectionDto>.Fail("Connection not found");

        conn.IsActive = !conn.IsActive;
        conn.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();

        return ServiceResponseDto<PosConnectionDto>.Ok(MapToDto(conn));
    }

    public async Task<ServiceResponseDto> DeleteConnectionAsync(int orgId, int connectionId)
    {
        var conn = await _context.PosConnections
            .FirstOrDefaultAsync(p => p.Id == connectionId && p.OrganizationId == orgId);

        if (conn == null)
            return ServiceResponseDto.Fail("Connection not found");

        _context.PosConnections.Remove(conn);
        await _unitOfWork.SaveAsync();
        return ServiceResponseDto.Ok("Connection deleted");
    }

    public async Task<ServiceResponseDto<PosConnectionCreatedDto>> RegenerateApiKeyAsync(int orgId, int connectionId)
    {
        var conn = await _context.PosConnections
            .FirstOrDefaultAsync(p => p.Id == connectionId && p.OrganizationId == orgId);

        if (conn == null)
            return ServiceResponseDto<PosConnectionCreatedDto>.Fail("Connection not found");

        var apiKey = GenerateApiKey();
        conn.ApiKeyHash = HashApiKey(apiKey);
        conn.WebhookSecret = GenerateWebhookSecret(16);
        conn.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveAsync();

        return ServiceResponseDto<PosConnectionCreatedDto>.Ok(new PosConnectionCreatedDto
        {
            Id = conn.Id,
            PosSystemName = conn.PosSystemName,
            IsActive = conn.IsActive,
            ApiKey = apiKey,
            WebhookSecret = conn.WebhookSecret,
            CreatedAt = conn.CreatedAt
        });
    }

    private static PosConnectionDto MapToDto(PosConnection p)
    {
        var preview = string.IsNullOrEmpty(p.ApiKeyHash) || p.ApiKeyHash.Length < 8
            ? "****"
            : "****" + p.ApiKeyHash[^8..].ToLowerInvariant();

        return new PosConnectionDto
        {
            Id = p.Id,
            PosSystemName = p.PosSystemName,
            IsActive = p.IsActive,
            LastSyncAt = p.LastSyncAt,
            Notes = p.Notes,
            CreatedAt = p.CreatedAt,
            ApiKeyPreview = preview
        };
    }

    private static string GenerateApiKey()
    {
        return (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N").Substring(0, 16)).ToLowerInvariant();
    }

    private static string GenerateWebhookSecret(int length)
    {
        return Guid.NewGuid().ToString("N").Substring(0, length).ToLowerInvariant();
    }

    private static string HashApiKey(string apiKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
