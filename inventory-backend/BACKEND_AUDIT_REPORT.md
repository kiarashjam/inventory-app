# Backend Service Implementation – Audit Report

**Base path:** `inventory\inventory-backend\`  
**Date:** 2025-02-09

---

## Summary

This report lists bugs, missing logic, side effects, and security issues found in the listed backend files. Items are grouped by severity and by file where applicable.

---

## CRITICAL – Security & Data Integrity

### 1. **Program.cs – No organization-scoped authorization (all inventory endpoints)**

- **Issue:** Endpoints take `orgId` from the route and trust it. There is no check that the authenticated user belongs to that organization.
- **Impact:** Any authenticated user can read or modify data of any organization by changing `orgId` in the URL (e.g. `GET /api/inventory/stock-items/999`, `GET /api/inventory/alerts/999`).
- **Location:** All groups: `stockGroup`, `categoryGroup`, `locationGroup`, `supplierGroup`, `alertGroup`.
- **Fix:** Resolve the user’s organization from claims (e.g. `OrganizationId`) and either:
  - Use that value instead of the route `orgId`, or
  - Validate that the route `orgId` equals the user’s `OrganizationId` and return 403 if not.

### 2. **AuthService.cs – Register: user created without transaction**

- **Issue:** User is created with `_userManager.CreateAsync`, then organization and `OrganizationUser` are added and saved in two separate `SaveChangesAsync` calls. If the second save fails, the user exists in the DB without an organization (orphan user).
- **Location:** `RegisterAsync` (lines 45–73).
- **Fix:** Run user create + organization create + `OrganizationUser` create inside a single transaction (e.g. `DbContext.Database.BeginTransactionAsync()` and commit after all steps succeed).

### 3. **AuthService.cs – JWT key null risk**

- **Issue:** `_configuration["Jwt:Key"]!` and `Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)` are used without null check. If `Jwt:Key` is missing, `GetBytes(null)` throws.
- **Location:** `GenerateTokens` (line 195), `GetPrincipalFromExpiredToken` (line 230). Program.cs uses `builder.Configuration["Jwt:Key"]!` (line 31) with the same risk at startup.
- **Fix:** Validate that `Jwt:Key` is non-null and non-empty at startup and in these methods; throw a clear configuration exception or return a failure response instead of NRE.

---

## HIGH – Logic & Correctness

### 4. **StockCategoryService.cs – GetCategoryByIdAsync can return null; callers ignore it**

- **Issue:** `GetCategoryByIdAsync` returns `null!` when the category is not found (line 183). Callers `CreateCategoryAsync` and `UpdateCategoryAsync` pass the result to `ServiceResponseDto.Ok(result)` without checking for null.
- **Impact:** After create/update, API can return `Success: true` with `Data: null` in edge cases (e.g. race, or if a different context were used).
- **Location:** `GetCategoryByIdAsync` (lines 175–194); call sites at lines 97–98 and 144–145.
- **Fix:** Have `GetCategoryByIdAsync` return `Task<StockCategoryDto?>` and handle null in callers (e.g. return `Fail("Category not found")` or retry/throw) instead of returning `Ok(null)`.

### 5. **StockItemService.cs – CreateLowStockAlertAsync: alert message not interpolated**

- **Issue:** The alert message is built with string interpolation, but the **update** path uses the same string as the **create** path. In both cases the string is:
  `$"{stockItemName} is low on stock. Current: {currentQuantity}, Minimum: {minimumThreshold}"`
  So interpolation is correct. Re-reading the code: it is correct. **Withdrawing this as an issue.**

- **Re-check:**  
  - Create: `Message = $"{stockItemName} is low on stock. Current: {currentQuantity}, Minimum: {minimumThreshold}"`  
  - Update: `existingAlert.Message = $"{stockItemName} is low on stock. Current: {currentQuantity}, Minimum: {minimumThreshold}"`  
  Both use interpolation. No bug here.

### 6. **Program.cs – Alerts list: StockItem not loaded, StockItemName always null**

- **Issue:** The alerts query does not `.Include(a => a.StockItem)`. Lazy loading is not enabled, so `a.StockItem` is null in the projection and `StockItemName` is always null in the response.
- **Location:** `alertGroup.MapGet("/{orgId:int}", ...)` (lines 310–325).
- **Fix:** Add `.Include(a => a.StockItem)` (or equivalent) before the projection so `StockItemName` is populated.

### 7. **PaginatedResponseDto.cs – Division by zero when PageSize is 0**

- **Issue:** `TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize)` throws if `PageSize` is 0.
- **Location:** Line 9.
- **Fix:** Use a safe divisor, e.g. `PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize)`, and/or validate `page` and `pageSize` in the API (e.g. enforce `page >= 1`, `pageSize >= 1` or `pageSize > 0`).

### 8. **Program.cs & services – No validation of page / pageSize**

- **Issue:** Endpoints and services accept `page` and `pageSize` without validation. `pageSize = 0` leads to division by zero in `PaginatedResponseDto.TotalPages`. `page < 1` can lead to negative `Skip` (e.g. `Skip((0 - 1) * 20)` = `Skip(-20)`) and argument exception.
- **Location:** Program.cs: stock items list, movements, alerts; corresponding service methods.
- **Fix:** Validate and clamp (e.g. `page = Math.Max(1, page)`, `pageSize = Math.Clamp(pageSize, 1, 100)`) in the API or in a shared helper before building `PaginatedResponseDto`.

### 9. **AuthService.cs – GetCurrentUserAsync does not filter by IsActive for organization**

- **Issue:** The query uses `ou.UserId == userId && ou.OrganizationId == organizationId` but does not require `ou.IsActive`. A user deactivated for an organization could still get “current user” for that org.
- **Location:** `GetCurrentUserAsync` (lines 184–185).
- **Fix:** Add `&& ou.IsActive` to the predicate for consistency with Login/RefreshToken.

### 10. **RefreshTokenAsync – OrganizationId claim missing**

- **Issue:** `int.Parse(principal.FindFirst("OrganizationId")?.Value ?? "0")` uses `"0"` when the claim is missing. Lookup then fails with “Invalid organization context,” which is correct, but the error message could be clearer (e.g. “Missing or invalid organization in token”).
- **Severity:** Low (behavior is safe; messaging can be improved).

---

## MEDIUM – Consistency & Validation

### 11. **StockItemService.cs – UpdateStockItemAsync does not update CurrentQuantity**

- **Note:** This is by design; quantity is changed via `AdjustStockAsync`. Confirmed that `UpdateStockItemDto` does not include `CurrentQuantity`. No change needed.

### 12. **StockItemService.cs – AdjustStockAsync allows negative stock**

- **Issue:** Comment states “Allow negative quantities for tracking.” There is no validation or warning when `newQuantity < 0`. Business may want to disallow or require a reason/cap.
- **Location:** `AdjustStockAsync` (lines 307–312).
- **Fix:** Either document the decision, or add validation (e.g. reject when `newQuantity < 0` or allow only with a specific reason/role).

### 13. **CreateStockItemDto / CreateStockItemAsync – No business validation**

- **Issue:** No validation that `MinimumThreshold >= 0`, `ParLevel >= 0`, `MaximumCapacity > 0`, or `CurrentQuantity >= 0`. Negative or inconsistent values can be stored.
- **Fix:** Add validation in the service (or via FluentValidation) and return a clear error when invalid.

### 14. **StorageLocationService.cs – DeleteLocationAsync allows delete when location has stock**

- **Issue:** When `hasStockAtLocation` is true (stock in `StockItemStorageLocations`), the code only has a comment (“Optionally: return error”) and still performs soft delete.
- **Location:** Lines 129–136.
- **Fix:** Either block deletion when `hasStockAtLocation` and return an error, or document that deletion is allowed and what happens to that stock.

### 15. **SupplierService.cs – DeleteSupplierAsync allows soft delete when purchase orders exist**

- **Issue:** When `hasPurchaseOrders` is true, the code comments “Optionally: return error” but still soft-deletes the supplier.
- **Location:** Lines 189–196.
- **Fix:** Either block or restrict deletion when purchase orders exist, or document the behavior.

### 16. **SupplierService.cs – Swallowing exception in email validation**

- **Issue:** `new System.Net.Mail.MailAddress(dto.Email)` is used in a try-catch; the catch block returns “Invalid email format” but does not log. In debug scenarios, the exception type could be useful.
- **Location:** Create and Update (lines 89–97, 143–151).
- **Fix:** Consider logging the exception (at least in development) while still returning a user-friendly message.

---

## LOW – DTOs, Mapping & Minor

### 17. **StockAlertDto vs entity**

- **Issue:** `StockAlert` has `IsDismissed` and `ReadAt`; `StockAlertDto` does not. Alerts list filters by `!a.IsDismissed` but the DTO does not expose dismissal state. This may be intentional for the list view.
- **Recommendation:** If clients need to show “dismissed” or “read at” in the UI, add optional properties to the DTO; otherwise document that the list is non-dismissed only.

### 18. **AuthService – Login and multiple organizations**

- **Issue:** `FirstOrDefaultAsync(ou => ou.UserId == user.Id && ou.IsActive)` returns the first active org. A user in multiple organizations cannot choose which org to log into.
- **Impact:** Product/UX decision; may be intentional for a single-org-per-login model.
- **Recommendation:** If multi-org is required, consider returning a list of orgs and a separate “select org” or “switch org” step.

### 19. **Program.cs – Login response body on failure**

- **Issue:** On login failure the endpoint returns `Results.Unauthorized()` with no body, while register returns `Results.BadRequest(result)` with the service result. Inconsistent and harder for clients to show a specific error message.
- **Location:** Line 147.
- **Fix:** Consider returning `Results.Unauthorized()` with a body that includes the failure message (or a standard error DTO) for consistency and client UX.

### 20. **ServiceResponseDto&lt;T&gt; – Data can be null on success**

- **Issue:** `ServiceResponseDto.Ok(T data)` sets `Data = data`. If callers ever pass `null` (e.g. from the `GetCategoryByIdAsync` case above), clients get `Success: true` and `Data: null`. The generic type allows `T?`.
- **Recommendation:** Either document that “Ok” must never be used with null data, or add an overload/guard that rejects null for `Ok`.

---

## Concurrency & Transactions

### 21. **No explicit transaction boundaries**

- **Observation:** Multi-step operations (e.g. AuthService Register, StockItemService create + alert, adjust + movement + alert) are not wrapped in explicit transactions. If a step fails after a previous `SaveAsync`, the DB can be left in a partial state.
- **Recommendation:** Use `DbContext.Database.BeginTransactionAsync()` (and commit/rollback) for any flow that must be all-or-nothing (especially Register and any “create + create related” flows).

### 22. **StockItem.RowVersion not used**

- **Observation:** `StockItem` has `[Timestamp] RowVersion` but services do not use it for optimistic concurrency. Concurrent updates could overwrite each other.
- **Recommendation:** If concurrency is a requirement, configure EF to use `RowVersion` for concurrency and handle `DbUpdateConcurrencyException` in the API.

---

## OrganizationId Filtering (Security)

- **Services:** All inventory services correctly filter by `orgId` in queries (`Where(s => s.OrganizationId == orgId)` or equivalent). So **within** a request, data is scoped to the given org.
- **Gap:** The org id itself is taken from the route and not validated against the current user’s organization (see Critical #1). So organization scoping in the service layer is correct; the missing piece is enforcing “user can only use their own org” at the API layer.

---

## LINQ & N+1

- **StockItemService:** Uses `.Include(s => s.Category)`, `.Include(s => s.PrimarySupplier)`, `.Include(s => s.PrimaryStorageLocation)` where needed; projections use `s.Category.Name` etc. after Include. No N+1 found.
- **StockCategoryService:** Uses `.Include(c => c.ParentCategory)` and `.Include(c => c.SubCategories)` where needed. No N+1 found.
- **Program.cs alerts:** Missing `.Include(a => a.StockItem)` (see High #6).

---

## Checklist Summary

| Check | Result |
|-------|--------|
| Null reference risks | JWT key (Critical #3); GetCategoryByIdAsync null return (High #4); others guarded or non-null. |
| Logic errors | Alert message (withdrawn); PaginatedResponseDto division by zero (High #7); alerts StockItemName (High #6). |
| Error handling | Register not transactional (Critical #2); no try-catch needed elsewhere if validation is done. |
| DTOs vs service returns | StockAlertDto missing IsDismissed/ReadAt (Low #17); ServiceResponseDto.Ok(null) possible (High #4, Low #20). |
| Concurrency | No transactions (Concurrency #21); RowVersion unused (Concurrency #22). |
| Validations | page/pageSize (High #8); negative stock (Medium #12); CreateStockItem thresholds (Medium #13). |
| Entity–DTO mapping | Mapping is correct; alerts need Include for StockItemName (High #6). |
| LINQ / N+1 | Alerts missing Include (High #6); services use Include appropriately. |
| OrganizationId filtering | Services filter by orgId; API does not validate orgId vs user (Critical #1). |
| Program.cs registrations | Services and DbContext registered correctly. |
| Program.cs endpoints | HTTP methods and routes are correct; low-stock route is distinct and matches. |
| Program.cs authorization | RequireAuthorization() present; org-scoped authorization missing (Critical #1). |
| JWT configuration | Key can be null (Critical #3); issuer/audience used in Program and token generation. |

---

## Recommended Priority

1. **Immediate:** Enforce organization-scoped authorization (Critical #1), fix Register transaction (Critical #2), validate JWT key (Critical #3).
2. **Short term:** Fix GetCategoryByIdAsync null handling (High #4), add Include for alerts StockItem (High #6), fix PaginatedResponseDto and page/pageSize validation (High #7, #8), add IsActive filter in GetCurrentUserAsync (High #9).
3. **Next:** Add validations for stock and DTOs (Medium #12–13), clarify delete behavior for locations/suppliers (Medium #14–15), consider transactions and RowVersion (Concurrency #21–22).
4. **As needed:** Login body on failure (Low #19), multi-org login behavior (Low #18), DTO and response consistency (Low #17, #20).
