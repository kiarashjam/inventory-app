using System.Text;
using InventoryPro.Application.ServiceContracts;
using InventoryPro.DataAccess.Data;
using InventoryPro.Domain.Entities;
using InventoryPro.Domain.Interfaces;
using InventoryPro.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ==================== DATABASE ====================
builder.Services.AddDbContext<InventoryProDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ==================== IDENTITY ====================
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<InventoryProDbContext>()
.AddDefaultTokenProviders();

// ==================== JWT AUTH ====================
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
    throw new InvalidOperationException("Jwt:Key is not configured. Set Jwt:Key in app settings.");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

// ==================== CORS ====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ==================== SERVICES (DI) ====================
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStockItemService, StockItemService>();
builder.Services.AddScoped<IStockCategoryService, StockCategoryService>();
builder.Services.AddScoped<IStorageLocationService, StorageLocationService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
builder.Services.AddScoped<IWasteService, WasteService>();
builder.Services.AddScoped<IStockCountService, StockCountService>();
builder.Services.AddScoped<IMenuItemService, MenuItemService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<ISupplierStockItemService, SupplierStockItemService>();
builder.Services.AddScoped<IUnitConversionService, UnitConversionService>();
builder.Services.AddScoped<IOrganizationService, OrganizationService>();
builder.Services.AddScoped<IPosConnectionService, PosConnectionService>();

// ==================== SWAGGER ====================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inventory Pro API",
        Version = "v1",
        Description = "Standalone Restaurant Inventory Management System"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Enter: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ==================== MIDDLEWARE ====================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory Pro API v1"));
}

app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// ==================== DATABASE MIGRATION ====================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<InventoryProDbContext>();
    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

// ==================== HELPERS ====================
IResult? RequireOrganization(HttpContext ctx, int routeOrgId)
{
    var claimValue = ctx.User.FindFirst("OrganizationId")?.Value;
    if (claimValue == null || !int.TryParse(claimValue, out var userOrgId) || userOrgId != routeOrgId)
        return Results.Forbid();
    return null;
}

// ==================== API ENDPOINTS ====================

// ---------- Auth ----------
var authGroup = app.MapGroup("/api/auth").WithTags("Auth");

authGroup.MapPost("/register", async (InventoryPro.Application.Dto.Auth.RegisterDto dto, IAuthService authService) =>
{
    var result = await authService.RegisterAsync(dto);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

authGroup.MapPost("/login", async (InventoryPro.Application.Dto.Auth.LoginDto dto, IAuthService authService) =>
{
    var result = await authService.LoginAsync(dto);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

authGroup.MapPost("/refresh", async (InventoryPro.Application.Dto.Auth.RefreshTokenDto dto, IAuthService authService) =>
{
    var result = await authService.RefreshTokenAsync(dto);
    return result.Success ? Results.Ok(result) : Results.Unauthorized();
});

authGroup.MapGet("/me", async (HttpContext ctx, IAuthService authService) =>
{
    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var orgId = int.Parse(ctx.User.FindFirst("OrganizationId")?.Value ?? "0");
    if (userId == null) return Results.Unauthorized();
    var result = await authService.GetCurrentUserAsync(userId, orgId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
}).RequireAuthorization();

// ---------- Stock Items ----------
var stockGroup = app.MapGroup("/api/inventory/stock-items").WithTags("Stock Items").RequireAuthorization();

stockGroup.MapGet("/{orgId:int}", async (int orgId, HttpContext ctx, IStockItemService svc, int page = 1, int pageSize = 20, string? search = null, int? categoryId = null, bool? isActive = null, bool? lowStockOnly = null) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 100);
    var result = await svc.GetStockItemsAsync(orgId, page, pageSize, search, categoryId, isActive, lowStockOnly);
    return Results.Ok(result);
});

stockGroup.MapGet("/{orgId:int}/{stockItemId:int}", async (int orgId, int stockItemId, HttpContext ctx, IStockItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetStockItemAsync(orgId, stockItemId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

stockGroup.MapPost("/{orgId:int}", async (int orgId, InventoryPro.Application.Dto.Inventory.CreateStockItemDto dto, HttpContext ctx, IStockItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
    var result = await svc.CreateStockItemAsync(orgId, dto, userId);
    return result.Success ? Results.Created($"/api/inventory/stock-items/{orgId}/{result.Data?.Id}", result) : Results.BadRequest(result);
});

stockGroup.MapPut("/{orgId:int}/{stockItemId:int}", async (int orgId, int stockItemId, HttpContext ctx, InventoryPro.Application.Dto.Inventory.UpdateStockItemDto dto, IStockItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.UpdateStockItemAsync(orgId, stockItemId, dto);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

stockGroup.MapDelete("/{orgId:int}/{stockItemId:int}", async (int orgId, int stockItemId, HttpContext ctx, IStockItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.DeleteStockItemAsync(orgId, stockItemId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

stockGroup.MapPost("/{orgId:int}/{stockItemId:int}/adjust", async (int orgId, int stockItemId, InventoryPro.Application.Dto.Inventory.StockAdjustmentDto dto, HttpContext ctx, IStockItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
    var result = await svc.AdjustStockAsync(orgId, stockItemId, dto, userId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

stockGroup.MapGet("/{orgId:int}/{stockItemId:int}/movements", async (int orgId, int stockItemId, HttpContext ctx, IStockItemService svc, int page = 1, int pageSize = 20) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 100);
    var result = await svc.GetStockMovementsAsync(orgId, stockItemId, page, pageSize);
    return Results.Ok(result);
});

stockGroup.MapGet("/{orgId:int}/low-stock", async (int orgId, HttpContext ctx, IStockItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetLowStockItemsAsync(orgId);
    return Results.Ok(result);
});

// ---------- Categories ----------
var categoryGroup = app.MapGroup("/api/inventory/categories").WithTags("Categories").RequireAuthorization();

categoryGroup.MapGet("/{orgId:int}", async (int orgId, HttpContext ctx, IStockCategoryService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetCategoriesAsync(orgId);
    return Results.Ok(result);
});

categoryGroup.MapPost("/{orgId:int}", async (int orgId, InventoryPro.Application.Dto.Inventory.CreateStockCategoryDto dto, HttpContext ctx, IStockCategoryService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.CreateCategoryAsync(orgId, dto);
    return result.Success ? Results.Created($"/api/inventory/categories/{orgId}/{result.Data?.Id}", result) : Results.BadRequest(result);
});

categoryGroup.MapPut("/{orgId:int}/{categoryId:int}", async (int orgId, int categoryId, HttpContext ctx, InventoryPro.Application.Dto.Inventory.UpdateStockCategoryDto dto, IStockCategoryService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.UpdateCategoryAsync(orgId, categoryId, dto);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

categoryGroup.MapDelete("/{orgId:int}/{categoryId:int}", async (int orgId, int categoryId, HttpContext ctx, IStockCategoryService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.DeleteCategoryAsync(orgId, categoryId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

// ---------- Storage Locations ----------
var locationGroup = app.MapGroup("/api/inventory/storage-locations").WithTags("Storage Locations").RequireAuthorization();

locationGroup.MapGet("/{orgId:int}", async (int orgId, HttpContext ctx, IStorageLocationService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetLocationsAsync(orgId);
    return Results.Ok(result);
});

locationGroup.MapPost("/{orgId:int}", async (int orgId, InventoryPro.Application.Dto.Inventory.CreateStorageLocationDto dto, HttpContext ctx, IStorageLocationService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.CreateLocationAsync(orgId, dto);
    return result.Success ? Results.Created($"/api/inventory/storage-locations/{orgId}/{result.Data?.Id}", result) : Results.BadRequest(result);
});

locationGroup.MapPut("/{orgId:int}/{locationId:int}", async (int orgId, int locationId, HttpContext ctx, InventoryPro.Application.Dto.Inventory.UpdateStorageLocationDto dto, IStorageLocationService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.UpdateLocationAsync(orgId, locationId, dto);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

locationGroup.MapDelete("/{orgId:int}/{locationId:int}", async (int orgId, int locationId, HttpContext ctx, IStorageLocationService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.DeleteLocationAsync(orgId, locationId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

// ---------- Suppliers ----------
var supplierGroup = app.MapGroup("/api/inventory/suppliers").WithTags("Suppliers").RequireAuthorization();

supplierGroup.MapGet("/{orgId:int}", async (int orgId, HttpContext ctx, ISupplierService svc, string? search = null, bool? isActive = null) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetSuppliersAsync(orgId, search, isActive);
    return Results.Ok(result);
});

supplierGroup.MapGet("/{orgId:int}/{supplierId:int}", async (int orgId, int supplierId, HttpContext ctx, ISupplierService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetSupplierAsync(orgId, supplierId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

supplierGroup.MapPost("/{orgId:int}", async (int orgId, InventoryPro.Application.Dto.Inventory.CreateSupplierDto dto, HttpContext ctx, ISupplierService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.CreateSupplierAsync(orgId, dto);
    return result.Success ? Results.Created($"/api/inventory/suppliers/{orgId}/{result.Data?.Id}", result) : Results.BadRequest(result);
});

supplierGroup.MapPut("/{orgId:int}/{supplierId:int}", async (int orgId, int supplierId, HttpContext ctx, InventoryPro.Application.Dto.Inventory.UpdateSupplierDto dto, ISupplierService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.UpdateSupplierAsync(orgId, supplierId, dto);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

supplierGroup.MapDelete("/{orgId:int}/{supplierId:int}", async (int orgId, int supplierId, HttpContext ctx, ISupplierService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.DeleteSupplierAsync(orgId, supplierId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

// ---------- Alerts ----------
var alertGroup = app.MapGroup("/api/inventory/alerts").WithTags("Alerts").RequireAuthorization();

alertGroup.MapGet("/{orgId:int}", async (int orgId, HttpContext ctx, InventoryProDbContext db, int page = 1, int pageSize = 20) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    page = Math.Max(1, page);
    pageSize = Math.Clamp(pageSize, 1, 100);

    var query = db.StockAlerts
        .Where(a => a.OrganizationId == orgId && !a.IsDismissed)
        .OrderByDescending(a => a.CreatedAt);

    var total = await query.CountAsync();
    var items = await query.Include(a => a.StockItem).Skip((page - 1) * pageSize).Take(pageSize)
        .Select(a => new InventoryPro.Application.Dto.Inventory.StockAlertDto
        {
            Id = a.Id,
            StockItemId = a.StockItemId,
            StockItemName = a.StockItem != null ? a.StockItem.Name : null,
            AlertType = a.AlertType,
            Message = a.Message,
            IsRead = a.IsRead,
            CreatedAt = a.CreatedAt
        }).ToListAsync();

    return Results.Ok(new InventoryPro.Application.Dto.Common.PaginatedResponseDto<InventoryPro.Application.Dto.Inventory.StockAlertDto>
    {
        Items = items, TotalCount = total, Page = page, PageSize = pageSize
    });
});

alertGroup.MapPost("/{orgId:int}/{alertId:int}/read", async (int orgId, int alertId, HttpContext ctx, InventoryProDbContext db) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var alert = await db.StockAlerts.FirstOrDefaultAsync(a => a.Id == alertId && a.OrganizationId == orgId);
    if (alert == null) return Results.NotFound();
    alert.IsRead = true;
    alert.ReadAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok();
});

alertGroup.MapPost("/{orgId:int}/read-all", async (int orgId, HttpContext ctx, InventoryProDbContext db) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    await db.StockAlerts.Where(a => a.OrganizationId == orgId && !a.IsRead)
        .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsRead, true).SetProperty(a => a.ReadAt, DateTime.UtcNow));
    return Results.Ok();
});

alertGroup.MapPost("/{orgId:int}/{alertId:int}/dismiss", async (int orgId, int alertId, HttpContext ctx, InventoryProDbContext db) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var alert = await db.StockAlerts.FirstOrDefaultAsync(a => a.Id == alertId && a.OrganizationId == orgId);
    if (alert == null) return Results.NotFound();
    alert.IsDismissed = true;
    await db.SaveChangesAsync();
    return Results.Ok();
});

// ---------- Purchase Orders ----------
var poGroup = app.MapGroup("/api/inventory/purchase-orders").WithTags("Purchase Orders").RequireAuthorization();

poGroup.MapGet("/{orgId:int}", async (int orgId, HttpContext ctx, IPurchaseOrderService svc, int page = 1, int pageSize = 20, string? status = null, int? supplierId = null) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
    return Results.Ok(await svc.GetPurchaseOrdersAsync(orgId, page, pageSize, status, supplierId));
});

poGroup.MapGet("/{orgId:int}/{orderId:int}", async (int orgId, int orderId, HttpContext ctx, IPurchaseOrderService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetPurchaseOrderAsync(orgId, orderId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

poGroup.MapPost("/{orgId:int}", async (int orgId, InventoryPro.Application.Dto.Inventory.CreatePurchaseOrderDto dto, HttpContext ctx, IPurchaseOrderService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
    var result = await svc.CreatePurchaseOrderAsync(orgId, dto, userId);
    return result.Success ? Results.Created($"/api/inventory/purchase-orders/{orgId}/{result.Data?.Id}", result) : Results.BadRequest(result);
});

poGroup.MapPut("/{orgId:int}/{orderId:int}", async (int orgId, int orderId, HttpContext ctx, InventoryPro.Application.Dto.Inventory.UpdatePurchaseOrderDto dto, IPurchaseOrderService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.UpdatePurchaseOrderAsync(orgId, orderId, dto);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

poGroup.MapPost("/{orgId:int}/{orderId:int}/submit", async (int orgId, int orderId, HttpContext ctx, IPurchaseOrderService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.SubmitPurchaseOrderAsync(orgId, orderId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

poGroup.MapPost("/{orgId:int}/{orderId:int}/cancel", async (int orgId, int orderId, HttpContext ctx, IPurchaseOrderService svc, string? reason = null) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.CancelPurchaseOrderAsync(orgId, orderId, reason);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

poGroup.MapPost("/{orgId:int}/{orderId:int}/receive", async (int orgId, int orderId, InventoryPro.Application.Dto.Inventory.GoodsReceivingDto dto, HttpContext ctx, IPurchaseOrderService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
    var result = await svc.ReceiveGoodsAsync(orgId, orderId, dto, userId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// ---------- Waste ----------
var wasteGroup = app.MapGroup("/api/inventory/waste").WithTags("Waste").RequireAuthorization();

wasteGroup.MapGet("/{orgId:int}", async (int orgId, HttpContext ctx, IWasteService svc, int page = 1, int pageSize = 20, string? reason = null) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
    return Results.Ok(await svc.GetWasteRecordsAsync(orgId, page, pageSize, reason));
});

wasteGroup.MapPost("/{orgId:int}", async (int orgId, InventoryPro.Application.Dto.Inventory.CreateWasteRecordDto dto, HttpContext ctx, IWasteService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
    var result = await svc.CreateWasteRecordAsync(orgId, dto, userId);
    return result.Success ? Results.Created("", result) : Results.BadRequest(result);
});

wasteGroup.MapGet("/{orgId:int}/summary", async (int orgId, HttpContext ctx, IWasteService svc, DateTime? startDate = null, DateTime? endDate = null) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetWasteSummaryAsync(orgId, startDate, endDate);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// ---------- Stock Counts ----------
var countGroup = app.MapGroup("/api/inventory/stock-counts").WithTags("Stock Counts").RequireAuthorization();

countGroup.MapGet("/{orgId:int}", async (int orgId, HttpContext ctx, IStockCountService svc, int page = 1, int pageSize = 20, string? status = null) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
    return Results.Ok(await svc.GetStockCountsAsync(orgId, page, pageSize, status));
});

countGroup.MapGet("/{orgId:int}/{countId:int}", async (int orgId, int countId, HttpContext ctx, IStockCountService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetStockCountAsync(orgId, countId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

countGroup.MapPost("/{orgId:int}", async (int orgId, InventoryPro.Application.Dto.Inventory.CreateStockCountDto dto, HttpContext ctx, IStockCountService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
    var result = await svc.StartCountAsync(orgId, dto, userId);
    return result.Success ? Results.Created("", result) : Results.BadRequest(result);
});

countGroup.MapPut("/{orgId:int}/{countId:int}/items", async (int orgId, int countId, List<InventoryPro.Application.Dto.Inventory.StockCountItemDto> items, HttpContext ctx, IStockCountService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
    var result = await svc.SubmitCountItemsAsync(orgId, countId, items, userId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

countGroup.MapPost("/{orgId:int}/{countId:int}/complete", async (int orgId, int countId, HttpContext ctx, IStockCountService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.CompleteCountAsync(orgId, countId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

countGroup.MapPost("/{orgId:int}/{countId:int}/approve", async (int orgId, int countId, HttpContext ctx, IStockCountService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var userId = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
    var result = await svc.ApproveCountAsync(orgId, countId, userId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// ---------- Menu Items ----------
var menuItemGroup = app.MapGroup("/api/inventory/menu-items").WithTags("Menu Items").RequireAuthorization();

menuItemGroup.MapGet("/{orgId:int}", async (int orgId, HttpContext ctx, IMenuItemService svc, int page = 1, int pageSize = 20, string? search = null, string? category = null, bool? isActive = null) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
    return Results.Ok(await svc.GetMenuItemsAsync(orgId, page, pageSize, search, category, isActive));
});

menuItemGroup.MapGet("/{orgId:int}/{menuItemId:int}", async (int orgId, int menuItemId, HttpContext ctx, IMenuItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetMenuItemAsync(orgId, menuItemId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

menuItemGroup.MapPost("/{orgId:int}", async (int orgId, InventoryPro.Application.Dto.Inventory.CreateMenuItemDto dto, HttpContext ctx, IMenuItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.CreateMenuItemAsync(orgId, dto);
    return result.Success ? Results.Created($"/api/inventory/menu-items/{orgId}/{result.Data?.Id}", result) : Results.BadRequest(result);
});

menuItemGroup.MapPut("/{orgId:int}/{menuItemId:int}", async (int orgId, int menuItemId, HttpContext ctx, InventoryPro.Application.Dto.Inventory.UpdateMenuItemDto dto, IMenuItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.UpdateMenuItemAsync(orgId, menuItemId, dto);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

menuItemGroup.MapDelete("/{orgId:int}/{menuItemId:int}", async (int orgId, int menuItemId, HttpContext ctx, IMenuItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.DeleteMenuItemAsync(orgId, menuItemId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

// ---------- Recipes ----------
var recipeGroup = app.MapGroup("/api/inventory/recipes").WithTags("Recipes").RequireAuthorization();

recipeGroup.MapGet("/{orgId:int}/menu-item/{menuItemId:int}", async (int orgId, int menuItemId, HttpContext ctx, IRecipeService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetRecipeAsync(orgId, menuItemId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

recipeGroup.MapPost("/{orgId:int}/menu-item/{menuItemId:int}", async (int orgId, int menuItemId, List<InventoryPro.Application.Dto.Inventory.RecipeMappingDto> mappings, HttpContext ctx, IRecipeService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.SetRecipeMappingsAsync(orgId, menuItemId, mappings);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

recipeGroup.MapDelete("/{orgId:int}/mapping/{mappingId:int}", async (int orgId, int mappingId, HttpContext ctx, IRecipeService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.DeleteMappingAsync(orgId, mappingId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

recipeGroup.MapGet("/{orgId:int}/stock-item/{stockItemId:int}/menu-items", async (int orgId, int stockItemId, HttpContext ctx, IRecipeService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetMenuItemsUsingStockItemAsync(orgId, stockItemId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// ---------- Reports ----------
var reportGroup = app.MapGroup("/api/inventory/reports").WithTags("Reports").RequireAuthorization();

reportGroup.MapGet("/{orgId:int}/stock-levels", async (int orgId, HttpContext ctx, InventoryProDbContext db, int? categoryId = null, int? storageLocationId = null, bool? lowStockOnly = null) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var query = db.StockItems.Where(s => s.OrganizationId == orgId && s.IsActive);
    if (categoryId.HasValue) query = query.Where(s => s.CategoryId == categoryId.Value);
    if (storageLocationId.HasValue) query = query.Where(s => s.PrimaryStorageLocationId == storageLocationId.Value);
    if (lowStockOnly == true) query = query.Where(s => s.CurrentQuantity <= s.MinimumThreshold);

    var items = await query.Include(s => s.Category).Include(s => s.PrimarySupplier)
        .OrderBy(s => s.Name)
        .Select(s => new
        {
            s.Id,
            s.Name,
            s.SKU,
            CategoryName = s.Category != null ? s.Category.Name : null,
            s.CurrentQuantity,
            s.MinimumThreshold,
            s.ParLevel,
            s.MaximumCapacity,
            s.CostPrice,
            s.AverageCostPrice,
            Value = s.CurrentQuantity * s.AverageCostPrice,
            s.BaseUnitOfMeasurement,
            s.IsPerishable,
            SupplierName = s.PrimarySupplier != null ? s.PrimarySupplier.Name : null,
            Status = s.CurrentQuantity <= 0 ? "OutOfStock" : s.CurrentQuantity <= s.MinimumThreshold ? "Low" : s.ParLevel.HasValue && s.CurrentQuantity < s.ParLevel.Value ? "BelowPar" : "InStock"
        }).ToListAsync();

    var totalValue = items.Sum(i => i.Value);
    return Results.Ok(new
    {
        Items = items,
        TotalValue = totalValue,
        TotalItems = items.Count,
        LowStockCount = items.Count(i => i.Status == "Low"),
        OutOfStockCount = items.Count(i => i.Status == "OutOfStock"),
        BelowParCount = items.Count(i => i.Status == "BelowPar")
    });
});

reportGroup.MapGet("/{orgId:int}/movements", async (int orgId, HttpContext ctx, InventoryProDbContext db, int page = 1, int pageSize = 20, int? stockItemId = null, int? movementType = null, DateTime? startDate = null, DateTime? endDate = null) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    page = Math.Max(1, page); pageSize = Math.Clamp(pageSize, 1, 100);
    var query = db.StockMovements.Where(m => m.OrganizationId == orgId);
    if (stockItemId.HasValue) query = query.Where(m => m.StockItemId == stockItemId.Value);
    if (movementType.HasValue) query = query.Where(m => (int)m.MovementType == movementType.Value);
    if (startDate.HasValue) query = query.Where(m => m.CreatedAt >= startDate.Value);
    if (endDate.HasValue) query = query.Where(m => m.CreatedAt <= endDate.Value);
    var total = await query.CountAsync();
    var items = await query.Include(m => m.StockItem).OrderByDescending(m => m.CreatedAt)
        .Skip((page - 1) * pageSize).Take(pageSize)
        .Select(m => new { m.Id, m.StockItemId, StockItemName = m.StockItem != null ? m.StockItem.Name : null,
            MovementType = (int)m.MovementType, m.Quantity, m.PreviousQuantity, m.NewQuantity,
            m.CostPerUnit, m.TotalCost, m.Reason, m.ReferenceType, m.ReferenceId, m.BatchNumber,
            m.ExpirationDate, m.CreatedAt, m.CreatedBy, m.Notes
        }).ToListAsync();
    return Results.Ok(new { Items = items, TotalCount = total, Page = page, PageSize = pageSize });
});

reportGroup.MapGet("/{orgId:int}/stock-levels/csv", async (int orgId, HttpContext ctx, InventoryProDbContext db) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var items = await db.StockItems.Where(s => s.OrganizationId == orgId && s.IsActive)
        .Include(s => s.Category).Include(s => s.PrimarySupplier)
        .OrderBy(s => s.Name)
        .Select(s => new { s.Name, s.SKU, Category = s.Category != null ? s.Category.Name : "",
            s.CurrentQuantity, s.MinimumThreshold, s.ParLevel, s.CostPrice, s.AverageCostPrice,
            Value = s.CurrentQuantity * s.AverageCostPrice,
            Supplier = s.PrimarySupplier != null ? s.PrimarySupplier.Name : "",
            Status = s.CurrentQuantity <= 0 ? "OutOfStock" : s.CurrentQuantity <= s.MinimumThreshold ? "Low" : "InStock"
        }).ToListAsync();
    var csv = "Name,SKU,Category,Current Qty,Min Threshold,PAR Level,Cost Price,Avg Cost,Value,Supplier,Status\n";
    foreach (var i in items)
        csv += $"\"{i.Name}\",\"{i.SKU}\",\"{i.Category}\",{i.CurrentQuantity},{i.MinimumThreshold},{i.ParLevel},{i.CostPrice},{i.AverageCostPrice},{i.Value},\"{i.Supplier}\",{i.Status}\n";
    return Results.Text(csv, "text/csv");
});

reportGroup.MapGet("/{orgId:int}/dashboard", async (int orgId, HttpContext ctx, InventoryProDbContext db) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var totalItems = await db.StockItems.CountAsync(s => s.OrganizationId == orgId && s.IsActive);
    var lowStock = await db.StockItems.CountAsync(s => s.OrganizationId == orgId && s.IsActive && s.CurrentQuantity > 0 && s.CurrentQuantity <= s.MinimumThreshold);
    var outOfStock = await db.StockItems.CountAsync(s => s.OrganizationId == orgId && s.IsActive && s.CurrentQuantity <= 0);
    var belowPar = await db.StockItems.CountAsync(s => s.OrganizationId == orgId && s.IsActive && s.ParLevel.HasValue && s.CurrentQuantity < s.ParLevel.Value && s.CurrentQuantity > s.MinimumThreshold);
    var totalValue = await db.StockItems.Where(s => s.OrganizationId == orgId && s.IsActive).SumAsync(s => s.CurrentQuantity * s.AverageCostPrice);
    var pendingOrders = await db.PurchaseOrders.CountAsync(p => p.OrganizationId == orgId && (p.Status == InventoryPro.Domain.Enums.PurchaseOrderStatus.Submitted || p.Status == InventoryPro.Domain.Enums.PurchaseOrderStatus.Confirmed));
    var recentMovements = await db.StockMovements.Where(m => m.OrganizationId == orgId)
        .OrderByDescending(m => m.CreatedAt).Take(5).Include(m => m.StockItem)
        .Select(m => new { m.Id, StockItemName = m.StockItem != null ? m.StockItem.Name : null, MovementType = (int)m.MovementType, m.Quantity, m.CreatedAt }).ToListAsync();
    var wasteThisMonth = await db.WasteRecords.Where(w => w.OrganizationId == orgId && w.RecordedAt >= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1)).SumAsync(w => (decimal?)w.TotalCost) ?? 0;
    var alertCount = await db.StockAlerts.CountAsync(a => a.OrganizationId == orgId && !a.IsRead && !a.IsDismissed);

    return Results.Ok(new {
        TotalItems = totalItems, LowStock = lowStock, OutOfStock = outOfStock, BelowPar = belowPar,
        TotalInventoryValue = totalValue, PendingOrders = pendingOrders, RecentMovements = recentMovements,
        WasteThisMonth = wasteThisMonth, UnreadAlerts = alertCount
    });
});

// ---------- Supplier Stock Items ----------
var supStockGroup = app.MapGroup("/api/inventory/supplier-stock-items").WithTags("Supplier Stock Items").RequireAuthorization();

supStockGroup.MapGet("/{orgId:int}/by-supplier/{supplierId:int}", async (int orgId, int supplierId, HttpContext ctx, ISupplierStockItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetItemsBySupplierAsync(orgId, supplierId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

supStockGroup.MapGet("/{orgId:int}/by-item/{stockItemId:int}", async (int orgId, int stockItemId, HttpContext ctx, ISupplierStockItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetSuppliersByItemAsync(orgId, stockItemId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

supStockGroup.MapPost("/{orgId:int}", async (int orgId, InventoryPro.Application.Dto.Inventory.CreateSupplierStockItemDto dto, HttpContext ctx, ISupplierStockItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.LinkAsync(orgId, dto);
    return result.Success ? Results.Created("", result) : Results.BadRequest(result);
});

supStockGroup.MapPut("/{orgId:int}/{linkId:int}", async (int orgId, int linkId, HttpContext ctx, InventoryPro.Application.Dto.Inventory.UpdateSupplierStockItemDto dto, ISupplierStockItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.UpdateAsync(orgId, linkId, dto);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

supStockGroup.MapDelete("/{orgId:int}/{linkId:int}", async (int orgId, int linkId, HttpContext ctx, ISupplierStockItemService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.UnlinkAsync(orgId, linkId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

// ---------- Unit Conversions ----------
var convGroup = app.MapGroup("/api/inventory/unit-conversions").WithTags("Unit Conversions").RequireAuthorization();

convGroup.MapGet("/{orgId:int}", async (int orgId, HttpContext ctx, IUnitConversionService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetConversionsAsync(orgId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

convGroup.MapPost("/{orgId:int}", async (int orgId, InventoryPro.Application.Dto.Inventory.CreateUnitConversionDto dto, HttpContext ctx, IUnitConversionService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.CreateConversionAsync(orgId, dto);
    return result.Success ? Results.Created("", result) : Results.BadRequest(result);
});

convGroup.MapDelete("/{orgId:int}/{conversionId:int}", async (int orgId, int conversionId, HttpContext ctx, IUnitConversionService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.DeleteConversionAsync(orgId, conversionId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

convGroup.MapGet("/{orgId:int}/convert", async (int orgId, int fromUnit, int toUnit, decimal quantity, HttpContext ctx, IUnitConversionService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.ConvertAsync(orgId, fromUnit, toUnit, quantity);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// ---------- Organization Settings ----------
var orgGroup = app.MapGroup("/api/inventory/organization").WithTags("Organization").RequireAuthorization();

orgGroup.MapGet("/{orgId:int}", async (int orgId, HttpContext ctx, IOrganizationService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetOrganizationAsync(orgId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

orgGroup.MapPut("/{orgId:int}", async (int orgId, InventoryPro.Application.Dto.Inventory.UpdateOrganizationDto dto, HttpContext ctx, IOrganizationService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.UpdateOrganizationAsync(orgId, dto);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

// ---------- POS Connections ----------
var posGroup = app.MapGroup("/api/inventory/pos-connections").WithTags("POS Connections").RequireAuthorization();

posGroup.MapGet("/{orgId:int}", async (int orgId, HttpContext ctx, IPosConnectionService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.GetConnectionsAsync(orgId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

posGroup.MapPost("/{orgId:int}", async (int orgId, InventoryPro.Application.Dto.Inventory.CreatePosConnectionDto dto, HttpContext ctx, IPosConnectionService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.CreateConnectionAsync(orgId, dto);
    return result.Success ? Results.Created("", result) : Results.BadRequest(result);
});

posGroup.MapPost("/{orgId:int}/{connectionId:int}/toggle", async (int orgId, int connectionId, HttpContext ctx, IPosConnectionService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.ToggleConnectionAsync(orgId, connectionId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

posGroup.MapDelete("/{orgId:int}/{connectionId:int}", async (int orgId, int connectionId, HttpContext ctx, IPosConnectionService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.DeleteConnectionAsync(orgId, connectionId);
    return result.Success ? Results.Ok(result) : Results.NotFound(result);
});

posGroup.MapPost("/{orgId:int}/{connectionId:int}/regenerate-key", async (int orgId, int connectionId, HttpContext ctx, IPosConnectionService svc) =>
{
    if (RequireOrganization(ctx, orgId) is { } err) return err;
    var result = await svc.RegenerateApiKeyAsync(orgId, connectionId);
    return result.Success ? Results.Ok(result) : Results.BadRequest(result);
});

app.Run();
