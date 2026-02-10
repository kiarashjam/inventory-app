# Inventory Pro - Backend Development Report

**Generated:** February 9, 2026 (Updated)  
**Project:** Inventory Pro — Standalone Restaurant Inventory Management System  
**Framework:** .NET 8.0 | ASP.NET Core  
**Architecture:** Clean Architecture (standalone project — NOT part of BonApp)  
**Repository:** `inventory-backend` (separate Git repository)  
**Database:** Own SQL Server database (`InventoryProDb`)  
**Status:** Phase 2 In Progress

---

## IMPLEMENTATION STATUS (Updated Feb 9, 2026)

### ✅ Completed (Phase 1 + Phase 2 Core)

| Component | Status | Notes |
|-----------|--------|-------|
| **Clean Architecture** | ✅ Done | 5 projects: Api, Domain, Application, Infrastructure, DataAccess |
| **38 Domain Entities** | ✅ Done | All entities created with navigation properties |
| **DbContext** | ✅ Done | All entities configured, unique indexes, FK configs, allergen seeds |
| **ASP.NET Identity + JWT** | ✅ Done | Register, Login, Refresh, Me endpoints |
| **Organization-scoped auth** | ✅ Done | JWT claims validation, RequireOrganization helper |
| **StockItem CRUD** | ✅ Done | Service + endpoints (paginated, searchable, filterable) |
| **StockCategory CRUD** | ✅ Done | Service + endpoints |
| **StorageLocation CRUD** | ✅ Done | Service + endpoints |
| **Supplier CRUD** | ✅ Done | Service + endpoints with search |
| **Stock Adjustments** | ✅ Done | Adjust stock with movement trail |
| **Stock Movement History** | ✅ Done | Paginated movement history per item |
| **Low Stock Items** | ✅ Done | Endpoint returns items below threshold |
| **Alert System** | ✅ Done | List, mark read, mark all read, dismiss |
| **Purchase Orders** | ✅ Done | Full lifecycle: Create, Update (draft), Submit, Cancel, Receive Goods |
| **Goods Receiving** | ✅ Done | Creates GR records, updates stock, creates movements |
| **Waste Recording** | ✅ Done | Create, list (paginated), summary with aggregation |
| **Stock Counts** | ✅ Done | Start, submit items, complete, approve with corrections |
| **Pagination Validation** | ✅ Done | Clamp page >= 1, pageSize 1-100 |
| **JWT Key Validation** | ✅ Done | Startup check with clear error |
| **Transaction Safety** | ✅ Done | Registration wrapped in transaction |

### ❌ Not Yet Implemented (Remaining Phases)

| Component | Phase | Priority |
|-----------|-------|----------|
| **Recipe/BOM System** (MenuItemStockMapping CRUD) | Phase 2 | P0 |
| **CSV Import/Export** | Phase 2 | P1 |
| **External POS API** (`/api/pos/v1/`) | Phase 2 | P0 |
| **POS Connection Management** | Phase 2 | P0 |
| **Stock Deduction Service** (auto-deduct on sale) | Phase 2 | P0 |
| **Auto-Reorder Suggestions** (PAR-based) | Phase 3 | P1 |
| **Supplier Stock Item Linking** | Phase 3 | P1 |
| **Storage Location Stock Tracking** (per-location qty) | Phase 3 | P1 |
| **Stock Transfers** between locations | Phase 3 | P1 |
| **Unit Conversion System** | Phase 3 | P1 |
| **Forgot/Reset Password** | Phase 3 | P1 |
| **User Invitation System** | Phase 3 | P1 |
| **Inventory Snapshots** | Phase 4 | P1 |
| **All Reports** (13 report types) | Phase 5 | P0-P1 |
| **HACCP Module** | Phase 7 | P2 |
| **Allergen/Nutrition Management** | Phase 7 | P1 |
| **Prep Lists** | Phase 8 | P2 |
| **Webhook System** (inbound + outbound) | Phase 6 | P1 |
| **Background Services** (7 services) | Phase 4-8 | P1-P2 |
| **FluentValidation Validators** | Phase 3 | P1 |
| **Role-based Authorization** (Owner/Manager/Staff) | Phase 3 | P1 |
| **Concurrency Middleware** | Phase 3 | P1 |
| **Rate Limiting Middleware** | Phase 6 | P1 |
| **Email Integration** | Phase 4 | P2 |
| **PDF Generation** | Phase 5 | P2 |

### Backend Endpoint Count
- **Implemented:** ~40 endpoints (Auth: 4, Stock Items: 8, Categories: 4, Storage Locations: 4, Suppliers: 5, Alerts: 4, Purchase Orders: 7, Waste: 3, Stock Counts: 6)
- **Remaining:** ~70+ endpoints (Recipes: 5, Reports: 13, POS API: 10, POS Connections: 7, Food Safety: 7, HACCP: 8, Prep Lists: 5, Storage Location stock: 2, Supplier stock items: 2, CSV import/export: 2, Auth additional: 4)

---

## 1. Executive Summary

This report defines the complete backend development plan for **Inventory Pro**, a fully **autonomous, standalone** restaurant inventory management system. This is an **independent application** — it has its own codebase, database, authentication, and deployment. It does NOT live inside the BonApp backend.

The application is designed for any restaurant or hospitality business, regardless of which POS system they use. It works with:

- **BonApp POS** — Connected via REST API / webhooks (future integration)
- **Lightspeed, Toast, Square, Orderbird** — Connected via REST API / webhooks
- **Any POS system** — Connected via the documented External POS API (`/api/pos/v1/`)
- **No POS at all** — Standalone inventory management with manual data entry, CSV imports

### Architectural Independence

This system is **completely decoupled** from BonApp:
- **Own database** — `InventoryProDb` with its own schema (no shared tables)
- **Own authentication** — ASP.NET Identity with JWT Bearer tokens (independent user accounts)
- **Own entities** — `Organization` (restaurant), `MenuItem` (menu products), `AppUser` (users) — NOT BonApp's `YumYumYard`, `Product`, `ApplicationUser`
- **Own deployment** — Separate Docker container, separate Azure App Service, separate CI/CD pipeline
- **API-first integration** — Any future connection to BonApp or other POS systems happens exclusively through the External POS API endpoints (`/api/pos/v1/`) using API keys

### Future BonApp Integration (Phase 2)

When the time comes to connect Inventory Pro to BonApp, it will happen through:
1. BonApp registers as an External POS Connection in Inventory Pro (gets an API key)
2. BonApp calls `POST /api/pos/v1/sale` when an order is confirmed (stock deduction)
3. BonApp calls `POST /api/pos/v1/sale` with negative quantity on refund (stock revert)
4. Inventory Pro calls BonApp's webhook URL when stock levels change (outbound webhook)
5. **No shared database. No shared code. No shared authentication. API calls only.**

---

## 2. Core Features to Build

### 2.1 Stock Management (CRUD + Tracking)

| Feature | Description | Priority |
|---------|-------------|----------|
| **Stock Items Registry** | Manage a master list of all inventory items (ingredients, supplies, packaging, beverages, etc.) — separate from menu Products. Each stock item has a name, SKU, category, unit of measurement, current quantity, PAR level, cost price, and supplier info. | P0 |
| **Product-to-Stock Mapping (Recipes/BOM)** | Link menu Products to their constituent stock items with quantities. E.g., "Margherita Pizza" requires 200g flour, 100g mozzarella, 50ml tomato sauce. This enables automatic stock deduction when orders are placed. Real-time recipe costing that recalculates when ingredient prices change (industry standard: MarketMan, Apicbase). | P0 |
| **PAR Level Management** | Dynamic PAR (Periodic Automatic Replenishment) levels per stock item — the ideal stock quantity to have on hand. PAR is calculated from average daily consumption × lead time + safety stock. When stock drops below PAR, auto-generate reorder suggestions. PAR levels can be set per day-of-week for seasonal/weekly patterns. | P0 |
| **Perpetual Inventory System** | Real-time running inventory that auto-updates with every POS sale, purchase receipt, waste entry, and adjustment — no manual counting needed for day-to-day tracking. Physical counts become periodic verification (weekly/monthly) rather than the primary tracking method. Industry standard at MarketMan, Apicbase, CrunchTime. | P0 |
| **Stock Adjustments** | Manual stock increase/decrease with reason codes (Received, Damaged, Expired, Theft, Correction, Transfer, Returned, Over-portioning). Every adjustment creates an audit trail entry. Supports WEPT classification (Waste, Errors, Portioning, Theft) for variance analysis. | P0 |
| **Storage Location Tracking** | Track stock by physical storage locations within a restaurant (Walk-in Cooler, Dry Storage, Bar, Wine Cellar, Freezer, Prep Station, etc.). Each stock item can be assigned to one or more storage locations with per-location quantities. Supports multi-location restaurants with inter-location transfers. | P0 |
| **Unit Conversion System** | Define conversion factors between units of measurement. E.g., 1 Case = 24 Bottles, 1 kg = 1000 g, 1 Gallon = 3.785 Liters. Allows purchasing in cases but consuming in individual units. Supports Alternate Count Units (ACUs) — staff can count in cases/cans/bottles rather than converting to base units (CrunchTime best practice). | P0 |
| **Stock Counting (Physical Inventory)** | Create and manage physical inventory count sessions with shelf-to-sheet counting — count sheets organized by physical storage location so staff walks through the storage area in order. Multi-user simultaneous counting (multiple staff count different sections, progress updates in real-time). Barcode scanning to auto-find items. Compare actual vs. expected quantities, highlight variances, approve corrections. Supports offline counting with sync. | P1 |
| **Low Stock & PAR Alerts** | When an item falls below its PAR level or minimum threshold, generate alerts. Configurable per item and per restaurant. Can trigger email notifications, in-app alerts, or auto-generate draft purchase orders. Alert escalation: Low Stock (below PAR) → Critical (below minimum) → Out of Stock. | P1 |
| **Stock Valuation** | Calculate total inventory value using FIFO (First In, First Out), LIFO (Last In, First Out), or Weighted Average cost methods. Configurable per restaurant. End-of-period inventory snapshots for accounting. | P1 |
| **Batch/Lot Tracking** | Track stock by batch number and expiration date. Enable FIFO consumption and expiration alerts. Two-way traceability — trace from supplier delivery to menu item served (critical for recall situations). | P1 |
| **Waste Tracking** | Record and categorize waste using WEPT framework (Waste/spoilage, Errors/incorrect prep, Portioning/over-serving, Theft). Generate waste reports for cost analysis. Industry benchmark: target variance 1.5-2.5% for food. | P1 |

### 2.2 Purchase Order Management

| Feature | Description | Priority |
|---------|-------------|----------|
| **Supplier Management** | CRUD for suppliers with contact info, payment terms, lead times, minimum order quantities, delivery schedules, and food safety certifications. Track supplier compliance status and performance metrics. | P0 |
| **Supplier Compliance Tracking** | Track supplier certifications (organic, HACCP, ISO 22000, halal, kosher), certification expiry dates, food safety audit scores, and delivery quality ratings. Alert when certifications are about to expire. | P2 |
| **Purchase Orders** | Create, submit, and track purchase orders to suppliers. Support draft → submitted → confirmed → partially received → fully received → closed lifecycle. Support email/print PO to supplier. | P1 |
| **Goods Receiving** | Record incoming deliveries against purchase orders. Partial receiving supported. Auto-update stock levels on receipt confirmation. Price variance detection (ordered price vs. invoice price). **Photo capture** — photograph deliveries for quality documentation (damaged goods, incorrect items). Temperature logging on receipt for cold chain compliance. | P1 |
| **Auto-Reorder (PAR-based)** | Based on PAR levels, average daily consumption, and supplier lead times, automatically suggest items that need reordering. Calculate optimal order quantity (order up to PAR level). Optionally auto-generate and even auto-submit draft purchase orders to preferred suppliers. Group suggestions by supplier for efficient ordering. | P1 |

### 2.3 POS Integration Layer

All POS systems (including BonApp, in the future) connect through the same API. There is no "internal" vs "external" distinction — every POS is an external connection.

| Feature | Description | Priority |
|---------|-------------|----------|
| **POS Connection Management** | Any POS system (BonApp, Lightspeed, Toast, Square, Orderbird, or custom) registers as a connection and receives an API key. All stock deductions from POS sales flow through the same API endpoints. No special treatment for any POS system. | P0 |
| **POS Webhook Receiver** | Expose webhook endpoints that POS systems can call to notify of sales. Parse the payload, map external product IDs to internal stock items, and deduct accordingly. HMAC-SHA256 signature verification on all incoming webhooks. | P1 |
| **Outbound Webhook Sender** | Send webhook notifications to external systems when inventory events occur (stock low, stock out, PO created, goods received, count completed). Configurable per event type per connection. Includes retry logic with exponential backoff (3 attempts), delivery logging, and HMAC-SHA256 signed payloads. | P1 |
| **External POS REST API** | Provide a documented REST API (versioned: `/api/pos/v1/`) that external POS systems can call to: query stock levels, report sales, adjust stock, get menu item availability. Secured via API keys (not Bearer tokens). OpenAPI/Swagger documentation auto-generated. | P1 |
| **CSV/Excel Import** | Bulk import stock items, adjustments, supplier data, and recipe mappings from CSV/Excel files. With validation, column mapping preview, error reporting before committing, and rollback on failure. Template downloads provided. | P1 |
| **CSV/Excel Export** | Export stock reports, valuation reports, movement history, and any table/report to CSV/Excel for external accounting systems. | P1 |
| **Concurrency Handling** | Optimistic concurrency control using row version/timestamp on StockItem.CurrentQuantity. Prevents race conditions during high-frequency concurrent stock changes (e.g., multiple orders deducting the same item simultaneously). Retry with backoff on conflict. | P0 |

### 2.4 Reporting & Analytics

| Feature | Description | Priority |
|---------|-------------|----------|
| **Stock Level Report** | Current stock levels for all items with status indicators (OK, Low, Critical, Out of Stock). Filterable by category, supplier, storage location. Includes PAR level comparison. | P0 |
| **Stock Movement History** | Chronological log of all stock changes (receipts, sales deductions, adjustments, waste) with user attribution and timestamps. Full audit trail. | P0 |
| **Actual vs. Theoretical Food Cost Report** | **Industry-critical report** (MarketMan, Apicbase, Restaurant365 all offer this). **Theoretical cost** = what food costs SHOULD be based on recipe costs × POS sales data (ideal scenario: zero waste, perfect portions, no theft). **Actual cost** = (Beginning Inventory + Purchases − Ending Inventory). **Variance** = Actual − Theoretical. A variance > 2-3% signals problems. Drill-down by category and individual item. Helps identify WEPT issues (Waste, Errors, Portioning, Theft). | P0 |
| **Menu Engineering Matrix** | Classify every menu item into 4 quadrants based on **profitability** (contribution margin) and **popularity** (units sold): **Stars** (high profit + high popularity — protect and promote), **Plowhorses** (low profit + high popularity — optimize pricing/portions), **Puzzles** (high profit + low popularity — improve visibility), **Dogs** (low profit + low popularity — consider removing). Data sourced from recipe costs + POS sales. Restaurants implementing menu engineering see 10-15% profit increases. | P1 |
| **Consumption Report** | Daily/weekly/monthly consumption patterns per stock item. Identifies trending items, seasonal patterns, and day-of-week patterns. Supports demand forecasting for prep planning. | P1 |
| **Valuation Report** | Total inventory value breakdown by category and storage location, with cost method applied. End-of-period snapshots for accounting. Historical valuation trend. | P1 |
| **Waste Report (WEPT Analysis)** | Waste analysis using WEPT framework: categorize by Waste (spoilage), Errors (incorrect prep/receiving), Portioning (over-serving), Theft. Calculate waste cost and percentage of total consumption. Industry target: 1.5-2.5% variance. Identify top waste items and root causes. | P1 |
| **Variance Report** | Comparison between expected stock (based on perpetual inventory from POS sales + receiving) and actual stock (from physical counts). WEPT-categorized variance. Highlights shrinkage, over-portioning, and potential theft. Configurable variance thresholds that auto-flag items exceeding limits. | P1 |
| **Supplier Performance Report** | Track supplier delivery times (on-time %), order accuracy (correct items/quantities %), price stability over time, rejection rate, and compliance certification status. Scoring/ranking system. | P2 |
| **Cost of Goods Sold (COGS)** | Calculate actual food/beverage cost per menu item and overall COGS percentage. Compare theoretical vs. actual COGS. Food cost % by category (food vs. beverage vs. supplies). Period-over-period trend analysis. | P1 |
| **Prep List Report** | Auto-generated daily prep lists based on expected sales (from historical patterns), current stock on hand, and recipe requirements. Tells kitchen staff exactly what and how much to prepare. Reduces over-prepping waste. | P2 |

### 2.5 Food Safety & Compliance

| Feature | Description | Priority |
|---------|-------------|----------|
| **Allergen Tracking** | Tag stock items with allergen information (14 EU allergens: gluten, crustaceans, eggs, fish, peanuts, soybeans, milk, nuts, celery, mustard, sesame, sulphites, lupin, molluscs). Allergens auto-propagate from ingredients to recipes/menu items. Allergen report per menu item for customer safety. Aligned with EU/Swiss food safety regulations. | P1 |
| **Nutritional Data** | Store nutritional information per stock item (calories, protein, carbs, fat, fiber, sodium per 100g/ml). Auto-calculate nutritional values for recipes/menu items based on ingredient composition. Useful for dietary menus and health-conscious customers. | P2 |
| **HACCP Task Management** | Digital HACCP (Hazard Analysis Critical Control Points) checklists for food safety compliance. Define recurring checklist templates (receiving inspection, temperature checks, cleaning schedules, date labeling). Staff completes checklists on mobile. Automated alerts for overdue tasks. Audit-ready logs with user, timestamp, and notes on every task completion. | P2 |
| **Temperature Logging** | Record temperatures during goods receiving (cold chain compliance) and from storage location temperature checks. Alert when temperatures exceed safe ranges. Integrates with HACCP checklists. | P2 |
| **Traceability & Recall Management** | Two-way ingredient traceability: from supplier delivery → storage → recipe → menu item served. In case of a recall, quickly identify all affected batches, which dishes used them, and when they were served. Generate recall impact reports. | P2 |
| **Expiration Management** | Track expiration dates on all perishable items. FIFO enforcement (first expiring items used first). Multi-tier alerts: approaching expiration (configurable days), expired today, past expiration. Auto-flag expired items for waste recording or disposal. | P1 |

### 2.6 Demand Forecasting & Prep Planning

| Feature | Description | Priority |
|---------|-------------|----------|
| **Demand Forecasting** | Predict future ingredient needs based on historical POS sales data, day-of-week patterns, seasonal trends, and special events/holidays. Helps prevent over-ordering and stockouts. | P2 |
| **Prep List Generation** | Auto-generate daily prep lists: analyze expected sales → calculate ingredient needs from recipes → subtract current stock on hand → output what kitchen needs to prepare and in what quantities. Assignable to staff members with completion tracking. | P2 |
| **Event-Based Ordering** | For catered events or known busy periods, create special orders above normal PAR levels. The system adjusts PAR temporarily and reverts after the event date. | P3 |

---

## 3. Domain Entities (New & Modified)

### 3.1 Core Entities (Own Database)

**Important:** All entities belong to this standalone system. `Organization` replaces BonApp's `YumYumYard`. `MenuItem` replaces BonApp's `Product`. `AppUser` is managed by this system's own ASP.NET Identity.

```
Organization (the restaurant — replaces BonApp's YumYumYard)
├── Id (int, PK)
├── Name (string, required) — restaurant name
├── Slug (string, unique) — URL-friendly identifier
├── Address (string, nullable)
├── City (string, nullable)
├── PostalCode (string, nullable)
├── Country (string, nullable)
├── Phone (string, nullable)
├── Email (string, nullable)
├── LogoUrl (string, nullable)
├── Currency (string) default "CHF"
├── Timezone (string) default "Europe/Zurich"
├── CostValuationMethod (enum: FIFO, LIFO, WeightedAverage) default WeightedAverage
├── DefaultFoodCostTargetPercent (decimal, nullable) default 30
├── DefaultBeverageCostTargetPercent (decimal, nullable) default 20
├── VarianceAlertThresholdPercent (decimal) default 3
├── LowStockAlertEmail (bool) default true
├── AutoDeductOnSale (bool) default true
├── EnableHaccp (bool) default false
├── EnableAllergenTracking (bool) default false
├── EnablePrepLists (bool) default false
├── InventoryCountFrequency (enum: Daily, Weekly, BiWeekly, Monthly) default Weekly
├── IsActive (bool)
├── CreatedAt (DateTime)
├── SubscriptionPlan (enum: Free, Starter, Professional, Enterprise) default Free

MenuItem (menu products — own entity, NOT BonApp's Product)
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── Name (string, required) — e.g., "Margherita Pizza"
├── Category (string, nullable) — e.g., "Pizza", "Drinks", "Desserts"
├── SellingPrice (decimal)
├── ExternalId (string, nullable) — ID in the connected POS system (for mapping)
├── TheoreticalFoodCost (decimal, nullable) — auto-calculated from recipe
├── FoodCostPercent (decimal, nullable) — TheoreticalFoodCost / SellingPrice * 100
├── MenuEngineeringCategory (enum: Star, Plowhorse, Puzzle, Dog, Unclassified)
├── ContributionMargin (decimal, nullable) — SellingPrice - TheoreticalFoodCost
├── IsActive (bool)
├── CreatedAt (DateTime)
├── UpdatedAt (DateTime)

SaleRecord (POS sales data — received via API from any POS system)
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── PosConnectionId (int, FK → PosConnection, nullable)
├── ExternalOrderId (string, nullable) — order ID from the POS system
├── SaleDate (DateTime)
├── Items (List<SaleRecordItem>)
├── TotalAmount (decimal, nullable)
├── CreatedAt (DateTime)

SaleRecordItem
├── Id (int, PK)
├── SaleRecordId (int, FK → SaleRecord)
├── MenuItemId (int, FK → MenuItem, nullable) — resolved from mapping
├── ExternalProductId (string, nullable) — product ID from POS
├── Quantity (int)
├── UnitPrice (decimal, nullable)

StockItem
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── Name (string, required)
├── SKU (string, unique per restaurant)
├── Description (string, nullable)
├── CategoryId (int, FK → StockCategory)
├── BaseUnitOfMeasurement (enum: Piece, Kilogram, Gram, Liter, Milliliter, Bottle, Box, Pack, Dozen, Portion)
├── CurrentQuantity (decimal)
├── MinimumThreshold (decimal)
├── ParLevel (decimal, nullable) — ideal stock quantity (PAR)
├── ParLevelMon/Tue/Wed/Thu/Fri/Sat/Sun (decimal, nullable) — day-of-week PAR overrides
├── MaximumCapacity (decimal, nullable)
├── CostPrice (decimal) — latest cost per unit
├── AverageCostPrice (decimal) — weighted average cost
├── PrimarySupplierId (int, FK → Supplier, nullable)
├── Barcode (string, nullable)
├── PrimaryStorageLocationId (int, FK → StorageLocation, nullable)
├── IsActive (bool)
├── IsPerishable (bool)
├── DefaultExpirationDays (int, nullable)
├── Notes (string, nullable)
├── RowVersion (byte[]) — optimistic concurrency token
├── CreatedAt (DateTime)
├── UpdatedAt (DateTime)
├── CreatedBy (string, FK → AppUser)
├── // Navigation properties:
├── AllergenTags (List<StockItemAllergen>) — allergens present in this item
├── AlternateUnits (List<AlternateCountUnit>) — counting shortcuts
├── NutritionalInfo (StockItemNutrition, nullable) — calories, protein, etc.

StockCategory
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── Name (string, required) — e.g., "Dairy", "Meat", "Vegetables", "Beverages", "Dry Goods", "Packaging"
├── DisplayOrder (int)
├── ParentCategoryId (int, FK → StockCategory, nullable) — for subcategories
├── IsActive (bool)

MenuItemStockMapping (Recipe / Bill of Materials)
├── Id (int, PK)
├── MenuItemId (int, FK → MenuItem) — the menu item
├── StockItemId (int, FK → StockItem) — the ingredient
├── QuantityRequired (decimal) — amount needed per 1 unit of Product
├── UnitOfMeasurement (enum) — may differ from StockItem's base unit
├── WastePercentage (decimal) — expected waste (e.g., 10% for vegetable peeling)
├── Notes (string, nullable) — e.g., "Use organic only for this dish"

Supplier
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── Name (string, required)
├── ContactPerson (string, nullable)
├── Email (string, nullable)
├── Phone (string, nullable)
├── Address (string, nullable)
├── City (string, nullable)
├── PostalCode (string, nullable)
├── Country (string, nullable)
├── PaymentTerms (string, nullable) — e.g., "Net 30", "COD"
├── LeadTimeDays (int, nullable) — average delivery lead time
├── MinimumOrderAmount (decimal, nullable)
├── Notes (string, nullable)
├── IsActive (bool)
├── CreatedAt (DateTime)

SupplierStockItem (which supplier provides which stock item)
├── Id (int, PK)
├── SupplierId (int, FK → Supplier)
├── StockItemId (int, FK → StockItem)
├── SupplierSKU (string, nullable) — supplier's own product code
├── UnitPrice (decimal)
├── MinimumOrderQuantity (decimal, nullable)
├── IsPreferred (bool) — preferred supplier for this item
├── LastOrderDate (DateTime, nullable)

StockMovement (Audit Trail)
├── Id (int, PK)
├── StockItemId (int, FK → StockItem)
├── OrganizationId (int, FK → Organization)
├── MovementType (enum: Received, Sold, Adjusted, Wasted, Transferred, Returned, CountCorrection, Expired)
├── Quantity (decimal) — positive = increase, negative = decrease
├── PreviousQuantity (decimal)
├── NewQuantity (decimal)
├── CostPerUnit (decimal, nullable)
├── TotalCost (decimal, nullable)
├── Reason (string, nullable) — human-readable reason
├── ReferenceType (string, nullable) — e.g., "OrderHeader", "PurchaseOrder", "StockCount", "Manual"
├── ReferenceId (int, nullable) — FK to the related record
├── BatchNumber (string, nullable)
├── ExpirationDate (DateTime, nullable)
├── CreatedAt (DateTime)
├── CreatedBy (string, FK → AppUser)
├── Notes (string, nullable)

PurchaseOrder
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── SupplierId (int, FK → Supplier)
├── OrderNumber (string, auto-generated)
├── Status (enum: Draft, Submitted, Confirmed, PartiallyReceived, FullyReceived, Cancelled, Closed)
├── OrderDate (DateTime)
├── ExpectedDeliveryDate (DateTime, nullable)
├── ActualDeliveryDate (DateTime, nullable)
├── SubTotal (decimal)
├── TaxAmount (decimal)
├── ShippingCost (decimal)
├── TotalAmount (decimal)
├── Notes (string, nullable)
├── CreatedAt (DateTime)
├── CreatedBy (string, FK → AppUser)
├── UpdatedAt (DateTime)

PurchaseOrderItem
├── Id (int, PK)
├── PurchaseOrderId (int, FK → PurchaseOrder)
├── StockItemId (int, FK → StockItem)
├── OrderedQuantity (decimal)
├── ReceivedQuantity (decimal)
├── UnitPrice (decimal)
├── TotalPrice (decimal)
├── Notes (string, nullable)

GoodsReceiving
├── Id (int, PK)
├── PurchaseOrderId (int, FK → PurchaseOrder)
├── OrganizationId (int, FK → Organization)
├── ReceivingDate (DateTime)
├── ReceivedBy (string, FK → AppUser)
├── InvoiceNumber (string, nullable)
├── InvoiceAmount (decimal, nullable)
├── Notes (string, nullable)
├── Status (enum: Pending, Approved, Rejected)
├── CreatedAt (DateTime)

GoodsReceivingItem
├── Id (int, PK)
├── GoodsReceivingId (int, FK → GoodsReceiving)
├── PurchaseOrderItemId (int, FK → PurchaseOrderItem)
├── StockItemId (int, FK → StockItem)
├── ReceivedQuantity (decimal)
├── AcceptedQuantity (decimal)
├── RejectedQuantity (decimal)
├── UnitPrice (decimal) — actual invoice price
├── BatchNumber (string, nullable)
├── ExpirationDate (DateTime, nullable)
├── Notes (string, nullable)

StockCount (Physical Inventory Session)
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── CountDate (DateTime)
├── Status (enum: InProgress, Completed, Approved, Cancelled)
├── Notes (string, nullable)
├── CreatedBy (string, FK → AppUser)
├── ApprovedBy (string, nullable, FK → AppUser)
├── ApprovedAt (DateTime, nullable)
├── CreatedAt (DateTime)
├── CompletedAt (DateTime, nullable)

StockCountItem
├── Id (int, PK)
├── StockCountId (int, FK → StockCount)
├── StockItemId (int, FK → StockItem)
├── ExpectedQuantity (decimal) — system quantity at count start
├── ActualQuantity (decimal) — physically counted
├── Variance (decimal) — computed: Actual - Expected
├── VarianceValue (decimal) — computed: Variance * CostPrice
├── Notes (string, nullable)
├── CountedBy (string, FK → AppUser)
├── CountedAt (DateTime)

WasteRecord
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── StockItemId (int, FK → StockItem)
├── Quantity (decimal)
├── CostPerUnit (decimal)
├── TotalCost (decimal) — computed
├── WasteReason (enum: Spoilage, Overproduction, CustomerReturn, Preparation, Expired, Damaged, Other)
├── RecordedAt (DateTime)
├── RecordedBy (string, FK → AppUser)
├── Notes (string, nullable)

StockAlert
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── StockItemId (int, FK → StockItem)
├── AlertType (enum: LowStock, OutOfStock, Expiring, Expired, OverStock)
├── Message (string)
├── IsRead (bool)
├── IsDismissed (bool)
├── CreatedAt (DateTime)
├── ReadAt (DateTime, nullable)
├── ReadBy (string, nullable)

PosConnection (any POS system — BonApp, Lightspeed, Toast, Square, etc.)
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── PosSystemName (string) — e.g., "Lightspeed", "Square", "Toast", "Custom"
├── ApiKey (string, encrypted) — API key for external POS to call our API
├── WebhookSecret (string, encrypted) — for verifying incoming webhooks
├── WebhookUrl (string, nullable) — our webhook endpoint for this connection
├── IsActive (bool)
├── LastSyncAt (DateTime, nullable)
├── CreatedAt (DateTime)
├── UpdatedAt (DateTime)

ExternalProductMapping
├── Id (int, PK)
├── ExternalPosConnectionId (int, FK → ExternalPosConnection)
├── ExternalProductId (string) — the product ID in the external POS
├── ExternalProductName (string, nullable) — for reference
├── StockItemId (int, FK → StockItem)
├── QuantityMultiplier (decimal) — how many stock units per 1 external sale
├── IsActive (bool)

StorageLocation (NEW — physical storage areas)
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── Name (string, required) — e.g., "Walk-in Cooler", "Dry Storage A", "Bar", "Wine Cellar", "Freezer 1"
├── Description (string, nullable)
├── LocationType (enum: WalkInCooler, Freezer, DryStorage, Bar, WineCellar, PrepStation, DisplayCase, Other)
├── DisplayOrder (int) — order in which areas should be counted (shelf-to-sheet)
├── TemperatureMin (decimal, nullable) — safe temp range for HACCP
├── TemperatureMax (decimal, nullable)
├── IsActive (bool)

StockItemStorageLocation (stock quantities per location)
├── Id (int, PK)
├── StockItemId (int, FK → StockItem)
├── StorageLocationId (int, FK → StorageLocation)
├── Quantity (decimal) — quantity at this location
├── ShelfPosition (string, nullable) — e.g., "Shelf 3, Row B"

AlternateCountUnit (counting shortcuts)
├── Id (int, PK)
├── StockItemId (int, FK → StockItem)
├── UnitName (string) — e.g., "Case", "6-pack", "Sleeve"
├── ConversionFactor (decimal) — 1 of this unit = X base units. E.g., 1 Case = 24 Bottles
├── Barcode (string, nullable) — case barcode may differ from individual item barcode

UnitConversion (global unit conversion table)
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── FromUnit (enum: UnitOfMeasurement)
├── ToUnit (enum: UnitOfMeasurement)
├── ConversionFactor (decimal) — 1 FromUnit = X ToUnit

AllergenTag (master allergen list — 14 EU allergens)
├── Id (int, PK)
├── Name (string) — "Gluten", "Crustaceans", "Eggs", "Fish", "Peanuts", "Soybeans", "Milk", "Nuts", "Celery", "Mustard", "Sesame", "Sulphites", "Lupin", "Molluscs"
├── Icon (string, nullable) — icon identifier
├── DisplayOrder (int)

StockItemAllergen (many-to-many: stock items ↔ allergens)
├── Id (int, PK)
├── StockItemId (int, FK → StockItem)
├── AllergenTagId (int, FK → AllergenTag)
├── Severity (enum: Contains, MayContain, FreeFrom)
├── Notes (string, nullable)

StockItemNutrition (nutritional data per stock item)
├── Id (int, PK)
├── StockItemId (int, FK → StockItem) — one-to-one
├── CaloriesPer100g (decimal, nullable)
├── ProteinPer100g (decimal, nullable)
├── CarbsPer100g (decimal, nullable)
├── FatPer100g (decimal, nullable)
├── FiberPer100g (decimal, nullable)
├── SodiumPer100g (decimal, nullable)
├── SugarPer100g (decimal, nullable)

HaccpChecklistTemplate (recurring food safety checklists)
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── Name (string) — e.g., "Morning Opening Checklist", "Receiving Inspection", "Evening Close"
├── Frequency (enum: Daily, Weekly, Monthly, OnReceiving)
├── IsActive (bool)
├── CreatedAt (DateTime)

HaccpChecklistItem (individual tasks in a template)
├── Id (int, PK)
├── TemplateId (int, FK → HaccpChecklistTemplate)
├── Description (string) — e.g., "Check walk-in cooler temperature", "Verify date labels on prep items"
├── ExpectedValue (string, nullable) — e.g., "< 4°C"
├── DisplayOrder (int)
├── IsCritical (bool)

HaccpChecklistLog (completed checklist entries)
├── Id (int, PK)
├── TemplateId (int, FK → HaccpChecklistTemplate)
├── OrganizationId (int, FK → Organization)
├── CompletedAt (DateTime)
├── CompletedBy (string, FK → AppUser)
├── Status (enum: Completed, PartiallyCompleted, Failed)
├── Notes (string, nullable)

HaccpChecklistLogItem (individual task results)
├── Id (int, PK)
├── LogId (int, FK → HaccpChecklistLog)
├── ChecklistItemId (int, FK → HaccpChecklistItem)
├── ActualValue (string, nullable) — e.g., "3.2°C"
├── IsPassed (bool)
├── Notes (string, nullable)
├── PhotoUrl (string, nullable) — evidence photo

PrepList (daily prep list)
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── PrepDate (DateTime)
├── Status (enum: Generated, InProgress, Completed)
├── GeneratedBy (string, FK → AppUser)
├── CreatedAt (DateTime)
├── CompletedAt (DateTime, nullable)

PrepListItem (what to prep)
├── Id (int, PK)
├── PrepListId (int, FK → PrepList)
├── StockItemId (int, FK → StockItem)
├── RequiredQuantity (decimal) — calculated from forecasted sales × recipes
├── OnHandQuantity (decimal) — current stock at generation time
├── PrepQuantity (decimal) — RequiredQuantity - OnHandQuantity (if positive)
├── ActualPrepQuantity (decimal, nullable) — what was actually prepped
├── AssignedTo (string, nullable, FK → AppUser)
├── IsCompleted (bool)
├── Notes (string, nullable)

WebhookSubscription (outbound webhooks)
├── Id (int, PK)
├── ExternalPosConnectionId (int, FK → ExternalPosConnection)
├── EventType (enum: StockLow, StockOut, GoodsReceived, PurchaseOrderCreated, CountCompleted, WasteRecorded)
├── TargetUrl (string) — external system's webhook URL
├── Secret (string, encrypted) — HMAC signing secret
├── IsActive (bool)
├── CreatedAt (DateTime)

WebhookDeliveryLog (outbound webhook delivery tracking)
├── Id (int, PK)
├── SubscriptionId (int, FK → WebhookSubscription)
├── EventType (string)
├── Payload (string, JSON)
├── ResponseStatusCode (int, nullable)
├── ResponseBody (string, nullable)
├── AttemptNumber (int)
├── DeliveredAt (DateTime)
├── IsSuccess (bool)

InventorySnapshot (end-of-period snapshots for reporting)
├── Id (int, PK)
├── OrganizationId (int, FK → Organization)
├── SnapshotDate (DateTime)
├── SnapshotType (enum: Daily, Weekly, Monthly, Manual)
├── TotalValue (decimal)
├── TotalItems (int)
├── CreatedAt (DateTime)

InventorySnapshotItem (per-item snapshot)
├── Id (int, PK)
├── SnapshotId (int, FK → InventorySnapshot)
├── StockItemId (int, FK → StockItem)
├── Quantity (decimal)
├── CostPerUnit (decimal)
├── TotalValue (decimal)

SupplierCertification (supplier compliance)
├── Id (int, PK)
├── SupplierId (int, FK → Supplier)
├── CertificationType (string) — e.g., "HACCP", "ISO 22000", "Organic", "Halal", "Kosher"
├── CertificationNumber (string, nullable)
├── IssuedDate (DateTime, nullable)
├── ExpiryDate (DateTime, nullable)
├── DocumentUrl (string, nullable)
├── IsActive (bool)
```

### 3.2 No Modified Entities

Since this is a **standalone application** with its own database, there are **no existing entities to modify**. All entities above are created fresh in the Inventory Pro database. The `Organization`, `MenuItem`, and `AppUser` entities are this system's own — they are NOT BonApp's `YumYumYard`, `Product`, or `ApplicationUser`.

**Note on Organization settings:** All settings that would have been on BonApp's `YumYumYard` (cost valuation method, food cost targets, HACCP toggles, etc.) are now directly on the `Organization` entity above.

---

## 4. API Endpoints to Build

### 4.1 Stock Item Endpoints (`/api/inventory/stock-items`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/{orgId}` | Manager | List all stock items (paginated, filterable) | Query: page, pageSize, categoryId?, search?, isActive?, lowStockOnly? | `PaginatedResponse<StockItemDto>` |
| 2 | GET | `/{orgId}/{stockItemId}` | Manager | Get stock item detail | - | `StockItemDetailDto` |
| 3 | POST | `/{orgId}` | Manager | Create stock item | `CreateStockItemDto` | `StockItemDto` |
| 4 | PUT | `/{orgId}/{stockItemId}` | Manager | Update stock item | `UpdateStockItemDto` | `StockItemDto` |
| 5 | DELETE | `/{orgId}/{stockItemId}` | Manager | Soft-delete stock item | - | `ServiceResponseDto` |
| 6 | POST | `/{orgId}/import` | Manager | Bulk import from CSV | `IFormFile` | `ImportResultDto` |
| 7 | GET | `/{orgId}/export` | Manager | Export to CSV | Query: categoryId?, format (csv/excel) | `File` |
| 8 | GET | `/{orgId}/low-stock` | Manager | Get items below minimum threshold | - | `List<StockAlertDto>` |
| 9 | POST | `/{orgId}/{stockItemId}/adjust` | Manager/Waiter | Manual stock adjustment | `StockAdjustmentDto` | `StockMovementDto` |
| 10 | GET | `/{orgId}/{stockItemId}/movements` | Manager | Get movement history for item | Query: startDate?, endDate?, page, pageSize | `PaginatedResponse<StockMovementDto>` |

### 4.2 Stock Category Endpoints (`/api/inventory/categories`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/{orgId}` | Manager | List stock categories | - | `List<StockCategoryDto>` |
| 2 | POST | `/{orgId}` | Manager | Create category | `CreateStockCategoryDto` | `StockCategoryDto` |
| 3 | PUT | `/{orgId}/{categoryId}` | Manager | Update category | `UpdateStockCategoryDto` | `StockCategoryDto` |
| 4 | DELETE | `/{orgId}/{categoryId}` | Manager | Delete category | - | `ServiceResponseDto` |

### 4.3 Recipe / Menu Item-Stock Mapping Endpoints (`/api/inventory/recipes`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/{orgId}/menu-item/{menuItemId}` | Manager | Get recipe for a menu item | - | `RecipeDto` |
| 2 | POST | `/{orgId}/menu-item/{menuItemId}` | Manager | Set/update recipe mappings | `List<MenuItemStockMappingDto>` | `RecipeDto` |
| 3 | DELETE | `/{orgId}/menu-item/{menuItemId}/{mappingId}` | Manager | Remove ingredient from recipe | - | `ServiceResponseDto` |
| 4 | GET | `/{orgId}/stock-item/{stockItemId}/menu-items` | Manager | Get all menu items using this stock item | - | `MenuItemUsageDto` |
| 5 | POST | `/{orgId}/calculate-deduction` | Manager | Preview stock deduction for a sale (dry run) | `SaleDeductionPreviewDto` | `List<StockDeductionResultDto>` |

### 4.4 Supplier Endpoints (`/api/inventory/suppliers`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/{orgId}` | Manager | List suppliers | Query: search?, isActive? | `List<SupplierDto>` |
| 2 | GET | `/{orgId}/{supplierId}` | Manager | Get supplier detail | - | `SupplierDetailDto` |
| 3 | POST | `/{orgId}` | Manager | Create supplier | `CreateSupplierDto` | `SupplierDto` |
| 4 | PUT | `/{orgId}/{supplierId}` | Manager | Update supplier | `UpdateSupplierDto` | `SupplierDto` |
| 5 | DELETE | `/{orgId}/{supplierId}` | Manager | Deactivate supplier | - | `ServiceResponseDto` |
| 6 | GET | `/{orgId}/{supplierId}/stock-items` | Manager | Get stock items from supplier | - | `List<SupplierStockItemDto>` |
| 7 | POST | `/{orgId}/{supplierId}/stock-items` | Manager | Link stock items to supplier | `List<CreateSupplierStockItemDto>` | `List<SupplierStockItemDto>` |

### 4.5 Purchase Order Endpoints (`/api/inventory/purchase-orders`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/{orgId}` | Manager | List purchase orders | Query: status?, supplierId?, startDate?, endDate?, page, pageSize | `PaginatedResponse<PurchaseOrderDto>` |
| 2 | GET | `/{orgId}/{orderId}` | Manager | Get PO detail | - | `PurchaseOrderDetailDto` |
| 3 | POST | `/{orgId}` | Manager | Create PO | `CreatePurchaseOrderDto` | `PurchaseOrderDto` |
| 4 | PUT | `/{orgId}/{orderId}` | Manager | Update PO (draft only) | `UpdatePurchaseOrderDto` | `PurchaseOrderDto` |
| 5 | POST | `/{orgId}/{orderId}/submit` | Manager | Submit PO to supplier | - | `PurchaseOrderDto` |
| 6 | POST | `/{orgId}/{orderId}/cancel` | Manager | Cancel PO | `CancelReasonDto` | `PurchaseOrderDto` |
| 7 | POST | `/{orgId}/{orderId}/receive` | Manager | Record goods receiving | `GoodsReceivingDto` | `GoodsReceivingResponseDto` |
| 8 | GET | `/{orgId}/suggestions` | Manager | Get auto-reorder suggestions | - | `List<ReorderSuggestionDto>` |
| 9 | POST | `/{orgId}/generate-from-suggestions` | Manager | Auto-generate PO from suggestions | `List<int>` stockItemIds | `PurchaseOrderDto` |

### 4.6 Stock Count Endpoints (`/api/inventory/stock-counts`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/{orgId}` | Manager | List stock count sessions | Query: status?, page, pageSize | `PaginatedResponse<StockCountDto>` |
| 2 | POST | `/{orgId}` | Manager | Start new count session | `CreateStockCountDto` | `StockCountDetailDto` |
| 3 | GET | `/{orgId}/{countId}` | Manager | Get count session detail | - | `StockCountDetailDto` |
| 4 | PUT | `/{orgId}/{countId}/items` | Manager/Waiter | Submit counted items | `List<StockCountItemDto>` | `StockCountDetailDto` |
| 5 | POST | `/{orgId}/{countId}/complete` | Manager | Complete count session | - | `StockCountDetailDto` |
| 6 | POST | `/{orgId}/{countId}/approve` | Manager | Approve and apply corrections | - | `StockCountDetailDto` |
| 7 | POST | `/{orgId}/{countId}/cancel` | Manager | Cancel count session | - | `ServiceResponseDto` |

### 4.7 Waste Tracking Endpoints (`/api/inventory/waste`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/{orgId}` | Manager | List waste records | Query: startDate?, endDate?, reason?, page, pageSize | `PaginatedResponse<WasteRecordDto>` |
| 2 | POST | `/{orgId}` | Manager/Waiter | Record waste | `CreateWasteRecordDto` | `WasteRecordDto` |
| 3 | GET | `/{orgId}/summary` | Manager | Waste summary/analytics | Query: startDate?, endDate? | `WasteSummaryDto` |

### 4.8 Reports & Analytics Endpoints (`/api/inventory/reports`)

| # | Method | Route | Auth | Description | Query | Response |
|---|--------|-------|------|-------------|-------|----------|
| 1 | GET | `/{orgId}/stock-levels` | Manager | Current stock levels report | categoryId?, storageLocationId?, lowStockOnly? | `StockLevelReportDto` |
| 2 | GET | `/{orgId}/movements` | Manager | Stock movement history | startDate?, endDate?, stockItemId?, type? | `PaginatedResponse<StockMovementDto>` |
| 3 | GET | `/{orgId}/consumption` | Manager | Consumption analysis | startDate, endDate, groupBy (day/week/month) | `ConsumptionReportDto` |
| 4 | GET | `/{orgId}/valuation` | Manager | Inventory valuation | asOfDate? | `ValuationReportDto` |
| 5 | GET | `/{orgId}/waste-analysis` | Manager | Waste analysis (WEPT) | startDate, endDate | `WasteAnalysisDto` |
| 6 | GET | `/{orgId}/cogs` | Manager | Cost of Goods Sold | startDate, endDate | `COGSReportDto` |
| 7 | GET | `/{orgId}/variance` | Manager | Variance report (expected vs actual) | stockCountId? | `VarianceReportDto` |
| 8 | GET | `/{orgId}/actual-vs-theoretical` | Manager | **Actual vs. Theoretical food cost** | startDate, endDate, categoryId? | `ActualVsTheoreticalReportDto` |
| 9 | GET | `/{orgId}/menu-engineering` | Manager | **Menu engineering matrix** | startDate, endDate, categoryId? | `MenuEngineeringReportDto` |
| 10 | GET | `/{orgId}/supplier-performance` | Manager | Supplier performance scorecard | supplierId?, startDate, endDate | `SupplierPerformanceReportDto` |
| 11 | POST | `/{orgId}/snapshot` | Manager | Take end-of-period inventory snapshot | - | `InventorySnapshotDto` |
| 12 | GET | `/{orgId}/snapshots` | Manager | List past inventory snapshots | startDate?, endDate? | `List<InventorySnapshotSummaryDto>` |
| 13 | POST | `/{orgId}/print-stock-report` | Manager | Print stock report to printer | printerId? | `ServiceResponseDto` |

### 4.8b Storage Location Endpoints (`/api/inventory/storage-locations`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/{orgId}` | Manager | List storage locations | - | `List<StorageLocationDto>` |
| 2 | POST | `/{orgId}` | Manager | Create storage location | `CreateStorageLocationDto` | `StorageLocationDto` |
| 3 | PUT | `/{orgId}/{locationId}` | Manager | Update storage location | `UpdateStorageLocationDto` | `StorageLocationDto` |
| 4 | DELETE | `/{orgId}/{locationId}` | Manager | Delete storage location | - | `ServiceResponseDto` |
| 5 | GET | `/{orgId}/{locationId}/stock` | Manager | Get stock items at location | - | `List<StockItemAtLocationDto>` |
| 6 | POST | `/{orgId}/transfer` | Manager | Transfer stock between locations | `StockTransferDto` | `StockMovementDto` |

### 4.8c Allergen & Nutrition Endpoints (`/api/inventory/food-safety`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/allergens` | Manager | List all allergen tags (14 EU) | - | `List<AllergenTagDto>` |
| 2 | GET | `/{orgId}/stock-items/{stockItemId}/allergens` | Manager | Get allergens for stock item | - | `List<StockItemAllergenDto>` |
| 3 | PUT | `/{orgId}/stock-items/{stockItemId}/allergens` | Manager | Set allergens for stock item | `List<SetAllergenDto>` | `List<StockItemAllergenDto>` |
| 4 | GET | `/{orgId}/menu-items/{menuItemId}/allergens` | Manager | Get allergens for menu item (auto-calculated from recipe) | - | `MenuItemAllergenReportDto` |
| 5 | GET | `/{orgId}/stock-items/{stockItemId}/nutrition` | Manager | Get nutritional data | - | `StockItemNutritionDto` |
| 6 | PUT | `/{orgId}/stock-items/{stockItemId}/nutrition` | Manager | Set nutritional data | `SetNutritionDto` | `StockItemNutritionDto` |
| 7 | GET | `/{orgId}/menu-items/{menuItemId}/nutrition` | Manager | Get menu item nutrition (auto-calculated from recipe) | - | `MenuItemNutritionReportDto` |

### 4.8d HACCP Endpoints (`/api/inventory/haccp`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/{orgId}/templates` | Manager | List HACCP checklist templates | - | `List<HaccpTemplateDto>` |
| 2 | POST | `/{orgId}/templates` | Manager | Create HACCP template | `CreateHaccpTemplateDto` | `HaccpTemplateDto` |
| 3 | PUT | `/{orgId}/templates/{templateId}` | Manager | Update template | `UpdateHaccpTemplateDto` | `HaccpTemplateDto` |
| 4 | DELETE | `/{orgId}/templates/{templateId}` | Manager | Delete template | - | `ServiceResponseDto` |
| 5 | GET | `/{orgId}/logs` | Manager | List completed checklist logs | Query: startDate?, endDate?, templateId? | `PaginatedResponse<HaccpLogDto>` |
| 6 | POST | `/{orgId}/logs` | Manager/Waiter | Submit completed checklist | `CreateHaccpLogDto` | `HaccpLogDto` |
| 7 | GET | `/{orgId}/logs/{logId}` | Manager | Get checklist log detail | - | `HaccpLogDetailDto` |
| 8 | GET | `/{orgId}/due-today` | Manager/Waiter | Get checklists due today | - | `List<HaccpDueTodayDto>` |

### 4.8e Prep List Endpoints (`/api/inventory/prep-lists`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | POST | `/{orgId}/generate` | Manager | Generate prep list for a date | `GeneratePrepListDto` (date, based on forecast or manual) | `PrepListDto` |
| 2 | GET | `/{orgId}` | Manager | List prep lists | Query: startDate?, endDate?, status? | `PaginatedResponse<PrepListSummaryDto>` |
| 3 | GET | `/{orgId}/{prepListId}` | Manager/Waiter | Get prep list detail | - | `PrepListDetailDto` |
| 4 | PUT | `/{orgId}/{prepListId}/items` | Manager/Waiter | Update prep item completion | `List<UpdatePrepItemDto>` | `PrepListDetailDto` |
| 5 | POST | `/{orgId}/{prepListId}/complete` | Manager | Mark prep list as completed | - | `PrepListDto` |

### 4.9 Alerts & Notifications Endpoints (`/api/inventory/alerts`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/{orgId}` | Manager | Get all alerts | Query: isRead?, alertType?, page, pageSize | `PaginatedResponse<StockAlertDto>` |
| 2 | POST | `/{orgId}/{alertId}/read` | Manager | Mark alert as read | - | `ServiceResponseDto` |
| 3 | POST | `/{orgId}/read-all` | Manager | Mark all alerts as read | - | `ServiceResponseDto` |
| 4 | POST | `/{orgId}/{alertId}/dismiss` | Manager | Dismiss alert | - | `ServiceResponseDto` |

### 4.10 POS Connection Management Endpoints (`/api/inventory/pos-connections`)

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/{orgId}/connections` | Manager | List POS connections | - | `List<ExternalPosConnectionDto>` |
| 2 | POST | `/{orgId}/connections` | Manager | Create POS connection (generates API key) | `CreateExternalPosConnectionDto` | `ExternalPosConnectionDto` |
| 3 | PUT | `/{orgId}/connections/{connectionId}` | Manager | Update connection | `UpdateExternalPosConnectionDto` | `ExternalPosConnectionDto` |
| 4 | DELETE | `/{orgId}/connections/{connectionId}` | Manager | Deactivate connection | - | `ServiceResponseDto` |
| 5 | POST | `/{orgId}/connections/{connectionId}/regenerate-key` | Manager | Regenerate API key | - | `ApiKeyResponseDto` |
| 6 | GET | `/{orgId}/connections/{connectionId}/mappings` | Manager | Get product mappings | - | `List<ExternalProductMappingDto>` |
| 7 | POST | `/{orgId}/connections/{connectionId}/mappings` | Manager | Create/update mappings | `List<ExternalProductMappingDto>` | `List<ExternalProductMappingDto>` |

### 4.11 POS API (API Key Auth — for all connected POS systems)

These endpoints are called BY POS systems (BonApp, Lightspeed, Toast, etc.), authenticated via API key in the `X-API-Key` header.

| # | Method | Route | Auth | Description | Request | Response |
|---|--------|-------|------|-------------|---------|----------|
| 1 | GET | `/api/pos/v1/stock` | API Key | Get all stock levels | Query: updatedSince? | `List<ExternalStockItemDto>` |
| 2 | GET | `/api/pos/v1/stock/{sku}` | API Key | Get stock level by SKU | - | `ExternalStockItemDto` |
| 3 | POST | `/api/pos/v1/sale` | API Key | Report a sale (deduct stock) | `ExternalSaleDto` | `ExternalSaleResponseDto` |
| 4 | POST | `/api/pos/v1/sale/bulk` | API Key | Report multiple sales (atomic) | `List<ExternalSaleDto>` | `ExternalBulkSaleResponseDto` |
| 5 | POST | `/api/pos/v1/adjustment` | API Key | Adjust stock level | `ExternalAdjustmentDto` | `ExternalAdjustmentResponseDto` |
| 6 | GET | `/api/pos/v1/availability` | API Key | Get menu item availability (in/out of stock based on ingredients) | - | `List<MenuItemAvailabilityDto>` |
| 7 | POST | `/api/pos/v1/webhook` | Webhook Secret | Receive POS webhook events | Varies by POS | 200 OK |
| 8 | GET | `/api/pos/v1/webhooks/subscriptions` | API Key | List outbound webhook subscriptions | - | `List<WebhookSubscriptionDto>` |
| 9 | POST | `/api/pos/v1/webhooks/subscriptions` | API Key | Create outbound webhook subscription | `CreateWebhookSubscriptionDto` | `WebhookSubscriptionDto` |
| 10 | DELETE | `/api/pos/v1/webhooks/subscriptions/{id}` | API Key | Delete outbound webhook subscription | - | 204 No Content |

**Total New Endpoints: ~110+**

---

## 5. Service Contracts (New Interfaces)

### Core Services

| Interface | Key Methods |
|-----------|------------|
| `IStockItemService` | CreateStockItem, UpdateStockItem, DeleteStockItem, GetStockItem, GetStockItems, GetLowStockItems, AdjustStock, ImportFromCsv, ExportToCsv, GetStockItemsBySupplier, GetStockItemsByLocation |
| `IStockCategoryService` | CreateCategory, UpdateCategory, DeleteCategory, GetCategories |
| `IStorageLocationService` | CreateLocation, UpdateLocation, DeleteLocation, GetLocations, GetStockAtLocation, TransferStock |
| `IRecipeService` | GetRecipe, SetRecipeMappings, RemoveMapping, GetProductsUsingStockItem, CalculateDeductionPreview, CalculateRecipeCost, GetRecipeCostBreakdown |
| `ISupplierService` | CreateSupplier, UpdateSupplier, DeleteSupplier, GetSupplier, GetSuppliers, LinkStockItems, GetSupplierStockItems, GetSupplierPerformance |
| `ISupplierComplianceService` | AddCertification, UpdateCertification, GetCertifications, CheckExpiringCertifications |
| `IPurchaseOrderService` | CreatePurchaseOrder, UpdatePurchaseOrder, SubmitPurchaseOrder, CancelPurchaseOrder, GetPurchaseOrders, GetPurchaseOrder, GetReorderSuggestions, GenerateFromSuggestions, GenerateFromParLevels |
| `IGoodsReceivingService` | ReceiveGoods, GetReceivingHistory, ApproveReceiving, RejectReceiving, RecordTemperature, AttachPhoto |
| `IStockCountService` | StartCount, SubmitCountItems, CompleteCount, ApproveCount, CancelCount, GetCounts, GetCountDetail, GenerateCountSheet (by storage location for shelf-to-sheet) |
| `IWasteService` | RecordWaste, GetWasteRecords, GetWasteSummary, GetWEPTAnalysis |
| `IStockMovementService` | GetMovements, GetMovementsByStockItem, CreateMovement |
| `IStockAlertService` | GetAlerts, MarkAsRead, MarkAllAsRead, DismissAlert, GenerateAlerts, CheckLowStock, CheckParLevels, CheckExpirations |
| `IInventoryReportService` | GetStockLevelReport, GetConsumptionReport, GetValuationReport, GetWasteAnalysis, GetCOGSReport, GetVarianceReport, GetActualVsTheoreticalReport, GetMenuEngineeringMatrix, GetSupplierPerformanceReport, TakeSnapshot, GetSnapshots, PrintStockReport |
| `IStockDeductionService` | DeductForSale, RevertDeduction (for cancelled orders/refunds), CheckMenuItemAvailability |
| `IUnitConversionService` | Convert, GetConversions, AddConversion, GetAlternateUnits, ConvertToBaseUnit |

### Food Safety Services

| Interface | Key Methods |
|-----------|------------|
| `IAllergenService` | GetAllergenTags, SetStockItemAllergens, GetStockItemAllergens, GetProductAllergens (auto-calculated from recipe), GetAllergenReport |
| `INutritionService` | GetStockItemNutrition, SetStockItemNutrition, CalculateProductNutrition (auto-calculated from recipe) |
| `IHaccpService` | GetTemplates, CreateTemplate, UpdateTemplate, DeleteTemplate, SubmitLog, GetLogs, GetLogDetail, GetDueToday |
| `ITraceabilityService` | TraceForward (ingredient → dishes), TraceBackward (dish → suppliers), GenerateRecallReport |

### Forecasting & Prep Services

| Interface | Key Methods |
|-----------|------------|
| `IDemandForecastService` | ForecastConsumption (by day/week), GetHistoricalPatterns, GetSeasonalTrends |
| `IPrepListService` | GeneratePrepList, GetPrepLists, GetPrepListDetail, UpdatePrepItemCompletion, CompletePrepList |

### POS Integration Services

| Interface | Key Methods |
|-----------|------------|
| `IPosConnectionService` | CreateConnection, UpdateConnection, DeleteConnection, GetConnections, RegenerateApiKey |
| `IPosProductMappingService` | CreateMappings, UpdateMappings, GetMappings, MapSaleToStockItems |
| `IPosApiService` | GetStockLevels, GetStockBySku, ProcessSale, ProcessBulkSale, ProcessAdjustment, GetMenuItemAvailability |
| `IWebhookReceiverService` | ProcessInboundWebhook, ValidateWebhookSignature |
| `IOutboundWebhookService` | GetSubscriptions, CreateSubscription, DeleteSubscription, SendWebhook, RetryFailedDeliveries |
| `ICsvImportService` | ParseCsvFile, ValidateImportData, ExecuteImport, GenerateImportReport, GetImportTemplate |
| `ICsvExportService` | ExportStockItems, ExportMovements, ExportValuation, ExportAnyReport |
| `IInventorySnapshotService` | TakeSnapshot, GetSnapshots, GetSnapshotDetail, CompareSnapshots |

---

## 6. Validation Rules (FluentValidation)

### Key Validators to Create

| Validator | Rules |
|-----------|-------|
| `CreateStockItemValidator` | Name required (max 200), SKU unique per restaurant, UnitOfMeasurement valid enum, MinimumThreshold >= 0, CostPrice >= 0, CategoryId exists |
| `StockAdjustmentValidator` | Quantity != 0, Reason required, resulting quantity >= 0 (optional — allow negative for tracking debt) |
| `CreatePurchaseOrderValidator` | SupplierId exists, at least 1 line item, all StockItemIds exist, quantities > 0, unit prices >= 0 |
| `GoodsReceivingValidator` | PurchaseOrderId exists and status is Submitted/Confirmed/PartiallyReceived, received quantities >= 0 and <= remaining |
| `StockCountItemValidator` | ActualQuantity >= 0, StockItemId exists in restaurant |
| `CreateWasteRecordValidator` | StockItemId exists, Quantity > 0, WasteReason valid enum |
| `MenuItemStockMappingValidator` | StockItemId exists, QuantityRequired > 0, WastePercentage 0-100 |
| `CreateSupplierValidator` | Name required (max 200), Email valid format if provided, Phone valid if provided |
| `ExternalSaleValidator` | At least 1 line item, quantities > 0, product references resolve to mappings |
| `CsvImportValidator` | File not empty, correct headers, data types valid, SKUs resolvable |
| `StorageLocationValidator` | Name required (max 100), unique name per restaurant, LocationType valid enum |
| `StockTransferValidator` | Source and destination locations exist, quantity > 0 and <= source quantity, same stock item |
| `HaccpTemplateValidator` | Name required, at least 1 checklist item, Frequency valid enum |
| `HaccpLogValidator` | TemplateId exists, all items have IsPassed value, critical items cannot be skipped |
| `PrepListValidator` | PrepDate not in past, restaurant exists |
| `AllergenValidator` | AllergenTagId exists, Severity valid enum |
| `NutritionValidator` | All values >= 0 if provided |
| `WebhookSubscriptionValidator` | TargetUrl is valid HTTPS URL, EventType valid enum, Secret required |
| `UnitConversionValidator` | FromUnit != ToUnit, ConversionFactor > 0 |

---

## 7. Database Migrations Plan

| Order | Migration Name | Description |
|-------|---------------|-------------|
| 1 | `AddStorageLocations` | Create StorageLocation table |
| 2 | `AddStockCategories` | Create StockCategory table |
| 3 | `AddStockItems` | Create StockItem table with FK to StockCategory, Supplier, StorageLocation, Organization. Includes PAR levels, RowVersion |
| 4 | `AddStockItemStorageLocations` | Create StockItemStorageLocation (per-location quantities) |
| 5 | `AddAlternateCountUnits` | Create AlternateCountUnit and UnitConversion tables |
| 6 | `AddMenuItemStockMappings` | Create MenuItemStockMapping (recipe) table |
| 7 | `AddSuppliers` | Create Supplier, SupplierStockItem, and SupplierCertification tables |
| 8 | `AddStockMovements` | Create StockMovement audit trail table |
| 9 | `AddPurchaseOrders` | Create PurchaseOrder and PurchaseOrderItem tables |
| 10 | `AddGoodsReceiving` | Create GoodsReceiving and GoodsReceivingItem tables |
| 11 | `AddStockCounts` | Create StockCount and StockCountItem tables |
| 12 | `AddWasteRecords` | Create WasteRecord table |
| 13 | `AddStockAlerts` | Create StockAlert table |
| 14 | `AddAllergenTracking` | Create AllergenTag, StockItemAllergen, StockItemNutrition tables |
| 15 | `AddHaccpModule` | Create HaccpChecklistTemplate, HaccpChecklistItem, HaccpChecklistLog, HaccpChecklistLogItem tables |
| 16 | `AddPrepLists` | Create PrepList and PrepListItem tables |
| 17 | `AddExternalPosIntegration` | Create ExternalPosConnection, ExternalProductMapping, WebhookSubscription, WebhookDeliveryLog tables |
| 18 | `AddInventorySnapshots` | Create InventorySnapshot and InventorySnapshotItem tables |
| 19 | `AddOrganizationTable` | Create Organization table with all config fields (cost targets, HACCP toggles, currency, etc.) |
| 20 | `AddMenuItemTable` | Create MenuItem table with recipe costing fields (TheoreticalFoodCost, FoodCostPercent, MenuEngineeringCategory, ContributionMargin) |
| 21 | `AddSaleRecordTable` | Create SaleRecord and SaleRecordItem tables for POS sales data audit trail |
| 22 | `SeedAllergenTags` | Seed the 14 EU mandatory allergens into AllergenTag table |

---

## 8. Integration Architecture (Standalone — API-Based)

This system operates **independently**. All POS integrations happen through the External POS API. There is no direct code dependency on BonApp or any other system.

### 8.1 POS Sale → Stock Deduction Flow (via API)

Any POS system (BonApp, Lightspeed, Toast, etc.) reports sales through the same API:

```
External POS System → POST /api/pos/v1/sale (with X-API-Key header)
    └── IStockDeductionService.DeductForExternalSale(saleDto, connectionId)
        ├── Resolve ExternalProductId → MenuItemId via ExternalProductMapping
        ├── Create SaleRecord + SaleRecordItems for audit trail
        ├── For each sale item:
        │   ├── Get MenuItemStockMappings (recipe) for this MenuItem
        │   ├── For each mapping:
        │   │   ├── Calculate: quantity = mapping.QuantityRequired * saleItem.Quantity
        │   │   ├── Apply waste factor: quantity *= (1 + mapping.WastePercentage / 100)
        │   │   ├── StockItem.CurrentQuantity -= quantity (with optimistic concurrency)
        │   │   ├── Create StockMovement record (type: Sold, reference: SaleRecord)
        │   │   └── Check if StockItem.CurrentQuantity < MinimumThreshold → create alert
        │   │   └── Fire outbound webhook if subscribed to StockLow/StockOut event
        └── UnitOfWork.SaveAsync()
        └── Return ExternalSaleResponseDto with deduction summary
```

### 8.2 POS Refund → Stock Revert Flow (via API)

```
External POS System → POST /api/pos/v1/sale (with negative quantities or refund flag)
    └── IStockDeductionService.RevertDeduction(saleDto, connectionId)
        ├── StockItem.CurrentQuantity += reverted quantity
        ├── Create StockMovement record (type: Returned, reference: SaleRecord)
        └── UnitOfWork.SaveAsync()
```

### 8.3 Goods Receiving → Stock Increase Flow

```
IGoodsReceivingService.ReceiveGoods()
    ├── Validate against PurchaseOrder
    ├── For each received item:
    │   ├── StockItem.CurrentQuantity += AcceptedQuantity
    │   ├── Update StockItem.CostPrice (latest price)
    │   ├── Recalculate StockItem.AverageCostPrice (weighted average)
    │   ├── Create StockMovement (type: Received, reference: PurchaseOrder)
    │   └── Update PurchaseOrderItem.ReceivedQuantity
    ├── Update PurchaseOrder.Status (PartiallyReceived or FullyReceived)
    ├── Fire outbound webhooks if subscribed to GoodsReceived event
    └── UnitOfWork.SaveAsync()
```

### 8.4 Printer Integration (Own Implementation)

Build own PDF generation for printable reports:
- Stock level reports (PDF export)
- Purchase orders (PDF for emailing/printing to suppliers)
- Stock count sheets (PDF for physical inventory)
- Use `QuestPDF` or `IronPDF` library for server-side PDF generation

### 8.5 Email Integration (Own SMTP/SendGrid)

Own email service using SendGrid (or any SMTP provider):
- Low stock alert emails to restaurant managers
- Purchase order confirmation emails to suppliers
- Stock count completion summary emails
- HACCP overdue checklist alerts
- Supplier certification expiration warnings
- Daily actual vs. theoretical variance summary (if variance > threshold)
- User invitation and password reset emails

### 8.6 Future BonApp Integration Plan

When connecting to BonApp in the future, the flow is:

```
1. BonApp admin creates a POS Connection in Inventory Pro UI
   → Gets an API key (X-API-Key) and webhook secret

2. BonApp backend is configured with this API key
   → On order confirmation: BonApp calls POST /api/pos/v1/sale
   → On refund: BonApp calls POST /api/pos/v1/sale (with refund flag)
   → On menu update: BonApp calls POST /api/pos/v1/products/sync (optional)

3. Inventory Pro sends outbound webhooks to BonApp (optional):
   → Stock level changes → BonApp can update menu availability
   → Out of stock → BonApp can disable menu items

4. NO SHARED DATABASE. NO SHARED CODE. API CALLS ONLY.
```

### 8.6 Actual vs. Theoretical Food Cost Calculation Flow

This is the industry's most important cost control tool (used by MarketMan, Apicbase, Restaurant365, CrunchTime):

```
IInventoryReportService.GetActualVsTheoreticalReport(restaurantId, startDate, endDate)
    ├── THEORETICAL COST (what costs SHOULD be in ideal scenario):
    │   ├── Query POS sales data (SaleRecordItems) for the period
    │   ├── For each SaleRecordItem:
    │   │   ├── Get MenuItemStockMappings (recipe) for this Product
    │   │   ├── For each ingredient: theoreticalQty = mapping.QuantityRequired * orderDetail.Count
    │   │   └── theoreticalCost += theoreticalQty * stockItem.AverageCostPrice
    │   └── Total Theoretical Cost = sum of all ingredient costs based on recipes
    │
    ├── ACTUAL COST (what costs REALLY were, based on inventory):
    │   ├── Beginning Inventory = InventorySnapshot value at startDate (or physical count)
    │   ├── + Purchases = sum of GoodsReceiving.AcceptedQuantity * UnitPrice during period
    │   ├── − Ending Inventory = InventorySnapshot value at endDate (or latest count)
    │   ├── Actual COGS = Beginning + Purchases − Ending
    │   └── (Formula: COGS = Beginning Inventory + Purchases − Ending Inventory)
    │
    ├── VARIANCE:
    │   ├── Variance $ = Actual Cost − Theoretical Cost
    │   ├── Variance % = (Variance $ / Theoretical Cost) * 100
    │   ├── If Variance % > restaurant.VarianceAlertThresholdPercent → generate alert
    │   └── Drill-down: per-category and per-item variance
    │
    └── WEPT CLASSIFICATION:
        ├── Cross-reference with WasteRecords → known waste
        ├── Cross-reference with StockCount variances → counting errors or theft
        ├── Remaining unexplained variance → likely portioning issues or theft
        └── Generate WEPT breakdown pie chart data
```

**Industry benchmark:** Well-managed restaurants target < 2% variance. Variance > 5% requires immediate investigation.

### 8.7 Menu Engineering Calculation Flow

```
IInventoryReportService.GetMenuEngineeringMatrix(restaurantId, startDate, endDate)
    ├── For each Product (menu item):
    │   ├── Sales Volume = count of OrderDetails for this product in period
    │   ├── Recipe Cost = sum(MenuItemStockMapping.QuantityRequired * StockItem.AverageCostPrice)
    │   ├── Selling Price = Product.Price
    │   ├── Contribution Margin = Selling Price − Recipe Cost
    │   ├── Total Revenue = Sales Volume * Selling Price
    │   └── Total Profit = Sales Volume * Contribution Margin
    │
    ├── Calculate averages:
    │   ├── Average Contribution Margin across all items
    │   └── Average Sales Volume (popularity threshold = 70% of average)
    │
    ├── Classify each item:
    │   ├── STAR: Sales > Average AND Margin > Average
    │   ├── PLOWHORSE: Sales > Average AND Margin < Average
    │   ├── PUZZLE: Sales < Average AND Margin > Average
    │   └── DOG: Sales < Average AND Margin < Average
    │
    └── Output: MenuEngineeringReportDto with items plotted on 4-quadrant matrix
        ├── Stars: protect, feature prominently, maintain quality
        ├── Plowhorses: increase price slightly, reduce portions, cheaper ingredients
        ├── Puzzles: rename, reposition on menu, pair with popular items
        └── Dogs: consider removing, offer as limited-time, or de-feature
```

### 8.8 Perpetual Inventory Flow

```
Perpetual inventory runs automatically — no manual counting needed for daily tracking:

1. STOCK IN (increases):
   ├── GoodsReceiving → auto-adds to CurrentQuantity
   ├── StockAdjustment (type: Received/Returned/Correction+) → auto-adds
   └── Refund/cancellation → RevertDeduction auto-adds

2. STOCK OUT (decreases):
   ├── POS sale (BonApp order confirmed) → auto-deducts via IStockDeductionService
   ├── External POS sale (via API/webhook) → auto-deducts via mapping
   ├── WasteRecord → auto-deducts
   ├── StockAdjustment (type: Damaged/Expired/Theft/Transfer) → auto-deducts
   └── Stock transfer to another location → decreases source, increases destination

3. VERIFICATION (periodic physical counts):
   ├── Physical count compares perpetual quantity vs actual on-shelf
   ├── Variance = Actual − Perpetual (expected)
   ├── Corrections applied on count approval → adjusts perpetual quantity
   └── Variance feeds into Actual vs Theoretical analysis
```

---

## 9. Authentication & Authorization

### Own Identity System (Completely Independent)

This application has its **own ASP.NET Identity** setup — NOT shared with BonApp.

#### User Authentication
- **Own `InventoryProDbContext`** inheriting from `IdentityDbContext<AppUser>`
- **JWT Bearer Token** authentication (access token + refresh token)
- **User registration** — `/api/auth/register` with email verification
- **User login** — `/api/auth/login` returns JWT access + refresh tokens
- **Password reset** — Own password reset flow via email
- **Refresh token** — `/api/auth/refresh` for token renewal

#### Roles & Permissions
| Role | Permissions |
|------|------------|
| `Owner` | Full access. Manage organization, users, billing, all inventory features. |
| `Manager` | All inventory operations. Cannot manage billing or delete organization. |
| `Staff` | Record waste, submit stock counts, complete HACCP checklists, view prep lists. Read-only for most other features. |

#### Multi-Tenancy
- Each user belongs to one or more `Organization` (restaurant)
- Organization context established via `OrganizationId` in JWT claims or route parameter
- Row-level security: all queries filtered by `OrganizationId`
- Users can be invited to an organization via email invitation link

#### Auth API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/auth/register` | Register new user + create organization |
| POST | `/api/auth/login` | Login, returns JWT tokens |
| POST | `/api/auth/refresh` | Refresh access token |
| POST | `/api/auth/forgot-password` | Send password reset email |
| POST | `/api/auth/reset-password` | Reset password with token |
| GET | `/api/auth/me` | Get current user profile |
| POST | `/api/auth/invite` | Invite user to organization |
| POST | `/api/auth/accept-invite` | Accept organization invitation |

### External POS API Authentication
- **API Key authentication** via `X-API-Key` header
- Each `PosConnection` has its own unique API key
- API key is hashed (SHA-256) in the database, plain text shown only once on creation
- Rate limiting: 100 requests per minute per API key
- Webhook signature verification using HMAC-SHA256 with the connection's `WebhookSecret`

---

## 10. New Middleware & Infrastructure

| Component | Description |
|-----------|-------------|
| `ApiKeyAuthenticationHandler` | Custom auth handler for `/api/pos/v1/*` endpoints. Validates `X-API-Key` header against hashed keys in `ExternalPosConnection` table. Caches valid keys for performance. |
| `WebhookSignatureMiddleware` | Validates HMAC-SHA256 signature on incoming webhooks from external POS systems. Reads raw body, computes hash, compares with `X-Webhook-Signature` header. |
| `InventoryRateLimitingMiddleware` | Rate limiting for external POS API (100 req/min per API key). Uses ASP.NET Core's built-in `Microsoft.AspNetCore.RateLimiting` with fixed window policy. Returns 429 with `Retry-After` header. |
| `ConcurrencyConflictMiddleware` | Catches `DbUpdateConcurrencyException` from optimistic concurrency conflicts on StockItem updates. Returns 409 Conflict with retry guidance. |
| `StockAlertBackgroundService` | `IHostedService` that runs periodically (every 15 min) to check stock levels against PAR levels and minimum thresholds, generate alerts, and send email notifications. |
| `ExpirationCheckBackgroundService` | `IHostedService` that runs daily to check batch expiration dates and generate alerts for items expiring within configurable days. |
| `HaccpReminderBackgroundService` | `IHostedService` that runs hourly to check for overdue HACCP checklists and send reminders. |
| `InventorySnapshotBackgroundService` | `IHostedService` that runs daily at end-of-business to take automated inventory value snapshots for Actual vs. Theoretical reporting. |
| `OutboundWebhookBackgroundService` | `IHostedService` that processes the outbound webhook queue, sends HTTP requests to subscriber URLs with HMAC-signed payloads, handles retries with exponential backoff (3 attempts), and logs delivery results. |
| `SupplierCertificationCheckService` | `IHostedService` that runs weekly to check for supplier certifications expiring within 30 days and generate alerts/emails. |
| `MenuEngineeringRecalcService` | `IHostedService` that runs weekly to recalculate menu engineering categories (Star/Plowhorse/Puzzle/Dog) for all products based on latest sales and cost data. |

---

## 11. Configuration

### appsettings.json (Standalone Application Config)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=InventoryProDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "super-secret-key-minimum-32-chars",
    "Issuer": "InventoryPro",
    "Audience": "InventoryProApp",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Email": {
    "Provider": "SendGrid",
    "ApiKey": "",
    "FromEmail": "noreply@inventorypro.app",
    "FromName": "Inventory Pro"
  },
  "Inventory": {
    "DefaultCostValuationMethod": "WeightedAverage",
    "DefaultFoodCostTargetPercent": 30,
    "DefaultBeverageCostTargetPercent": 20,
    "DefaultVarianceAlertThresholdPercent": 3,
    "LowStockCheckIntervalMinutes": 15,
    "ExpirationCheckTimeOfDay": "06:00",
    "ExpirationAlertDaysBefore": 3,
    "MaxCsvImportRows": 10000,
    "SnapshotTimeOfDay": "23:00",
    "MenuEngineeringRecalcDay": "Monday",
    "MenuEngineeringPopularityThresholdPercent": 70,
    "HaccpReminderCheckIntervalMinutes": 60,
    "SupplierCertificationCheckDay": "Monday",
    "SupplierCertificationAlertDaysBefore": 30,
    "ExternalApi": {
      "RateLimitPerMinute": 100,
      "MaxBulkSaleItems": 500
    },
    "OutboundWebhooks": {
      "MaxRetries": 3,
      "RetryBaseDelaySeconds": 30,
      "TimeoutSeconds": 10
    }
  }
}
```

### New Options Class

```
InventoryOptions
├── DefaultCostValuationMethod (string)
├── DefaultFoodCostTargetPercent (decimal)
├── DefaultBeverageCostTargetPercent (decimal)
├── DefaultVarianceAlertThresholdPercent (decimal)
├── LowStockCheckIntervalMinutes (int)
├── ExpirationCheckTimeOfDay (string)
├── ExpirationAlertDaysBefore (int)
├── MaxCsvImportRows (int)
├── SnapshotTimeOfDay (string)
├── MenuEngineeringRecalcDay (string)
├── MenuEngineeringPopularityThresholdPercent (decimal)
├── HaccpReminderCheckIntervalMinutes (int)
├── SupplierCertificationCheckDay (string)
├── SupplierCertificationAlertDaysBefore (int)
├── ExternalApiRateLimitPerMinute (int)
├── MaxBulkSaleItems (int)
├── OutboundWebhookMaxRetries (int)
├── OutboundWebhookRetryBaseDelaySeconds (int)
├── OutboundWebhookTimeoutSeconds (int)
```

---

## 12. Testing Plan

### Unit Tests (Estimated: ~60 test files)

| Category | Tests |
|----------|-------|
| **Service Tests** | StockItemService (CRUD, import, export), RecipeService (mapping, deduction calc), SupplierService, PurchaseOrderService (lifecycle), GoodsReceivingService, StockCountService, WasteService, StockDeductionService (deduct, revert), StockAlertService, InventoryReportService, ExternalPosApiService |
| **Validator Tests** | All 10+ validators with positive/negative cases |
| **Mapper Tests** | All entity-to-DTO and DTO-to-entity mappers |
| **Helper Tests** | Cost calculation helpers, CSV parser, unit conversion |
| **Integration Flow Tests** | Order → deduction → revert flow, PO → receiving → stock update flow |

### Integration Tests (Estimated: ~25 test files)

| Handler | Test Scenarios |
|---------|---------------|
| **StockItem Endpoints** | CRUD, pagination, filtering, import/export, low stock |
| **Recipe Endpoints** | Set mappings, deduction preview, product usage |
| **Supplier Endpoints** | CRUD, link stock items |
| **PurchaseOrder Endpoints** | Full lifecycle (draft → submit → receive → close) |
| **StockCount Endpoints** | Full lifecycle (create → count → complete → approve) |
| **Waste Endpoints** | Record, list, summary |
| **Reports Endpoints** | All 8 report types with date filters |
| **External POS API** | API key auth, sale processing, stock queries, rate limiting |
| **Webhook Endpoints** | Signature validation, payload processing |

---

## 13. Development Phases

### Phase 1 — Project Setup, Auth & Core Entities (Weeks 1-3)
- **Create standalone .NET 8.0 solution** (`InventoryPro.sln`) with Clean Architecture layers
- Own ASP.NET Identity (`AppUser`, `Organization`) + JWT auth (register, login, refresh, invite)
- `Organization` entity with all config fields (currency, cost targets, HACCP toggles, etc.)
- `MenuItem` entity with recipe costing fields
- StorageLocation entity + CRUD
- StockItem, StockCategory entities + CRUD endpoints (with PAR levels, storage locations)
- Unit conversion system & alternate count units
- StockMovement audit trail with optimistic concurrency
- Manual stock adjustments (with WEPT reason codes)
- Basic stock level report
- **Dockerize** the application from Day 1

### Phase 2 — Recipe System & POS API (Weeks 4-6)
- MenuItemStockMapping (recipes) CRUD with real-time cost calculation
- External POS API (`/api/pos/v1/`) with API key authentication
- POS connection management (register any POS, generate API key)
- Sale processing via API → stock deduction (perpetual inventory)
- External product mapping (POS product → MenuItem)
- Deduction preview endpoint
- Menu item availability check
- Rate limiting middleware
- `SaleRecord` + `SaleRecordItem` entities for POS data audit trail

### Phase 3 — Suppliers & Purchase Orders (Weeks 7-9)
- Supplier CRUD with compliance/certification tracking
- Purchase Order full lifecycle
- Goods Receiving with stock auto-update, photo capture, temperature logging
- PAR-based auto-reorder suggestions

### Phase 4 — Counting, Waste & Alerts (Weeks 10-12)
- Stock Count sessions with shelf-to-sheet counting (organized by storage location)
- Multi-user simultaneous counting support
- Waste tracking with WEPT classification
- Low stock & PAR alerts (in-app + email)
- Background services for alert generation
- Inventory snapshots (automated daily)

### Phase 5 — Actual vs Theoretical, Menu Engineering & Reports (Weeks 13-15)
- **Actual vs. Theoretical food cost report** (the industry's #1 cost control tool)
- **Menu Engineering Matrix** (Stars, Plowhorses, Puzzles, Dogs)
- All remaining reports (consumption, valuation, COGS, variance, supplier performance)
- CSV/Excel import & export
- PDF generation for reports (QuestPDF)
- Batch/lot tracking
- MenuEngineeringRecalcService background job

### Phase 6 — Webhooks & Advanced Integration (Weeks 16-17)
- Inbound webhook receiver with signature verification
- **Outbound webhook sender** (notify external systems of inventory events)
- Webhook subscription management
- Retry logic with exponential backoff

### Phase 7 — Food Safety & Compliance (Weeks 18-19)
- Allergen tracking at ingredient level (14 EU allergens)
- Auto-calculated allergen and nutritional reports per menu item
- HACCP digital checklists (templates, logs, mobile completion)
- Traceability (ingredient → dish → customer)
- Supplier certification expiration tracking

### Phase 8 — Forecasting, Prep Lists & Polish (Weeks 20-22)
- Demand forecasting from historical POS/sale data
- Auto-generated prep lists
- End-to-end testing
- Performance optimization
- CI/CD pipeline setup (GitHub Actions or Azure DevOps)

---

## 14. File Structure (Standalone Project — Own Repository)

```
inventory-backend/                     (SEPARATE GIT REPO)
├── InventoryPro.sln

InventoryPro.Domain/
├── Entities/
│   ├── Core/                         (Organization, MenuItem, SaleRecord, etc.)
│   │   ├── Organization.cs
│   │   ├── MenuItem.cs
│   │   ├── SaleRecord.cs
│   │   ├── SaleRecordItem.cs
│   ├── Inventory/
│   │   ├── StockItem.cs
│   │   ├── StockCategory.cs
│   │   ├── StorageLocation.cs
│   │   ├── StockItemStorageLocation.cs
│   │   ├── AlternateCountUnit.cs
│   │   ├── UnitConversion.cs
│   │   ├── MenuItem.cs → (already in Core/)
│   │   ├── MenuItemStockMapping.cs
│   │   ├── Supplier.cs
│   │   ├── SupplierStockItem.cs
│   │   ├── SupplierCertification.cs
│   │   ├── StockMovement.cs
│   │   ├── PurchaseOrder.cs
│   │   ├── PurchaseOrderItem.cs
│   │   ├── GoodsReceiving.cs
│   │   ├── GoodsReceivingItem.cs
│   │   ├── StockCount.cs
│   │   ├── StockCountItem.cs
│   │   ├── WasteRecord.cs
│   │   ├── StockAlert.cs
│   │   ├── PosConnection.cs
│   │   ├── PosProductMapping.cs
│   │   ├── WebhookSubscription.cs
│   │   ├── WebhookDeliveryLog.cs
│   │   ├── InventorySnapshot.cs
│   │   └── InventorySnapshotItem.cs
│   ├── FoodSafety/                   (NEW folder)
│   │   ├── AllergenTag.cs
│   │   ├── StockItemAllergen.cs
│   │   ├── StockItemNutrition.cs
│   │   ├── HaccpChecklistTemplate.cs
│   │   ├── HaccpChecklistItem.cs
│   │   ├── HaccpChecklistLog.cs
│   │   └── HaccpChecklistLogItem.cs
│   ├── PrepPlanning/                 (NEW folder)
│   │   ├── PrepList.cs
│   │   └── PrepListItem.cs
├── Enums/
│   ├── UnitOfMeasurement.cs          (NEW)
│   ├── MovementType.cs               (NEW)
│   ├── PurchaseOrderStatus.cs        (NEW)
│   ├── GoodsReceivingStatus.cs       (NEW)
│   ├── StockCountStatus.cs           (NEW)
│   ├── WasteReason.cs                (NEW)
│   ├── AlertType.cs                  (NEW)
│   ├── CostValuationMethod.cs        (NEW)
│   ├── LocationType.cs               (NEW)
│   ├── AllergenSeverity.cs           (NEW)
│   ├── HaccpFrequency.cs             (NEW)
│   ├── HaccpLogStatus.cs             (NEW)
│   ├── PrepListStatus.cs             (NEW)
│   ├── InventoryCountFrequency.cs    (NEW)
│   ├── MenuEngineeringCategory.cs    (NEW)
│   ├── SnapshotType.cs               (NEW)
│   └── WebhookEventType.cs           (NEW)
├── Interfaces/
│   ├── Inventory/                    (NEW folder)
│   │   ├── IStockItemRepository.cs
│   │   ├── IStockCategoryRepository.cs
│   │   ├── IStorageLocationRepository.cs
│   │   ├── ISupplierRepository.cs
│   │   ├── IStockMovementRepository.cs
│   │   ├── IPurchaseOrderRepository.cs
│   │   ├── IStockCountRepository.cs
│   │   ├── IWasteRecordRepository.cs
│   │   ├── IStockAlertRepository.cs
│   │   ├── IPosConnectionRepository.cs
│   │   ├── IHaccpRepository.cs
│   │   ├── IPrepListRepository.cs
│   │   ├── IInventorySnapshotRepository.cs
│   │   └── IWebhookRepository.cs

InventoryPro.Application/
├── Dto/
│   ├── Inventory/                    (NEW folder — ~70 DTO files)
│   ├── FoodSafety/                   (NEW folder — ~15 DTO files)
│   └── PrepPlanning/                 (NEW folder — ~8 DTO files)
├── ServiceContracts/
│   ├── Inventory/                    (NEW folder — 20 interfaces)
│   ├── FoodSafety/                   (NEW folder — 4 interfaces)
│   └── PrepPlanning/                 (NEW folder — 2 interfaces)
├── Validation/
│   └── Inventory/                    (NEW folder — 20+ validators)
├── Mappers/
│   └── Inventory/                    (NEW folder — ~25 mapper files)
├── Configuration/
│   └── InventoryOptions.cs           (NEW)

InventoryPro.Infrastructure/
├── Services/
│   ├── Inventory/                    (NEW folder)
│   │   ├── StockItemService.cs
│   │   ├── StockCategoryService.cs
│   │   ├── StorageLocationService.cs
│   │   ├── UnitConversionService.cs
│   │   ├── RecipeService.cs
│   │   ├── SupplierService.cs
│   │   ├── SupplierComplianceService.cs
│   │   ├── PurchaseOrderService.cs
│   │   ├── GoodsReceivingService.cs
│   │   ├── StockCountService.cs
│   │   ├── WasteService.cs
│   │   ├── StockMovementService.cs
│   │   ├── StockAlertService.cs
│   │   ├── StockDeductionService.cs
│   │   ├── InventoryReportService.cs
│   │   ├── InventorySnapshotService.cs
│   │   ├── CsvImportService.cs
│   │   ├── CsvExportService.cs
│   │   ├── PosConnectionService.cs
│   │   ├── PosProductMappingService.cs
│   │   ├── PosApiService.cs
│   │   ├── WebhookReceiverService.cs
│   │   └── OutboundWebhookService.cs
│   ├── FoodSafety/                   (NEW folder)
│   │   ├── AllergenService.cs
│   │   ├── NutritionService.cs
│   │   ├── HaccpService.cs
│   │   └── TraceabilityService.cs
│   ├── PrepPlanning/                 (NEW folder)
│   │   ├── DemandForecastService.cs
│   │   └── PrepListService.cs
├── BackgroundServices/
│   ├── StockAlertBackgroundService.cs         (NEW)
│   ├── ExpirationCheckBackgroundService.cs    (NEW)
│   ├── HaccpReminderBackgroundService.cs      (NEW)
│   ├── InventorySnapshotBackgroundService.cs  (NEW)
│   ├── OutboundWebhookBackgroundService.cs    (NEW)
│   ├── SupplierCertificationCheckService.cs   (NEW)
│   └── MenuEngineeringRecalcService.cs        (NEW)

InventoryPro.Api/
├── Handlers/
│   ├── Inventory/                    (NEW folder)
│   │   ├── StockItemEndpointsHandler.cs
│   │   ├── StockCategoryEndpointsHandler.cs
│   │   ├── StorageLocationEndpointsHandler.cs
│   │   ├── RecipeEndpointsHandler.cs
│   │   ├── SupplierEndpointsHandler.cs
│   │   ├── PurchaseOrderEndpointsHandler.cs
│   │   ├── StockCountEndpointsHandler.cs
│   │   ├── WasteEndpointsHandler.cs
│   │   ├── InventoryReportEndpointsHandler.cs
│   │   ├── StockAlertEndpointsHandler.cs
│   │   ├── PosConnectionEndpointsHandler.cs
│   │   ├── PosApiEndpointsHandler.cs
│   │   ├── AuthEndpointsHandler.cs
│   │   ├── FoodSafetyEndpointsHandler.cs
│   │   ├── HaccpEndpointsHandler.cs
│   │   └── PrepListEndpointsHandler.cs
├── Middleware/
│   ├── ApiKeyAuthenticationHandler.cs    (NEW)
│   ├── WebhookSignatureMiddleware.cs     (NEW)
│   └── ConcurrencyConflictMiddleware.cs  (NEW)

InventoryPro.DataAccess/
├── Repository/
│   └── Inventory/                    (NEW folder — 14 repository implementations)
├── Migrations/
│   └── (22 new migrations)
```

---

## 15. Estimated Scope Summary

| Category | Count |
|----------|-------|
| Domain Entities (all new — own DB) | 38 (incl. Organization, MenuItem, SaleRecord, AppUser) |
| Domain Enums | 19 |
| API Endpoints | ~120+ (incl. auth endpoints) |
| Service Interfaces | 30 (incl. auth, email, PDF) |
| Service Implementations | 32 |
| Repository Interfaces | 16 |
| Repository Implementations | 16 |
| Validators | 22+ |
| DTOs | ~100 |
| Mappers | ~28 |
| Database Migrations | 24 |
| Background Services | 7 |
| Middleware | 4 (incl. multi-tenant filter) |
| Auth Controllers/Endpoints | 8 |
| Estimated Unit Test Files | ~100 |
| Estimated Integration Test Files | ~45 |
| **Total .cs Files (est.)** | **~500+** |

**Note:** This is a COMPLETE standalone application. Unlike a module added to an existing codebase, this includes its own `Program.cs`, `DbContext`, Identity setup, email service, PDF generation, Docker configuration, and CI/CD pipeline.

---

## 16. Industry Benchmarks & Competitive Feature Comparison

This system is designed to compete with the following industry leaders:

| Feature | MarketMan | Apicbase | CrunchTime | Restaurant365 | **Inventory Pro** |
|---------|-----------|----------|------------|----------------|---------------------|
| Perpetual Inventory | Yes | Yes | Yes | Yes | **Yes** |
| Actual vs. Theoretical | Yes | Yes | Yes | Yes | **Yes** |
| Menu Engineering Matrix | No | Yes | Yes | Yes | **Yes** |
| PAR Level Management | Yes | No | Yes | Yes | **Yes** |
| Shelf-to-Sheet Counting | No | No | Yes | Yes | **Yes** |
| Alternate Count Units | No | No | Yes | Yes | **Yes** |
| Multi-user Counting | No | No | Yes | Yes | **Yes** |
| HACCP Checklists | No | Yes | No | No | **Yes** |
| Allergen Tracking | No | Yes | No | No | **Yes** |
| Nutritional Calculation | No | Yes | No | No | **Yes** |
| Prep List Generation | No | Yes | Yes | No | **Yes** |
| Outbound Webhooks | No | Yes | No | No | **Yes** |
| External POS API | Yes (limited) | Yes | No | No | **Yes (any POS)** |
| Supplier Compliance | No | No | No | No | **Yes** |
| Multi-location Transfers | Yes | Yes | Yes | Yes | **Yes** |
| Demand Forecasting | No | No | Yes | No | **Yes** |
| POS-Agnostic (any POS) | No | No | No | No | **Yes** |
| Self-hosted Option | No | No | No | No | **Yes** |

### Key Differentiators
1. **Fully standalone & POS-agnostic** — works with ANY POS system (BonApp, Lightspeed, Toast, Square, or none at all)
2. **External POS API** with full OpenAPI documentation, API key auth, and outbound webhooks — any POS can integrate
3. **Complete food safety suite** — allergens, nutrition, HACCP, traceability (only Apicbase has this)
4. **Menu Engineering + Actual vs. Theoretical** combined (most competitors have one or the other)
5. **Swiss market optimized** — CHF currency, EU allergen compliance, multi-language (EN/DE/FR)
6. **Future BonApp integration** — seamlessly connects to BonApp POS via the same API when ready

---

*Report generated for Inventory Pro - Standalone Restaurant Inventory Management System - February 9, 2026 (Updated: standalone architecture)*
