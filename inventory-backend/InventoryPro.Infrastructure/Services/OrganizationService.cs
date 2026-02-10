using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class OrganizationService : IOrganizationService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public OrganizationService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponseDto<OrganizationDto>> GetOrganizationAsync(int orgId)
    {
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == orgId);

        if (org == null)
            return ServiceResponseDto<OrganizationDto>.Fail("Organization not found");

        var dto = MapToDto(org);
        return ServiceResponseDto<OrganizationDto>.Ok(dto);
    }

    public async Task<ServiceResponseDto<OrganizationDto>> UpdateOrganizationAsync(int orgId, UpdateOrganizationDto dto)
    {
        var org = await _context.Organizations
            .FirstOrDefaultAsync(o => o.Id == orgId);

        if (org == null)
            return ServiceResponseDto<OrganizationDto>.Fail("Organization not found");

        // Update only non-null fields from dto
        if (dto.Name != null)
            org.Name = dto.Name;

        if (dto.Address != null)
            org.Address = dto.Address;

        if (dto.City != null)
            org.City = dto.City;

        if (dto.PostalCode != null)
            org.PostalCode = dto.PostalCode;

        if (dto.Country != null)
            org.Country = dto.Country;

        if (dto.Phone != null)
            org.Phone = dto.Phone;

        if (dto.Email != null)
            org.Email = dto.Email;

        if (dto.Currency != null)
            org.Currency = dto.Currency;

        if (dto.Timezone != null)
            org.Timezone = dto.Timezone;

        if (dto.DefaultFoodCostTargetPercent.HasValue)
            org.DefaultFoodCostTargetPercent = dto.DefaultFoodCostTargetPercent.Value;

        if (dto.DefaultBeverageCostTargetPercent.HasValue)
            org.DefaultBeverageCostTargetPercent = dto.DefaultBeverageCostTargetPercent.Value;

        if (dto.VarianceAlertThresholdPercent.HasValue)
            org.VarianceAlertThresholdPercent = dto.VarianceAlertThresholdPercent.Value;

        if (dto.LowStockAlertEmail.HasValue)
            org.LowStockAlertEmail = dto.LowStockAlertEmail.Value;

        if (dto.AutoDeductOnSale.HasValue)
            org.AutoDeductOnSale = dto.AutoDeductOnSale.Value;

        if (dto.EnableHaccp.HasValue)
            org.EnableHaccp = dto.EnableHaccp.Value;

        if (dto.EnableAllergenTracking.HasValue)
            org.EnableAllergenTracking = dto.EnableAllergenTracking.Value;

        if (dto.EnablePrepLists.HasValue)
            org.EnablePrepLists = dto.EnablePrepLists.Value;

        await _unitOfWork.SaveAsync();

        var result = MapToDto(org);
        return ServiceResponseDto<OrganizationDto>.Ok(result);
    }

    private OrganizationDto MapToDto(Domain.Entities.Organization org)
    {
        return new OrganizationDto
        {
            Id = org.Id,
            Name = org.Name,
            Address = org.Address,
            City = org.City,
            PostalCode = org.PostalCode,
            Country = org.Country,
            Phone = org.Phone,
            Email = org.Email,
            Currency = org.Currency,
            Timezone = org.Timezone,
            DefaultFoodCostTargetPercent = org.DefaultFoodCostTargetPercent,
            DefaultBeverageCostTargetPercent = org.DefaultBeverageCostTargetPercent,
            VarianceAlertThresholdPercent = org.VarianceAlertThresholdPercent,
            LowStockAlertEmail = org.LowStockAlertEmail,
            AutoDeductOnSale = org.AutoDeductOnSale,
            EnableHaccp = org.EnableHaccp,
            EnableAllergenTracking = org.EnableAllergenTracking,
            EnablePrepLists = org.EnablePrepLists,
            SubscriptionPlan = org.SubscriptionPlan.ToString(),
            CreatedAt = org.CreatedAt
        };
    }
}
