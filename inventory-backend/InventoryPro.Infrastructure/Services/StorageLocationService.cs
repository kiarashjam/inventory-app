using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class StorageLocationService : IStorageLocationService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public StorageLocationService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<StorageLocationDto>> GetLocationsAsync(int orgId)
    {
        var locations = await _context.StorageLocations
            .Where(l => l.OrganizationId == orgId)
            .OrderBy(l => l.DisplayOrder)
            .ThenBy(l => l.Name)
            .Select(l => new StorageLocationDto
            {
                Id = l.Id,
                Name = l.Name,
                Description = l.Description,
                LocationType = l.LocationType,
                DisplayOrder = l.DisplayOrder,
                TemperatureMin = l.TemperatureMin,
                TemperatureMax = l.TemperatureMax,
                IsActive = l.IsActive
            })
            .ToListAsync();

        return locations;
    }

    public async Task<ServiceResponseDto<StorageLocationDto>> CreateLocationAsync(int orgId, CreateStorageLocationDto dto)
    {
        // Check for duplicate name
        var existingLocation = await _context.StorageLocations
            .FirstOrDefaultAsync(l => l.OrganizationId == orgId && l.Name == dto.Name);

        if (existingLocation != null)
            return ServiceResponseDto<StorageLocationDto>.Fail("Storage location with this name already exists");

        // Validate temperature range if both are provided
        if (dto.TemperatureMin.HasValue && dto.TemperatureMax.HasValue)
        {
            if (dto.TemperatureMin.Value > dto.TemperatureMax.Value)
                return ServiceResponseDto<StorageLocationDto>.Fail("Minimum temperature cannot be greater than maximum temperature");
        }

        var location = new Domain.Entities.StorageLocation
        {
            OrganizationId = orgId,
            Name = dto.Name,
            Description = dto.Description,
            LocationType = dto.LocationType,
            DisplayOrder = dto.DisplayOrder,
            TemperatureMin = dto.TemperatureMin,
            TemperatureMax = dto.TemperatureMax,
            IsActive = true
        };

        _context.StorageLocations.Add(location);
        await _unitOfWork.SaveAsync();

        var result = MapToDto(location);
        return ServiceResponseDto<StorageLocationDto>.Ok(result);
    }

    public async Task<ServiceResponseDto<StorageLocationDto>> UpdateLocationAsync(int orgId, int locationId, UpdateStorageLocationDto dto)
    {
        var location = await _context.StorageLocations
            .FirstOrDefaultAsync(l => l.Id == locationId && l.OrganizationId == orgId);

        if (location == null)
            return ServiceResponseDto<StorageLocationDto>.Fail("Storage location not found");

        // Check for duplicate name (excluding current location)
        var existingLocation = await _context.StorageLocations
            .FirstOrDefaultAsync(l => l.OrganizationId == orgId && l.Name == dto.Name && l.Id != locationId);

        if (existingLocation != null)
            return ServiceResponseDto<StorageLocationDto>.Fail("Storage location with this name already exists");

        // Validate temperature range if both are provided
        if (dto.TemperatureMin.HasValue && dto.TemperatureMax.HasValue)
        {
            if (dto.TemperatureMin.Value > dto.TemperatureMax.Value)
                return ServiceResponseDto<StorageLocationDto>.Fail("Minimum temperature cannot be greater than maximum temperature");
        }

        location.Name = dto.Name;
        location.Description = dto.Description;
        location.LocationType = dto.LocationType;
        location.DisplayOrder = dto.DisplayOrder;
        location.TemperatureMin = dto.TemperatureMin;
        location.TemperatureMax = dto.TemperatureMax;
        location.IsActive = dto.IsActive;

        await _unitOfWork.SaveAsync();

        var result = MapToDto(location);
        return ServiceResponseDto<StorageLocationDto>.Ok(result);
    }

    public async Task<ServiceResponseDto> DeleteLocationAsync(int orgId, int locationId)
    {
        var location = await _context.StorageLocations
            .FirstOrDefaultAsync(l => l.Id == locationId && l.OrganizationId == orgId);

        if (location == null)
            return ServiceResponseDto.Fail("Storage location not found");

        // Check if location is used as primary storage location for any stock items
        var hasStockItems = await _context.StockItems
            .AnyAsync(s => s.PrimaryStorageLocationId == locationId && s.OrganizationId == orgId && s.IsActive);

        if (hasStockItems)
            return ServiceResponseDto.Fail("Cannot delete storage location that is assigned to active stock items");

        // Check if location has stock items stored in it
        var hasStockAtLocation = await _context.StockItemStorageLocations
            .AnyAsync(sl => sl.StorageLocationId == locationId);

        if (hasStockAtLocation)
        {
            // Optionally: return error or allow deletion
            // For now, we'll allow deletion but could add validation
        }

        // Soft delete
        location.IsActive = false;

        await _unitOfWork.SaveAsync();
        return ServiceResponseDto.Ok("Storage location deleted successfully");
    }

    private StorageLocationDto MapToDto(Domain.Entities.StorageLocation location)
    {
        return new StorageLocationDto
        {
            Id = location.Id,
            Name = location.Name,
            Description = location.Description,
            LocationType = location.LocationType,
            DisplayOrder = location.DisplayOrder,
            TemperatureMin = location.TemperatureMin,
            TemperatureMax = location.TemperatureMax,
            IsActive = location.IsActive
        };
    }
}
