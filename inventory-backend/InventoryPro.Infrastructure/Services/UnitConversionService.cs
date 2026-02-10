using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Enums;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class UnitConversionService : IUnitConversionService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UnitConversionService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponseDto<List<UnitConversionDto>>> GetConversionsAsync(int orgId)
    {
        var conversions = await _context.UnitConversions
            .Where(uc => uc.OrganizationId == orgId)
            .Select(uc => new UnitConversionDto
            {
                Id = uc.Id,
                FromUnit = (int)uc.FromUnit,
                ToUnit = (int)uc.ToUnit,
                ConversionFactor = uc.ConversionFactor
            })
            .ToListAsync();

        return ServiceResponseDto<List<UnitConversionDto>>.Ok(conversions);
    }

    public async Task<ServiceResponseDto<UnitConversionDto>> CreateConversionAsync(int orgId, CreateUnitConversionDto dto)
    {
        // Validate no duplicate (FromUnit+ToUnit)
        var existing = await _context.UnitConversions
            .FirstOrDefaultAsync(uc => uc.OrganizationId == orgId &&
                uc.FromUnit == (UnitOfMeasurement)dto.FromUnit &&
                uc.ToUnit == (UnitOfMeasurement)dto.ToUnit);

        if (existing != null)
            return ServiceResponseDto<UnitConversionDto>.Fail("Conversion already exists");

        // Validate conversion factor
        if (dto.ConversionFactor <= 0)
            return ServiceResponseDto<UnitConversionDto>.Fail("Conversion factor must be greater than zero");

        // Add the conversion
        var conversion = new Domain.Entities.UnitConversion
        {
            OrganizationId = orgId,
            FromUnit = (UnitOfMeasurement)dto.FromUnit,
            ToUnit = (UnitOfMeasurement)dto.ToUnit,
            ConversionFactor = dto.ConversionFactor
        };

        _context.UnitConversions.Add(conversion);

        // Add the reverse conversion (ToUnit->FromUnit with 1/ConversionFactor)
        var reverseConversion = new Domain.Entities.UnitConversion
        {
            OrganizationId = orgId,
            FromUnit = (UnitOfMeasurement)dto.ToUnit,
            ToUnit = (UnitOfMeasurement)dto.FromUnit,
            ConversionFactor = 1 / dto.ConversionFactor
        };

        _context.UnitConversions.Add(reverseConversion);
        await _unitOfWork.SaveAsync();

        var result = new UnitConversionDto
        {
            Id = conversion.Id,
            FromUnit = (int)conversion.FromUnit,
            ToUnit = (int)conversion.ToUnit,
            ConversionFactor = conversion.ConversionFactor
        };

        return ServiceResponseDto<UnitConversionDto>.Ok(result);
    }

    public async Task<ServiceResponseDto> DeleteConversionAsync(int orgId, int conversionId)
    {
        var conversion = await _context.UnitConversions
            .FirstOrDefaultAsync(uc => uc.Id == conversionId && uc.OrganizationId == orgId);

        if (conversion == null)
            return ServiceResponseDto.Fail("Conversion not found");

        // Find and delete both directions
        var reverseConversion = await _context.UnitConversions
            .FirstOrDefaultAsync(uc => uc.OrganizationId == orgId &&
                uc.FromUnit == conversion.ToUnit &&
                uc.ToUnit == conversion.FromUnit);

        _context.UnitConversions.Remove(conversion);
        if (reverseConversion != null)
        {
            _context.UnitConversions.Remove(reverseConversion);
        }

        await _unitOfWork.SaveAsync();

        return ServiceResponseDto.Ok("Conversion deleted successfully");
    }

    public async Task<ServiceResponseDto<decimal>> ConvertAsync(int orgId, int fromUnit, int toUnit, decimal quantity)
    {
        if (fromUnit == toUnit)
            return ServiceResponseDto<decimal>.Ok(quantity);

        var conversion = await _context.UnitConversions
            .FirstOrDefaultAsync(uc => uc.OrganizationId == orgId &&
                uc.FromUnit == (UnitOfMeasurement)fromUnit &&
                uc.ToUnit == (UnitOfMeasurement)toUnit);

        if (conversion == null)
            return ServiceResponseDto<decimal>.Fail("Conversion not found");

        var result = quantity * conversion.ConversionFactor;
        return ServiceResponseDto<decimal>.Ok(result);
    }
}
