using InventoryPro.Application.Dto.Common;
using InventoryPro.Application.Dto.Inventory;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InventoryPro.Infrastructure.Services;

public class SupplierService : ISupplierService
{
    private readonly InventoryProDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public SupplierService(InventoryProDbContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<SupplierDto>> GetSuppliersAsync(int orgId, string? search = null, bool? isActive = null)
    {
        var query = _context.Suppliers
            .Where(s => s.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s =>
                s.Name.Contains(search) ||
                (s.ContactPerson != null && s.ContactPerson.Contains(search)) ||
                (s.Email != null && s.Email.Contains(search)) ||
                (s.Phone != null && s.Phone.Contains(search)) ||
                (s.City != null && s.City.Contains(search)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        var suppliers = await query
            .OrderBy(s => s.Name)
            .Select(s => new SupplierDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Email = s.Email,
                Phone = s.Phone,
                Address = s.Address,
                City = s.City,
                PostalCode = s.PostalCode,
                Country = s.Country,
                PaymentTerms = s.PaymentTerms,
                LeadTimeDays = s.LeadTimeDays,
                MinimumOrderAmount = s.MinimumOrderAmount,
                Notes = s.Notes,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return suppliers;
    }

    public async Task<ServiceResponseDto<SupplierDto>> GetSupplierAsync(int orgId, int supplierId)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == supplierId && s.OrganizationId == orgId);

        if (supplier == null)
            return ServiceResponseDto<SupplierDto>.Fail("Supplier not found");

        var dto = MapToDto(supplier);
        return ServiceResponseDto<SupplierDto>.Ok(dto);
    }

    public async Task<ServiceResponseDto<SupplierDto>> CreateSupplierAsync(int orgId, CreateSupplierDto dto)
    {
        // Check for duplicate name
        var existingSupplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId && s.Name == dto.Name);

        if (existingSupplier != null)
            return ServiceResponseDto<SupplierDto>.Fail("Supplier with this name already exists");

        // Validate email format if provided
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            try
            {
                var email = new System.Net.Mail.MailAddress(dto.Email);
            }
            catch
            {
                return ServiceResponseDto<SupplierDto>.Fail("Invalid email format");
            }
        }

        var supplier = new Domain.Entities.Supplier
        {
            OrganizationId = orgId,
            Name = dto.Name,
            ContactPerson = dto.ContactPerson,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            PaymentTerms = dto.PaymentTerms,
            LeadTimeDays = dto.LeadTimeDays,
            MinimumOrderAmount = dto.MinimumOrderAmount,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);
        await _unitOfWork.SaveAsync();

        var result = MapToDto(supplier);
        return ServiceResponseDto<SupplierDto>.Ok(result);
    }

    public async Task<ServiceResponseDto<SupplierDto>> UpdateSupplierAsync(int orgId, int supplierId, UpdateSupplierDto dto)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == supplierId && s.OrganizationId == orgId);

        if (supplier == null)
            return ServiceResponseDto<SupplierDto>.Fail("Supplier not found");

        // Check for duplicate name (excluding current supplier)
        var existingSupplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.OrganizationId == orgId && s.Name == dto.Name && s.Id != supplierId);

        if (existingSupplier != null)
            return ServiceResponseDto<SupplierDto>.Fail("Supplier with this name already exists");

        // Validate email format if provided
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            try
            {
                var email = new System.Net.Mail.MailAddress(dto.Email);
            }
            catch
            {
                return ServiceResponseDto<SupplierDto>.Fail("Invalid email format");
            }
        }

        supplier.Name = dto.Name;
        supplier.ContactPerson = dto.ContactPerson;
        supplier.Email = dto.Email;
        supplier.Phone = dto.Phone;
        supplier.Address = dto.Address;
        supplier.City = dto.City;
        supplier.PostalCode = dto.PostalCode;
        supplier.Country = dto.Country;
        supplier.PaymentTerms = dto.PaymentTerms;
        supplier.LeadTimeDays = dto.LeadTimeDays;
        supplier.MinimumOrderAmount = dto.MinimumOrderAmount;
        supplier.Notes = dto.Notes;
        supplier.IsActive = dto.IsActive;

        await _unitOfWork.SaveAsync();

        var result = MapToDto(supplier);
        return ServiceResponseDto<SupplierDto>.Ok(result);
    }

    public async Task<ServiceResponseDto> DeleteSupplierAsync(int orgId, int supplierId)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == supplierId && s.OrganizationId == orgId);

        if (supplier == null)
            return ServiceResponseDto.Fail("Supplier not found");

        // Check if supplier is used as primary supplier for any stock items
        var hasStockItems = await _context.StockItems
            .AnyAsync(s => s.PrimarySupplierId == supplierId && s.OrganizationId == orgId && s.IsActive);

        if (hasStockItems)
            return ServiceResponseDto.Fail("Cannot delete supplier that is assigned to active stock items");

        // Check if supplier has purchase orders
        var hasPurchaseOrders = await _context.PurchaseOrders
            .AnyAsync(po => po.SupplierId == supplierId);

        if (hasPurchaseOrders)
        {
            // Optionally: return error or allow deletion
            // For now, we'll allow soft delete but could add validation
        }

        // Soft delete
        supplier.IsActive = false;

        await _unitOfWork.SaveAsync();
        return ServiceResponseDto.Ok("Supplier deleted successfully");
    }

    private SupplierDto MapToDto(Domain.Entities.Supplier supplier)
    {
        return new SupplierDto
        {
            Id = supplier.Id,
            Name = supplier.Name,
            ContactPerson = supplier.ContactPerson,
            Email = supplier.Email,
            Phone = supplier.Phone,
            Address = supplier.Address,
            City = supplier.City,
            PostalCode = supplier.PostalCode,
            Country = supplier.Country,
            PaymentTerms = supplier.PaymentTerms,
            LeadTimeDays = supplier.LeadTimeDays,
            MinimumOrderAmount = supplier.MinimumOrderAmount,
            Notes = supplier.Notes,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt
        };
    }
}
