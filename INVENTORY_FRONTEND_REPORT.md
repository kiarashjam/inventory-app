# Inventory Pro - Frontend Development Report

**Generated:** February 9, 2026 (Updated)  
**Project:** Inventory Pro — Standalone Restaurant Inventory Management System  
**Framework:** React 19 | Vite 5 | TypeScript  
**Architecture:** Standalone React application (NOT part of BonApp monorepo)  
**Repository:** `inventory-frontend` (separate Git repository)  
**Status:** Phase 2 In Progress

---

## IMPLEMENTATION STATUS (Updated Feb 9, 2026)

### ✅ Completed Pages & Components

| Component | Status | Notes |
|-----------|--------|-------|
| **Project Setup** | ✅ Done | Vite + React 19 + TypeScript + Ant Design 6 |
| **Ant Design Theme** | ✅ Done | Custom tokens in `antdTheme.ts` |
| **Axios Config** | ✅ Done | JWT interceptors, 401 handler with persist cleanup |
| **Redux Store** | ✅ Done | `configureStore` with persist (auth, inventory, ui slices) |
| **Auth Slice** | ✅ Done | login, register, getMe thunks + logout, clearError |
| **Inventory Slice** | ✅ Done | fetchStockItems, fetchCategories, fetchStorageLocations |
| **UI Slice** | ✅ Done | lang, sidebarCollapsed, mobileMenuOpen |
| **i18n Setup** | ✅ Done | i18next with EN translations, browser language detection |
| **Login Page** | ✅ Done | Email/password, redirect if authenticated |
| **Register Page** | ✅ Done | First/Last name, email, password, org name |
| **AppLayout** | ✅ Done | Header (org name, notifications, user dropdown) + Outlet |
| **AppSidebar** | ✅ Done | Full navigation menu with all sections |
| **Dashboard** | ✅ Done | Total Items, Low Stock, Out of Stock, Pending Orders + Quick Actions |
| **Stock Item List** | ✅ Done | Paginated table, search, status tags, edit/delete |
| **Stock Item Form** | ✅ Done | Create/edit with category, supplier, location selects |
| **Category List** | ✅ Done | Table with name, order, parent, active, edit/delete |
| **Category Form** | ✅ Done | Create/edit with parent category select |
| **Storage Location List** | ✅ Done | Table with type tags, temp range, edit/delete |
| **Storage Location Form** | ✅ Done | Create/edit with location type select |
| **Supplier List** | ✅ Done | Table with search, contact info, edit/delete |
| **Supplier Form** | ✅ Done | Full form: contact, address, terms, lead time |
| **Purchase Order List** | ✅ Done | Paginated table with status tags, actions by status |
| **Purchase Order Form** | ✅ Done | Supplier select, date picker, dynamic line items |
| **Purchase Order Detail** | ✅ Done | Info display, items table, Submit/Cancel/Receive modal |
| **Waste Record List** | ✅ Done | Summary cards, table with WEPT reason tags |
| **Waste Record Form** | ✅ Done | Stock item select, quantity, reason, cost preview |
| **Stock Count List** | ✅ Done | Table with status tags, variance values |
| **Stock Count Start** | ✅ Done | Notes, optional category/location filters |
| **Stock Count Detail** | ✅ Done | Editable quantities, submit/complete/approve flows |
| **Alert List** | ✅ Done | Type tags, mark read, mark all read, pagination |
| **ProtectedRoute** | ✅ Done | Checks isAuthenticated + localStorage token |
| **GuestRoute (redirect)** | ✅ Done | Login/Register redirect to dashboard if authenticated |
| **Routing** | ✅ Done | All implemented pages wired in App.tsx |

### ❌ Not Yet Implemented (Remaining Phases)

| Component | Phase | Priority |
|-----------|-------|----------|
| **Stock Item Detail Page** (tabbed view) | Phase 2 | P0 |
| **Stock Adjustment Modal** | Phase 2 | P0 |
| **Stock Movement History Page** | Phase 2 | P0 |
| **Menu Item Pages** (CRUD) | Phase 2 | P0 |
| **Recipe/BOM Pages** (CRUD + live cost calc) | Phase 2 | P0 |
| **Supplier Detail Page** (performance, certs) | Phase 3 | P1 |
| **Reorder Suggestions Page** | Phase 3 | P1 |
| **Forgot/Reset Password Pages** | Phase 3 | P1 |
| **Accept Invite Page** | Phase 3 | P1 |
| **Organization Switcher** | Phase 3 | P1 |
| **Food Safety Overview** | Phase 6 | P2 |
| **Allergen Dashboard** | Phase 6 | P1 |
| **HACCP Pages** (templates, entry, logs) | Phase 6 | P2 |
| **Traceability Search** | Phase 6 | P2 |
| **Prep List Pages** (overview, generator, detail) | Phase 6 | P2 |
| **All Report Pages** (11 report types) | Phase 5 | P0-P1 |
| **Settings Pages** (general, org, users, food safety) | Phase 7 | P1 |
| **POS Connection Pages** | Phase 7 | P1 |
| **Product Mapping Manager** | Phase 7 | P1 |
| **Webhook Subscription Manager** | Phase 7 | P1 |
| **Reusable Components** (StockLevelBar, badges, etc.) | Phase 2-5 | P1 |
| **Custom Hooks** (17 planned) | Phase 2-7 | P1 |
| **API Modules** (17 centralized) | Phase 2-7 | P1 |
| **Chart.js Integration** | Phase 5 | P1 |
| **CSV Import/Export** | Phase 5 | P1 |
| **PDF Export** | Phase 5 | P2 |
| **Barcode Scanner** | Phase 4 | P1 |
| **Offline Support** (IndexedDB) | Phase 4 | P2 |
| **Dashboard Widgets** (drag-and-drop) | Phase 2 | P1 |
| **Mobile Bottom Nav** | Phase 3 | P1 |
| **FR/DE Translations** | Phase 7 | P1 |

### Frontend File Count
- **Implemented:** ~30 TSX files (2 auth + 1 dashboard + 2 stock items + 2 categories + 2 storage + 2 suppliers + 3 purchase orders + 2 waste + 3 stock counts + 1 alerts + 2 layout + 1 App + 1 main) + ~10 TS config files
- **Remaining:** ~165+ TSX files, ~75 TS files as per full plan

---

## 1. Executive Summary

This report defines the complete frontend development plan for **Inventory Pro**, a fully **autonomous, standalone** restaurant inventory management frontend. This is a **completely independent application** — it has its own codebase, dependencies, design system, authentication UI, and deployment. It does NOT live inside the BonApp frontend monorepo.

The application is designed for **any restaurant** regardless of their POS system:
- Restaurants using **BonApp POS** (future integration via API)
- Restaurants using **Lightspeed, Toast, Square, Orderbird**
- Restaurants using **any POS system** with API/webhook support
- Restaurants with **no POS system** at all (manual inventory management)

### Architectural Independence

- **Own Git repository** — `inventory-frontend/` is a standalone project
- **Own design system** — Based on Ant Design 5, NOT `@repo/ui`
- **Own authentication** — Login, register, forgot password (talks to own Inventory Pro backend)
- **Own deployment** — Separate Docker build, separate CDN, separate CI/CD pipeline
- **No shared code** — No `@repo/ui`, `@repo/configs`, or `@repo/eslint-config` dependencies
- **No cross-app navigation** — No links to/from BonApp Manager, Waiter, or Customer apps

### Design Principles

- **Mobile-first responsive design** — Many inventory tasks (stock counts, waste recording, goods receiving, HACCP checklists) happen in the kitchen/storage room, not at a desk
- **Offline-capable** — Critical operations (stock counting, HACCP checklists) should work offline and sync when connection resumes
- **Fast data entry** — Barcode scanner support, quick-add patterns, batch operations, alternate count units for faster counting
- **Data visualization over text** — Dashboard uses charts, color-coded bars, and visual indicators so operators understand metrics at a glance
- **Customizable dashboard** — Drag-and-drop widgets so each user can prioritize the most important information for their daily operations
- **Clear visual hierarchy** — Dashboard shows what needs attention (low stock, pending orders, alerts, overdue HACCP) at a glance
- **Own design system** — Ant Design 5 as base, with custom theme tokens for inventory-specific colors and components
- **Shelf-to-sheet counting** — Count interface organized by physical storage location so staff walks through in order (industry best practice)

---

## 2. Application Configuration

### Standalone Project Setup

| Setting | Value |
|---------|-------|
| **App Name** | `inventory-pro` |
| **Location** | `inventory-frontend/` (own Git repository) |
| **Dev Port** | 3000 |
| **Base Path** | `/` |
| **Language** | TypeScript (.tsx) |
| **React Version** | ^18.3.1 |
| **Vite Version** | ^5.3.1 |
| **UI Library** | Ant Design 5 (own theme — NOT @repo/ui) |
| **State Management** | Redux Toolkit |
| **Redux Persist** | Yes (auth, settings, ui) |
| **Router** | React Router ^7.x |
| **Forms** | Formik + Yup |
| **i18n Fallback** | English ("en") |
| **Charts** | Chart.js ^4.x (for reports/analytics) |
| **Tables** | Ant Design Table + custom virtual scrolling for large datasets |
| **Date Handling** | dayjs |
| **Animation** | Framer Motion |
| **PDF Export** | html2pdf.js |
| **CSV/Excel** | SheetJS (xlsx) for import/export |
| **Barcode** | quagga2 or html5-qrcode for barcode scanning |
| **Notifications** | react-hot-toast |

### Package Manager

```bash
# Standalone project — uses npm or pnpm directly (no monorepo)
npm create vite@latest inventory-frontend -- --template react-ts
cd inventory-frontend
npm install
```

### CI/CD Pipeline

GitHub Actions or Azure DevOps (standalone pipeline):
- Trigger: `main`, `develop`, `feature/*`
- Steps: Install → Lint → Test → Build → Deploy
- Deployment: Docker → Azure App Service / Vercel / Netlify

---

## 3. Dependencies

### Production Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `react` | ^18.3.1 | UI framework |
| `react-dom` | ^18.3.1 | DOM rendering |
| `react-router` | ^7.1.3 | Routing (v7, match Manager) |
| `@reduxjs/toolkit` | ^2.2.6 | State management |
| `react-redux` | ^9.1.2 | React-Redux bindings |
| `redux-persist` | ^6.0.0 | State persistence |
| `axios` | ^1.6.8 | HTTP client |
| `formik` | ^2.4.6 | Form handling |
| `yup` | ^1.4.0 | Schema validation |
| `antd` | ^5.22.1 | UI component library |
| `@ant-design/icons` | ^5.5.2 | Icons |
| `@headlessui/react` | ^2.2.0 | Accessible primitives |
| `chart.js` | ^4.4.3 | Charts for reports |
| `react-chartjs-2` | ^5.2.0 | Chart.js React bindings |
| `framer-motion` | ^11.3.31 | Animations |
| `react-hot-toast` | ^2.4.1 | Toast notifications |
| `dayjs` | ^1.11.13 | Date handling |
| `i18next` | ^23.11.4 | Internationalization |
| `i18next-browser-languagedetector` | ^8.0.0 | Language detection |
| `i18next-http-backend` | ^2.5.1 | Translation loading |
| `react-i18next` | ^14.1.1 | React i18n bindings |
| `localforage` | ^1.10.0 | Offline storage |
| `xlsx` | ^0.18.5 | CSV/Excel import/export |
| `file-saver` | ^2.0.5 | File downloads |
| `html2pdf.js` | ^0.10.1 | PDF export |
| `lodash` | ^4.17.21 | Utility functions |
| `tailwind-merge` | ^2.5.2 | Tailwind class merging |
| `@hello-pangea/dnd` | ^17.0.0 | Drag and drop (for reordering and dashboard widgets) |
| `recharts` | ^2.12.0 | Alternative charts for complex visualizations (menu engineering scatter plot) |
| `react-grid-layout` | ^1.4.4 | Drag-and-drop dashboard widget layout |
| `react-webcam` | ^7.2.0 | Camera capture for goods receiving photos and HACCP evidence |
| `idb` | ^8.0.0 | IndexedDB wrapper for offline data (stock counts, HACCP checklists) |

### Dev Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| `vite` | ^5.3.1 | Build tool |
| `@vitejs/plugin-react` | ^4.3.1 | React Vite plugin |
| `vite-plugin-svgr` | ^4.2.0 | SVG as React components |
| `sass` | ^1.77.1 | SCSS preprocessor |
| `vitest` | ^1.6.0 | Unit testing |
| `@testing-library/react` | ^15.0.7 | Component testing |
| `@testing-library/jest-dom` | ^6.4.5 | Jest DOM matchers |
| `jsdom` | ^24.0.0 | Test DOM environment |

---

## 4. Application Structure

### Router Configuration

```
basename: "/"

/login → LoginPage (own auth — NOT BonApp's Login app)
/register → RegisterPage (create account + organization)
/forgot-password → ForgotPasswordPage
/reset-password → ResetPasswordPage

/ → redirect to /dashboard (if authenticated)
/dashboard → Dashboard (customizable drag-and-drop widgets)

/stock-items → StockItemList (paginated, filterable, with PAR indicators)
  /new → StockItemForm (create — with allergens, nutrition, alternate units)
  /:id → StockItemDetail (view/edit with tabs)
  /:id/edit → StockItemForm (edit)
  /:id/movements → StockMovementHistory
  /:id/allergens → StockItemAllergenManager

/categories → StockCategoryList
  /new → StockCategoryForm
  /:id/edit → StockCategoryForm

/storage-locations → StorageLocationList (physical storage areas)
  /new → StorageLocationForm
  /:id → StorageLocationDetail (items at this location)
  /:id/edit → StorageLocationForm

/menu-items → MenuItemList (manage own menu items — NOT BonApp Products)
  /new → MenuItemForm (create)
  /:id → MenuItemDetail
  /:id/edit → MenuItemForm (edit)

/recipes → RecipeList (menu items with mappings + food cost % + menu engineering category)
  /:menuItemId → RecipeDetail (ingredients, cost breakdown, allergens, nutrition)
  /:menuItemId/edit → RecipeForm

/suppliers → SupplierList
  /new → SupplierForm (with certifications)
  /:id → SupplierDetail (performance metrics, certifications)
  /:id/edit → SupplierForm
  /:id/certifications → SupplierCertificationManager

/purchase-orders → PurchaseOrderList
  /new → PurchaseOrderForm
  /:id → PurchaseOrderDetail
  /:id/edit → PurchaseOrderForm (draft only)
  /:id/receive → GoodsReceivingForm (with photo capture + temperature)

/stock-counts → StockCountList
  /new → StockCountForm (select locations for shelf-to-sheet counting)
  /:id → StockCountDetail (variance analysis with WEPT classification)
  /:id/count → StockCountEntry (mobile-optimized, organized by storage location)

/waste → WasteRecordList (with WEPT categorization)
  /new → WasteRecordForm (quick entry with barcode scan)

/prep-lists → PrepListOverview
  /generate → PrepListGenerator (select date, review forecast)
  /:id → PrepListDetail (assignable tasks, completion tracking)

/food-safety → FoodSafetyOverview
  /allergens → AllergenDashboard (view allergens across all products)
  /haccp → HaccpChecklistList
  /haccp/templates → HaccpTemplateManager
  /haccp/templates/new → HaccpTemplateForm
  /haccp/:logId → HaccpChecklistLogDetail
  /haccp/complete/:templateId → HaccpChecklistEntry (mobile-optimized completion form)
  /traceability → TraceabilitySearch (trace ingredient → dishes, or dish → suppliers)

/reports → ReportsOverview
  /stock-levels → StockLevelReport
  /movements → MovementHistoryReport
  /consumption → ConsumptionReport
  /valuation → ValuationReport
  /waste-analysis → WasteAnalysisReport (WEPT breakdown)
  /cogs → COGSReport
  /variance → VarianceReport
  /actual-vs-theoretical → ActualVsTheoreticalReport ★ NEW (industry's #1 cost control report)
  /menu-engineering → MenuEngineeringReport ★ NEW (4-quadrant scatter plot)
  /supplier-performance → SupplierPerformanceReport

/alerts → AlertList

/settings → InventorySettings
  /general → GeneralSettings (valuation method, food cost targets, variance thresholds, etc.)
  /organization → OrganizationSettings (restaurant name, address, currency, timezone)
  /users → UserManagement (invite users, manage roles — Owner/Manager/Staff)
  /storage-locations → StorageLocationSettings
  /food-safety → FoodSafetySettings (HACCP, allergens, nutrition toggles)
  /integrations → IntegrationSettings
    /connections → PosConnectionList (any POS system)
    /connections/new → PosConnectionForm
    /connections/:id → PosConnectionDetail
    /connections/:id/mappings → ProductMappingManager
    /connections/:id/webhooks → WebhookSubscriptionManager

* → NotFound / redirect to /dashboard
```

### Router Files (16 files — all .tsx)

| File | Routes |
|------|--------|
| `AuthRouter.tsx` | `/login`, `/register`, `/forgot-password`, `/reset-password` |
| `DashboardRouter.tsx` | `/dashboard` |
| `StockItemRouter.tsx` | `/stock-items/*` |
| `CategoryRouter.tsx` | `/categories/*` |
| `StorageLocationRouter.tsx` | `/storage-locations/*` |
| `MenuItemRouter.tsx` | `/menu-items/*` |
| `RecipeRouter.tsx` | `/recipes/*` |
| `SupplierRouter.tsx` | `/suppliers/*` |
| `PurchaseOrderRouter.tsx` | `/purchase-orders/*` |
| `StockCountRouter.tsx` | `/stock-counts/*` |
| `WasteRouter.tsx` | `/waste/*` |
| `PrepListRouter.tsx` | `/prep-lists/*` |
| `FoodSafetyRouter.tsx` | `/food-safety/*` |
| `ReportsRouter.tsx` | `/reports/*` |
| `AlertRouter.tsx` | `/alerts/*` |
| `SettingsRouter.tsx` | `/settings/*` |

---

## 5. Redux Store Design

### Store Configuration

```javascript
const store = configureStore({
  reducer: {
    auth: authReducer,               // OWN auth (register, login, JWT tokens)
    inventory: inventoryReducer,     // stock items, categories, storage locations
    recipes: recipesReducer,         // product-stock mappings, menu engineering
    suppliers: suppliersReducer,     // suppliers, certifications
    purchaseOrders: purchaseOrdersReducer,
    stockCounts: stockCountsReducer, // counting with shelf-to-sheet
    waste: wasteReducer,             // waste with WEPT classification
    prepLists: prepListsReducer,     // prep list generation & tracking
    foodSafety: foodSafetyReducer,   // allergens, nutrition, HACCP
    reports: reportsReducer,         // includes actual vs theoretical, menu engineering
    alerts: alertsReducer,
    settings: settingsReducer,
    integrations: integrationsReducer, // connections + webhook subscriptions
    ui: uiReducer,
  },
});
```

**Redux Persist:** Persist `auth`, `settings`, and `ui` (dashboard widget layout) slices.

### Slice Definitions

#### auth (own auth system — NOT shared with BonApp)

| State Property | Type | Description |
|---------------|------|-------------|
| `user` | object | User profile (name, email, role: Owner/Manager/Staff) |
| `accessToken` | string | JWT Bearer token (from own backend) |
| `refreshToken` | string | Refresh token |
| `isAuthenticated` | bool | Auth status |
| `organizationId` | int | Current organization (restaurant) context |
| `organizationName` | string | Restaurant name |
| `organizations` | array | List of organizations user belongs to (for org switcher) |
| `lang` | string | Current language |
| `status` | string | Auth status (idle/loading/succeeded/failed) |
| `error` | string | Auth error message |

#### inventory

| State Property | Type | Description |
|---------------|------|-------------|
| `stockItems` | array | Paginated stock item list |
| `selectedStockItem` | object | Currently viewed/edited stock item |
| `categories` | array | Stock categories tree |
| `storageLocations` | array | Physical storage locations |
| `lowStockItems` | array | Items below PAR level or minimum threshold |
| `belowParItems` | array | Items below PAR level but above minimum |
| `totalCount` | int | Total items for pagination |
| `filters` | object | Current filter state (search, category, status, storageLocation, lowStockOnly, belowPar) |
| `pagination` | object | Current page, pageSize |
| `sortBy` | string | Sort column |
| `sortOrder` | string | asc/desc |
| `isLoading` | bool | List loading state |
| `detailLoading` | bool | Detail loading state |
| `postInProgress` | bool | Create/update in progress |
| `error` | string | Error message |
| `importResult` | object | CSV import result |
| `movements` | array | Stock movements for selected item |

#### recipes

| State Property | Type | Description |
|---------------|------|-------------|
| `products` | array | Products with recipe status |
| `selectedRecipe` | object | Current product's recipe (ingredients list) |
| `deductionPreview` | array | Preview of stock deduction |
| `isLoading` | bool | Loading |
| `postInProgress` | bool | Saving |
| `error` | string | Error |

#### suppliers

| State Property | Type | Description |
|---------------|------|-------------|
| `suppliers` | array | Supplier list |
| `selectedSupplier` | object | Current supplier detail |
| `supplierStockItems` | array | Stock items linked to supplier |
| `isLoading` | bool | Loading |
| `postInProgress` | bool | Saving |
| `error` | string | Error |

#### purchaseOrders

| State Property | Type | Description |
|---------------|------|-------------|
| `orders` | array | PO list (paginated) |
| `selectedOrder` | object | Current PO detail |
| `reorderSuggestions` | array | Auto-reorder suggestions |
| `draftItems` | array | Items being added to a new PO |
| `receivingItems` | array | Items being received |
| `filters` | object | Status, supplier, date filters |
| `pagination` | object | Current page, pageSize |
| `isLoading` | bool | Loading |
| `postInProgress` | bool | Saving |
| `error` | string | Error |

#### stockCounts

| State Property | Type | Description |
|---------------|------|-------------|
| `counts` | array | Count session list |
| `activeCount` | object | Currently active count session |
| `countItems` | array | Items being counted |
| `searchTerm` | string | Filter count items by name/SKU |
| `isLoading` | bool | Loading |
| `postInProgress` | bool | Saving |
| `error` | string | Error |

#### waste

| State Property | Type | Description |
|---------------|------|-------------|
| `records` | array | Waste record list (paginated) |
| `summary` | object | Waste summary statistics |
| `filters` | object | Date range, reason, stock item |
| `pagination` | object | Current page, pageSize |
| `isLoading` | bool | Loading |
| `postInProgress` | bool | Saving |
| `error` | string | Error |

#### reports

| State Property | Type | Description |
|---------------|------|-------------|
| `stockLevelReport` | object | Current stock level data |
| `consumptionReport` | object | Consumption analysis data |
| `valuationReport` | object | Inventory valuation data |
| `wasteAnalysis` | object | Waste analysis data (WEPT breakdown) |
| `cogsReport` | object | COGS data |
| `varianceReport` | object | Variance data |
| `actualVsTheoreticalReport` | object | Actual vs. Theoretical food cost data (industry's #1 report) |
| `menuEngineeringReport` | object | Menu engineering matrix data (Stars, Plowhorses, Puzzles, Dogs) |
| `supplierPerformanceReport` | object | Supplier scorecard data |
| `dateRange` | object | Selected date range for reports |
| `isLoading` | bool | Loading |
| `error` | string | Error |

#### alerts

| State Property | Type | Description |
|---------------|------|-------------|
| `alerts` | array | Alert list |
| `unreadCount` | int | Unread alert badge count |
| `filters` | object | Type, read status |
| `isLoading` | bool | Loading |
| `error` | string | Error |

#### settings

| State Property | Type | Description |
|---------------|------|-------------|
| `inventorySettings` | object | Organization inventory config |
| `costValuationMethod` | string | FIFO/LIFO/WeightedAverage |
| `autoDeductOnSale` | bool | Auto-deduct on POS sale |
| `lowStockAlertEmail` | bool | Email alert setting |
| `isLoading` | bool | Loading |
| `postInProgress` | bool | Saving |
| `error` | string | Error |

#### prepLists

| State Property | Type | Description |
|---------------|------|-------------|
| `prepLists` | array | Prep list summaries |
| `selectedPrepList` | object | Current prep list detail with items |
| `generationPreview` | object | Preview before generating (forecasted items) |
| `filters` | object | Date, status filters |
| `isLoading` | bool | Loading |
| `postInProgress` | bool | Saving |
| `error` | string | Error |

#### foodSafety

| State Property | Type | Description |
|---------------|------|-------------|
| `allergenTags` | array | Master list of 14 EU allergens |
| `stockItemAllergens` | array | Allergens for selected stock item |
| `productAllergenReport` | object | Auto-calculated allergens for a menu item |
| `productNutritionReport` | object | Auto-calculated nutrition for a menu item |
| `haccpTemplates` | array | HACCP checklist templates |
| `haccpLogs` | array | Completed checklist logs |
| `haccpDueToday` | array | Checklists due today |
| `selectedHaccpLog` | object | Current checklist log detail |
| `traceabilityResult` | object | Traceability search result |
| `isLoading` | bool | Loading |
| `postInProgress` | bool | Saving |
| `error` | string | Error |

#### integrations

| State Property | Type | Description |
|---------------|------|-------------|
| `connections` | array | External POS connections |
| `selectedConnection` | object | Current connection detail |
| `productMappings` | array | External product mappings |
| `webhookSubscriptions` | array | Outbound webhook subscriptions |
| `newApiKey` | string | Newly generated API key (shown once) |
| `isLoading` | bool | Loading |
| `postInProgress` | bool | Saving |
| `error` | string | Error |

#### ui

| State Property | Type | Description |
|---------------|------|-------------|
| `lang` | string | Current language |
| `sidebarCollapsed` | bool | Sidebar state |
| `mobileMenuOpen` | bool | Mobile menu state |
| `activeModal` | string | Currently open modal ID |
| `isLoading` | bool | Global loading overlay |
| `dashboardLayout` | array | Saved dashboard widget positions/sizes (react-grid-layout) |
| `dashboardWidgets` | array | Which widgets are visible on dashboard |

---

## 6. API Layer (Centralized)

Centralized API modules (not scattered thunks):

### API Modules (17 files in `src/api/`)

| Module | Functions |
|--------|----------|
| `authApi.ts` | register, login, refreshToken, forgotPassword, resetPassword, getMe, inviteUser, acceptInvite |
| `menuItemApi.ts` | getMenuItems, getMenuItem, createMenuItem, updateMenuItem, deleteMenuItem, syncFromPos |
| `stockItemApi.ts` | getStockItems, getStockItem, createStockItem, updateStockItem, deleteStockItem, adjustStock, importCsv, exportCsv, getLowStockItems, getStockMovements |
| `stockCategoryApi.js` | getCategories, createCategory, updateCategory, deleteCategory |
| `storageLocationApi.js` | getLocations, createLocation, updateLocation, deleteLocation, getStockAtLocation, transferStock |
| `recipeApi.js` | getRecipe, setRecipeMappings, removeMapping, getProductsUsingStockItem, calculateDeductionPreview |
| `supplierApi.js` | getSuppliers, getSupplier, createSupplier, updateSupplier, deleteSupplier, getSupplierStockItems, linkStockItems, getCertifications, addCertification, updateCertification |
| `purchaseOrderApi.js` | getPurchaseOrders, getPurchaseOrder, createPurchaseOrder, updatePurchaseOrder, submitPurchaseOrder, cancelPurchaseOrder, receiveGoods, getReorderSuggestions, generateFromSuggestions |
| `stockCountApi.js` | getStockCounts, getStockCount, startCount, submitCountItems, completeCount, approveCount, cancelCount |
| `wasteApi.js` | getWasteRecords, createWasteRecord, getWasteSummary |
| `prepListApi.js` | generatePrepList, getPrepLists, getPrepList, updatePrepItemCompletion, completePrepList |
| `foodSafetyApi.js` | getAllergenTags, getStockItemAllergens, setStockItemAllergens, getProductAllergens, getStockItemNutrition, setStockItemNutrition, getProductNutrition |
| `haccpApi.js` | getTemplates, createTemplate, updateTemplate, deleteTemplate, getLogs, submitLog, getLogDetail, getDueToday |
| `reportApi.js` | getStockLevelReport, getMovementHistory, getConsumptionReport, getValuationReport, getWasteAnalysis, getCOGSReport, getVarianceReport, **getActualVsTheoreticalReport**, **getMenuEngineeringReport**, getSupplierPerformanceReport, takeSnapshot, getSnapshots, printStockReport |
| `alertApi.js` | getAlerts, markAsRead, markAllAsRead, dismissAlert |
| `integrationApi.js` | getConnections, createConnection, updateConnection, deleteConnection, regenerateApiKey, getProductMappings, setProductMappings, getWebhookSubscriptions, createWebhookSubscription, deleteWebhookSubscription |
| `traceabilityApi.js` | traceForward, traceBackward, generateRecallReport |

### Axios Configuration

```typescript
// src/config/axios/axios-config.ts
// Standalone config — talks to Inventory Pro backend:
// - Base URL from VITE_API_BASE_URL (own backend, NOT BonApp)
// - Request interceptor: attach JWT token, auto-refresh if expiring within 5 min
// - Response interceptor: 401 → redirect to /login
// - Exports: authorizedAxios, unauthorizedAxios
```

---

## 7. Thunks (Redux Async Actions)

### Thunk Files (16 files in `src/redux/thunks/`)

| File | Thunks |
|------|--------|
| `authThunk.ts` | register, login, logout, getMe, refreshToken, forgotPassword, resetPassword, inviteUser, acceptInvite |
| `inventoryThunk.js` | fetchStockItems, fetchStockItem, createStockItem, updateStockItem, deleteStockItem, adjustStock, importCsv, exportCsv, fetchLowStockItems, fetchStockMovements |
| `categoryThunk.js` | fetchCategories, createCategory, updateCategory, deleteCategory |
| `storageLocationThunk.js` | fetchLocations, createLocation, updateLocation, deleteLocation, fetchStockAtLocation, transferStock |
| `recipeThunk.js` | fetchRecipe, setRecipeMappings, removeMapping, fetchProductsUsingStockItem, calculateDeductionPreview |
| `supplierThunk.js` | fetchSuppliers, fetchSupplier, createSupplier, updateSupplier, deleteSupplier, fetchSupplierStockItems, linkStockItems, fetchCertifications, addCertification |
| `purchaseOrderThunk.js` | fetchPurchaseOrders, fetchPurchaseOrder, createPurchaseOrder, updatePurchaseOrder, submitPurchaseOrder, cancelPurchaseOrder, receiveGoods, fetchReorderSuggestions, generateFromSuggestions |
| `stockCountThunk.js` | fetchStockCounts, fetchStockCount, startCount, submitCountItems, completeCount, approveCount, cancelCount |
| `wasteThunk.js` | fetchWasteRecords, createWasteRecord, fetchWasteSummary |
| `prepListThunk.js` | generatePrepList, fetchPrepLists, fetchPrepList, updatePrepItemCompletion, completePrepList |
| `foodSafetyThunk.js` | fetchAllergenTags, fetchStockItemAllergens, setStockItemAllergens, fetchProductAllergens, fetchStockItemNutrition, setStockItemNutrition, fetchProductNutrition |
| `haccpThunk.js` | fetchTemplates, createTemplate, updateTemplate, deleteTemplate, fetchLogs, submitLog, fetchLogDetail, fetchDueToday |
| `reportThunk.js` | fetchStockLevelReport, fetchMovementHistory, fetchConsumptionReport, fetchValuationReport, fetchWasteAnalysis, fetchCOGSReport, fetchVarianceReport, **fetchActualVsTheoreticalReport**, **fetchMenuEngineeringReport**, fetchSupplierPerformanceReport, takeSnapshot, printStockReport |
| `alertThunk.js` | fetchAlerts, markAlertAsRead, markAllAlertsAsRead, dismissAlert |
| `integrationThunk.js` | fetchConnections, createConnection, updateConnection, deleteConnection, regenerateApiKey, fetchProductMappings, setProductMappings, fetchWebhookSubscriptions, createWebhookSubscription, deleteWebhookSubscription |
| `traceabilityThunk.js` | traceForward, traceBackward, generateRecallReport |

---

## 8. Page & Component Architecture

### 8.1 Layout Components

| Component | Description |
|-----------|-------------|
| `Layout/Layout.jsx` | Main layout: Header + Sidebar + Content area (Outlet). Responsive — sidebar collapses to bottom nav on mobile. Alert badge in header. |
| `Layout/Header.jsx` | Logo, breadcrumbs, search bar, alert bell (with unread count badge), language selector, account dropdown. |
| `Layout/Sidebar.jsx` | Navigation links: Dashboard, Stock Items, Recipes, Suppliers, Purchase Orders, Stock Counts, Waste, Reports, Alerts, Settings. Active state highlighting. Collapsible on desktop, drawer on mobile. |
| `Layout/MobileBottomNav.jsx` | Bottom navigation bar for mobile: Dashboard, Stock, Orders, Alerts, More. |

### 8.2 Dashboard Page

The dashboard is the landing page showing a summary of the inventory status at a glance. **Fully customizable** with drag-and-drop widget placement (using `react-grid-layout`) — each user can arrange and resize widgets to prioritize the most important information for their role. Widget layout is persisted in Redux Persist.

| Widget | Description | Data Source |
|--------|-------------|-------------|
| **Stock Health Summary** | Cards showing: Total Items, Below PAR count (yellow), Low Stock count (orange), Out of Stock count (red), Overstock count (blue) | `GET /api/inventory/stock-items/{orgId}` |
| **Food Cost Gauge** | Large gauge/donut chart showing current actual food cost % vs. target %. Green if on target, red if over threshold. The most important number for restaurant profitability. | `GET /api/inventory/reports/{orgId}/actual-vs-theoretical` |
| **Actual vs Theoretical Variance** | Line chart showing daily/weekly food cost variance trend. Highlights when variance exceeds threshold (3%). Quick link to full report. | `GET /api/inventory/reports/{orgId}/actual-vs-theoretical` |
| **Menu Engineering Snapshot** | Mini 4-quadrant scatter plot showing Stars/Plowhorses/Puzzles/Dogs distribution. Shows count per category. Quick link to full report. | `GET /api/inventory/reports/{orgId}/menu-engineering` |
| **Recent Alerts** | Last 5 alerts with type icons and timestamps (including HACCP overdue, supplier cert expiring) | `GET /api/inventory/alerts/{orgId}?page=1&pageSize=5` |
| **Stock Value Card** | Total inventory value with trend arrow (vs. last period snapshot) | `GET /api/inventory/reports/{orgId}/valuation` |
| **Pending Purchase Orders** | Count of POs by status (Draft, Submitted, Confirmed) with total value | `GET /api/inventory/purchase-orders/{orgId}?status=submitted` |
| **PAR Level Status** | Items below PAR that need reordering. Shows item count and estimated order value. "Auto-Generate POs" button. | `GET /api/inventory/purchase-orders/{orgId}/suggestions` |
| **Top Consumed Items Chart** | Bar chart of top 10 consumed items this period | `GET /api/inventory/reports/{orgId}/consumption` |
| **Waste Overview Chart (WEPT)** | Pie chart of waste by WEPT category (Waste, Errors, Portioning, Theft). Cost totals. | `GET /api/inventory/reports/{orgId}/waste-analysis` |
| **HACCP Compliance** | Today's checklists: completed vs. pending vs. overdue. Red alert if any overdue. Quick link to complete. | `GET /api/inventory/haccp/{orgId}/due-today` |
| **Today's Prep List** | Summary of today's prep list items. Completed vs. remaining. Quick link to detail. | `GET /api/inventory/prep-lists/{orgId}?date=today` |
| **Recent Movements** | Last 10 stock movements with icons per type | `GET /api/inventory/reports/{orgId}/movements?pageSize=10` |
| **Quick Actions** | Buttons: Record Waste, Adjust Stock, New PO, Start Count, Complete HACCP, Generate Prep List | Navigation shortcuts |

### 8.3 Stock Items Pages

| Page/Component | Description |
|----------------|-------------|
| `StockItemList.jsx` | Ant Design Table with: search bar, category filter dropdown, storage location filter, status filter (Active/Inactive/Low Stock/Below PAR/Out of Stock), sort by name/quantity/value/PAR status. Columns: Name, SKU, Category, Storage Location, Quantity (with color-coded PAR bar), Unit, PAR Level, Min Threshold, Cost Price, Value, Allergen icons, Status badge. Actions: Edit, Adjust, View History. Bulk actions: Export, Delete. |
| `StockItemForm.jsx` | Formik form with tabs: **Basic** (Name, SKU, Description, Category, Base Unit of Measurement, Barcode with scan button), **Stock Levels** (Initial Quantity, PAR Level, day-of-week PAR overrides, Min Threshold, Max Capacity), **Pricing** (Cost Price, Primary Supplier dropdown), **Location** (Primary Storage Location dropdown, additional locations), **Alternate Count Units** (define case sizes, pack sizes — e.g., "1 Case = 24 Bottles"), **Allergens** (tag with 14 EU allergens, severity: Contains/May Contain/Free From), **Nutrition** (calories, protein, carbs, fat, fiber per 100g), **Other** (Is Perishable toggle, Default Expiration Days, Notes). Validation via Yup. |
| `StockItemDetail.jsx` | Full stock item view with tabs: Overview (details + PAR level gauge showing current vs PAR vs min), Movement History (timeline), Recipes (which products use this item, food cost impact), Suppliers (which suppliers provide it with price comparison), Allergens (allergen tags), Nutrition (nutritional info), Location (storage locations with quantities). Edit and Adjust buttons. |
| `StockAdjustmentModal.jsx` | Modal for quick stock adjustment: Adjustment type (Increase/Decrease), Quantity (supports alternate count units — e.g., "2 Cases" auto-converts to "48 Bottles"), WEPT Reason (dropdown: Waste/Spoilage, Error/Incorrect Count, Portioning/Over-serving, Theft, Received, Transfer, Other), Notes. Shows current quantity and result preview. |
| `StockMovementHistory.jsx` | Filtered timeline of stock movements for one item. Each entry shows: timestamp, type icon, quantity change (green +/red -), reference link, user who made the change, WEPT category. Date range filter. |
| `CsvImportModal.jsx` | Modal for CSV import: File upload dropzone, download template button, column mapping preview, validation results table (errors highlighted), confirm button. Shows progress bar during import. Supports import of stock items, recipes, and supplier data. |

### 8.4 Recipe Pages

| Page/Component | Description |
|----------------|-------------|
| `RecipeList.jsx` | Table of menu Products with recipe status. Columns: Product Name, Category, Selling Price, # Ingredients, Recipe Cost, **Food Cost %** (color-coded: green < target, yellow near target, red > target), **Contribution Margin**, **Menu Engineering Category** (Star/Plowhorse/Puzzle/Dog badge). Filter: Has Recipe / No Recipe / by category / by engineering category. Sort by food cost %, margin, popularity. Click to edit recipe. |
| `RecipeDetail.jsx` | View a product's recipe: Product info card at top (price, category, **menu engineering badge**), ingredient list table below. Columns: Stock Item, Quantity Required, Unit, Waste %, Cost per Unit, Sub-Total. **Total recipe cost** at bottom. **Food cost percentage** with target comparison. **Allergen summary** — auto-calculated from all ingredients (shows which of 14 EU allergens are present). **Nutritional summary** — auto-calculated from ingredient composition. |
| `RecipeForm.jsx` | Edit recipe ingredients: Add ingredient (search stock items with barcode scan), set quantity, unit (with unit conversion helper), waste percentage. Drag-and-drop reorder. Remove ingredient. **Live cost calculation** as ingredients change — food cost % updates in real-time. Allergen preview updates as ingredients are added/removed. |

### 8.5 Supplier Pages

| Page/Component | Description |
|----------------|-------------|
| `SupplierList.jsx` | Table: Name, Contact, Phone, Email, # Items Supplied, Last Order Date, **Performance Score** (color-coded), **Certification Status** (valid/expiring/expired badges). Actions: Edit, View, Delete. Search bar. |
| `SupplierForm.jsx` | Formik form with tabs: **Basic** (Name, Contact Person, Email, Phone, Address fields, Payment Terms, Lead Time Days, Min Order Amount, Notes), **Certifications** (add/manage certifications: type, number, issue date, expiry date, upload document). |
| `SupplierDetail.jsx` | Supplier info card + tabs: Stock Items (linked items with prices, price history chart), Purchase History (POs to this supplier), **Performance Dashboard** (avg delivery time trend, on-time delivery %, order accuracy %, price stability chart, rejection rate), **Certifications** (list with expiry countdown, upload/view documents). |
| `SupplierStockItemModal.jsx` | Modal to link stock items to supplier: Search stock items, set supplier SKU, unit price, min order quantity, preferred flag. |
| `SupplierCertificationForm.jsx` | Form to add/edit supplier certifications: Type dropdown (HACCP, ISO 22000, Organic, Halal, Kosher, custom), number, dates, document upload. |

### 8.6 Purchase Order Pages

| Page/Component | Description |
|----------------|-------------|
| `PurchaseOrderList.jsx` | Table with status badges (Draft/Submitted/Confirmed/Received/Cancelled). Filter by status, supplier, date range. Columns: PO #, Supplier, Date, Expected Delivery, # Items, Total Amount, Status. |
| `PurchaseOrderForm.jsx` | Multi-step form: 1) Select Supplier, 2) Add Items (search stock items, set quantity & price), 3) Review & Notes, 4) Submit or Save Draft. Line items table with running total. |
| `PurchaseOrderDetail.jsx` | PO header info + line items table + status timeline. Actions based on status: Edit (Draft), Submit (Draft), Receive (Submitted/Confirmed/Partially Received), Cancel (not Received). |
| `GoodsReceivingForm.jsx` | Receive goods against PO: Table of ordered items with columns: Item, Ordered Qty, Previously Received, Receiving Now (input with alternate count unit support), Accepted, Rejected, Actual Price (highlights variance from ordered price), Batch #, Expiry Date, **Temperature** (for cold chain items — validates against safe ranges). **Photo capture button** — photograph delivery for quality documentation (damaged goods, incorrect items). Uses `react-webcam`. Running total with price variance summary. Submit button. |
| `ReorderSuggestions.jsx` | Cards/table showing items that need reordering: Item, Current Stock, Min Threshold, Daily Consumption, Days Until Stockout, Suggested Order Qty, Preferred Supplier. Checkbox select + "Generate PO" button. |

### 8.7 Stock Count Pages

| Page/Component | Description |
|----------------|-------------|
| `StockCountList.jsx` | Table of count sessions: Date, Status (In Progress/Completed/Approved), # Items, Total Variance Value, Variance %, Counted By, Actions. Color-coded rows by variance severity. |
| `StockCountForm.jsx` | Start new count: Select **storage locations** to count (for shelf-to-sheet — organized by physical area), or select categories/items. Shows expected item list organized by location. Assign sections to different staff members for **multi-user simultaneous counting**. |
| `StockCountEntry.jsx` | **The core counting interface** — optimized for mobile/tablet use in storage room. **Organized by storage location** (shelf-to-sheet) — staff walks through each location in order. Items appear in shelf order within each location. For each item: Name, SKU, Unit, **Alternate Count Units** (count in cases/packs instead of individual units), Expected Qty (hidden/shown toggle), Actual Qty (large number input or barcode scan). Search/filter bar at top. **Multi-user progress indicators** — see which sections other staff are counting in real-time. Progress indicator (X of Y counted). **Offline support** — counts saved to IndexedDB via `idb`, auto-syncs when connection resumes. Can save partial progress. Supports barcode scanning to auto-find item. |
| `StockCountDetail.jsx` | Count result overview: Table with Expected vs. Actual vs. Variance columns, **WEPT Classification** (Waste/Errors/Portioning/Theft reason for each variance). Color-coded rows (green = match, yellow = minor variance, red = significant variance exceeding threshold). Total variance value and percentage. Variance breakdown by storage location (which area had most shrinkage). Approve/reject buttons for manager. |

### 8.8 Waste Pages

| Page/Component | Description |
|----------------|-------------|
| `WasteRecordList.jsx` | Table with date range filter and WEPT category filter. Columns: Date, Stock Item, Quantity, Unit, **WEPT Category** (color-coded badge), Specific Reason, Cost, Recorded By, Notes. Summary cards at top: Total Waste Cost (period), **WEPT breakdown** (4 mini-cards), Top Waste Reason, Most Wasted Item. Industry target indicator (1.5-2.5% of total consumption). |
| `WasteRecordForm.jsx` | Quick-entry form: Search/select stock item (or barcode scan), Quantity (with alternate count unit support), **WEPT Category** (primary selector: Waste, Error, Portioning, Theft), Specific Reason (context-dependent dropdown — e.g., if WEPT = "Waste" then: Spoilage, Overproduction, Expired, Damaged), Notes. Shows cost preview. Optimized for fast mobile entry. |

### 8.9 Storage Location Pages

| Page/Component | Description |
|----------------|-------------|
| `StorageLocationList.jsx` | Cards/grid view of storage locations: Name, Type icon (cooler/freezer/dry/bar), Item Count, Total Value, Temperature Range (if set). Drag to reorder (sets count sheet order for shelf-to-sheet). Actions: Edit, View Stock, Delete. |
| `StorageLocationForm.jsx` | Formik form: Name, Description, Location Type (dropdown: Walk-in Cooler, Freezer, Dry Storage, Bar, Wine Cellar, Prep Station, Display Case, Other), Display Order, Temperature Min/Max (for HACCP compliance). |
| `StorageLocationDetail.jsx` | Location info card + table of stock items at this location with quantities and shelf positions. Transfer button to move stock to another location. |
| `StockTransferModal.jsx` | Modal to transfer stock between locations: Select item, from location, to location, quantity. Creates movement audit trail. |

### 8.10 Prep List Pages

| Page/Component | Description |
|----------------|-------------|
| `PrepListOverview.jsx` | Calendar view showing prep lists by date. Status indicators: Generated (blue), In Progress (orange), Completed (green). Today's list highlighted. |
| `PrepListGenerator.jsx` | Generate a new prep list: Select date, system shows forecasted sales based on historical patterns. Preview ingredient requirements calculated from recipes. Review and adjust quantities. Assign tasks to staff members. Generate button. |
| `PrepListDetail.jsx` | Prep list for a specific date. Table: Stock Item, Required Qty, On-Hand Qty, Prep Qty (to prepare), Assigned To (staff dropdown), Actual Prepped (input), Completed checkbox. Progress bar (X of Y tasks completed). Optimized for tablet use in kitchen. Print button for paper checklist. |

### 8.11 Food Safety Pages

| Page/Component | Description |
|----------------|-------------|
| `FoodSafetyOverview.jsx` | Dashboard with 3 sections: **Allergen Compliance** (items with/without allergen data), **HACCP Status** (today's checklists: done/pending/overdue), **Traceability** (quick search). |
| `AllergenDashboard.jsx` | Matrix view: rows = menu products, columns = 14 EU allergens. Cells show Contains (red), May Contain (yellow), Free From (green), or Unknown (gray). Filterable by allergen. Export to PDF for printing/displaying in restaurant. |
| `HaccpChecklistList.jsx` | Table of HACCP checklist templates and recent logs. Templates: Name, Frequency, # Items, Status. Logs: Date, Template, Completed By, Status (Passed/Failed), critical failures highlighted. |
| `HaccpTemplateManager.jsx` | Create/edit HACCP templates: Name, Frequency (Daily/Weekly/Monthly/On Receiving), add checklist items (description, expected value, is critical toggle). Reorder items via drag-and-drop. |
| `HaccpChecklistEntry.jsx` | **Mobile-optimized completion form** for staff. Shows each checklist item with: description, expected value, actual value input, pass/fail toggle, notes, photo capture (for evidence). Critical items highlighted with red border. Offline support via IndexedDB. Submit when complete. |
| `HaccpChecklistLogDetail.jsx` | View completed checklist: all items with results, photos, timestamps. PDF export for audit records. |
| `TraceabilitySearch.jsx` | Search interface: enter ingredient/batch number to **trace forward** (which dishes used it, when served) or enter menu item to **trace backward** (which ingredients, which suppliers, which batches). Useful for recall situations. Results shown as a visual flow/tree diagram. |

### 8.12 Report Pages

| Page/Component | Description |
|----------------|-------------|
| `ReportsOverview.jsx` | Grid of report cards organized in 3 tiers: **Critical** (Actual vs Theoretical, Menu Engineering, COGS), **Operational** (Stock Levels, Variance, Consumption, Waste), **Administrative** (Valuation, Movements, Supplier Performance). Each card has icon, description, last-run date, and "View Report" button. |
| `ActualVsTheoreticalReport.jsx` | **The industry's #1 cost control report** (used by MarketMan, Apicbase, Restaurant365). Side-by-side comparison: Theoretical Cost (from recipes × POS sales) vs. Actual Cost (from inventory counts + purchasing). **Variance %** prominently displayed with color coding (green < 2%, yellow 2-3%, red > 3%). Drill-down by category, then by individual item. **WEPT breakdown** chart showing where variance comes from (Waste, Errors, Portioning, Theft). Trend chart showing variance over time. Date range filter. Export to CSV/PDF. |
| `MenuEngineeringReport.jsx` | **Interactive 4-quadrant scatter plot** (using Recharts) plotting each menu item by Popularity (X-axis: units sold) vs. Profitability (Y-axis: contribution margin). Items color-coded by quadrant: **Stars** (green, top-right), **Plowhorses** (blue, bottom-right), **Puzzles** (orange, top-left), **Dogs** (red, bottom-left). Click an item to see details. Summary table below with recommendations per category. Date range filter. Food cost % column. Export. |
| `StockLevelReport.jsx` | Tabular + visual report. Table: Item, Category, Storage Location, Qty, PAR Level, Min, Max, Status, Value. Charts: Stock status distribution (pie), PAR compliance (% items at/above PAR), top 10 items by value (bar). Filters: Category, storage location, status. Export to CSV/PDF. |
| `MovementHistoryReport.jsx` | Timeline + table view. Filter by date range, item, movement type, WEPT category. Chart: movements over time (line). Export. |
| `ConsumptionReport.jsx` | Period-based analysis. Bar charts showing consumption by item/category. Trend lines. Group by day/week/month. Top consumers table. **Demand forecasting preview** — projected consumption for next period based on historical patterns. Export. |
| `ValuationReport.jsx` | Total inventory value breakdown. By category (pie chart), by storage location (pie chart), by item (table with cost method applied). Historical valuation trend (line chart from inventory snapshots). As-of-date picker. Period-over-period comparison. Export. |
| `WasteAnalysisReport.jsx` | Waste breakdown by **WEPT category** (pie — Waste, Errors, Portioning, Theft), by item (bar), trend over time (line). Cost analysis. **Industry benchmark comparison** (actual % vs. target 1.5-2.5%). Actionable recommendations section. Export. |
| `COGSReport.jsx` | Cost of Goods Sold analysis. By product: theoretical cost vs actual cost. Overall COGS percentage. **Food cost vs beverage cost** breakdown. Category-level analysis. Period-over-period trend. Target vs. actual comparison. Date range filter. Export. |
| `VarianceReport.jsx` | From stock count data. Expected vs actual comparison table. **WEPT classification** per item. Shrinkage calculation. Variance by category (bar chart), **by storage location** (which area has most shrinkage). Configurable threshold highlighting. Date filter or stock count session selector. Export. |
| `SupplierPerformanceReport.jsx` | Supplier scorecard: on-time delivery %, order accuracy %, price stability (trend chart), rejection rate, certification compliance status. Ranking table across all suppliers. Individual supplier drill-down. Date range filter. Export. |

### 8.13 Alert Pages

| Page/Component | Description |
|----------------|-------------|
| `AlertList.jsx` | List of alerts with type icons (Low Stock=orange, Out of Stock=red, Below PAR=yellow, Expiring=amber, Expired=dark red, HACCP Overdue=purple, Supplier Cert Expiring=teal). Mark as read, dismiss. Filter by type, read status. Bulk actions. |
| `AlertBadge.jsx` | (Used in Header) Bell icon with unread count badge. Click opens alerts dropdown. |
| `AlertDropdown.jsx` | Quick-view dropdown from header: Last 5 alerts with "View All" link. Mark all as read button. |

### 8.14 Settings Pages

| Page/Component | Description |
|----------------|-------------|
| `InventorySettings.jsx` | Settings layout with tabs: General, Food Safety, Integrations. |
| `GeneralSettings.jsx` | Formik form: Cost Valuation Method (dropdown: FIFO/LIFO/Weighted Average), **Food Cost Target %** (number input, e.g., 30%), **Beverage Cost Target %** (e.g., 20%), **Variance Alert Threshold %** (e.g., 3%), Auto-Deduct on Order (toggle), Low Stock Alert Email (toggle), Expiration Alert Days Before (number input), **Inventory Count Frequency** (dropdown: Daily/Weekly/Bi-weekly/Monthly), Default Unit of Measurement (dropdown), Default Currency. |
| `FoodSafetySettings.jsx` | Toggles and configuration for: Enable HACCP Module, Enable Allergen Tracking, Enable Nutritional Data, Enable Prep Lists. HACCP reminder frequency. |
| `OrganizationSettings.jsx` | Restaurant profile: Name, Address, City, PostalCode, Country, Phone, Email, Logo upload, Currency, Timezone. |
| `UserManagement.jsx` | List users in this organization with roles. Invite new users via email. Change roles (Owner/Manager/Staff). Remove users. |
| `IntegrationSettings.jsx` | POS integration management (any POS system). |
| `PosConnectionList.jsx` | Table of connections: POS Name, Status (Active/Inactive), API Key (masked), Last Sync, # Mappings, **# Webhook Subscriptions**. Actions: Edit, Regenerate Key, View Mappings, **Manage Webhooks**, Deactivate. |
| `PosConnectionForm.jsx` | Create/edit connection: POS System Name (dropdown: BonApp, Lightspeed, Toast, Square, Orderbird, Custom), Notes. On create: shows generated API key (copy button, one-time display warning). |
| `ProductMappingManager.jsx` | Table of external-to-internal product mappings: External Product ID, External Product Name, Mapped Stock Item (dropdown), Quantity Multiplier, Active toggle. Add/remove mappings. Bulk import via CSV. |
| `WebhookSubscriptionManager.jsx` | Manage outbound webhook subscriptions for this connection: Event Type (dropdown: Stock Low, Stock Out, Goods Received, PO Created, Count Completed, Waste Recorded), Target URL, Active toggle. View delivery logs with success/failure status. Test webhook button. |

---

## 9. Shared/Reusable Components (New)

### Feature Components (in `src/components/`)

| Component | Description |
|-----------|-------------|
| `StockLevelBar.jsx` | Horizontal bar showing current stock relative to **PAR level**, min threshold, and max capacity. Green (above PAR), Yellow (below PAR), Orange (below minimum), Red (out of stock). Shows PAR marker line. Used in tables and detail views. |
| `StockStatusBadge.jsx` | Colored badge: "In Stock" (green), "Below PAR" (yellow), "Low Stock" (orange), "Out of Stock" (red), "Overstock" (blue), "Inactive" (gray). |
| `MenuEngineeringBadge.jsx` | Colored badge for menu engineering categories: Star (gold star icon), Plowhorse (blue), Puzzle (orange), Dog (gray). |
| `FoodCostIndicator.jsx` | Small indicator showing food cost % with color coding: green (below target), yellow (near target), red (above target). Used in recipe lists and detail views. |
| `AllergenIconBar.jsx` | Row of allergen icons for a stock item or menu product. Shows the applicable allergens from the 14 EU allergens with tooltips. Clickable to see details. |
| `WEPTCategoryBadge.jsx` | Colored badge for WEPT classification: Waste (brown), Error (orange), Portioning (yellow), Theft (red). |
| `MovementTypeIcon.jsx` | Icon component for each movement type: Received (arrow-down green), Sold (arrow-up blue), Adjusted (pencil yellow), Wasted (trash red), Transferred (arrows purple), etc. |
| `PurchaseOrderStatusBadge.jsx` | Colored badge for PO statuses: Draft (gray), Submitted (blue), Confirmed (teal), Partially Received (orange), Fully Received (green), Cancelled (red). |
| `QuantityInput.jsx` | Specialized number input with +/- buttons, **alternate count unit selector** (e.g., switch between "Bottles" and "Cases" — auto-converts). Large touch targets for mobile use. |
| `BarcodeScannerButton.jsx` | Button that opens camera for barcode scanning. Returns scanned value to parent component. Supports both item barcodes and case barcodes (resolves to alternate count units). Uses quagga2 or html5-qrcode. |
| `PhotoCaptureButton.jsx` | Button that opens camera via `react-webcam` to capture photos. Used in goods receiving (delivery documentation) and HACCP checklists (evidence photos). Saves as base64 or uploads to blob storage. |
| `DateRangeFilter.jsx` | Reusable date range picker with presets (Today, This Week, This Month, This Quarter, Custom). Uses Ant Design DatePicker. |
| `CurrencyDisplay.jsx` | Formats decimal values as currency (CHF by default, configurable per restaurant) with proper decimal places and thousands separator. |
| `ExportButton.jsx` | Dropdown button with export options: CSV, Excel, PDF. Triggers appropriate export function. |
| `EmptyState.jsx` | Illustrated empty state for lists/pages with no data. Custom message and CTA button. |
| `StatCard.jsx` | Dashboard stat card with: icon, label, value, trend indicator (up/down arrow with percentage). Supports drag handle for dashboard customization. |
| `DashboardWidgetWrapper.jsx` | Wrapper component for dashboard widgets. Provides consistent card styling, resize handle, drag handle, and collapse/expand toggle. Used with `react-grid-layout`. |
| `SearchFilterBar.jsx` | Reusable search + filter bar: text search input, filter dropdowns (configurable), clear all button. |
| `BulkActionBar.jsx` | Appears when items are selected in a table. Shows: selected count, action buttons (Export, Delete, etc.), clear selection. |
| `PrintButton.jsx` | Button to send reports to a connected printer. Printer selection dropdown if multiple printers. |
| `ImportWizard.jsx` | Multi-step import wizard: 1) Download template, 2) Upload file, 3) Map columns, 4) Validate, 5) Preview, 6) Confirm. Reusable for stock items, recipes, mappings, etc. |
| `VarianceThresholdBar.jsx` | Visual bar showing actual vs. theoretical variance with threshold line. Used in Actual vs Theoretical and Variance reports. Color transitions from green to red as variance increases. |
| `UnitConversionHelper.jsx` | Inline helper that appears when entering quantities — shows conversion between units (e.g., "2 Cases = 48 Bottles"). Helps staff avoid calculation errors. |

---

## 10. Hooks (Custom)

| Hook | Description |
|------|-------------|
| `useAuthInitialization` | Shared pattern: load auth state on mount, redirect to login if not authenticated. |
| `useClearSessionOnUnload` | Shared pattern: clear session data on window unload. |
| `useStockItemSearch` | Debounced search across stock items by name, SKU, or barcode. Returns filtered results. |
| `useBarcodeScan` | Hook to manage barcode scanner state: open/close camera, handle scan result, resolve case barcodes to alternate count units, error handling. |
| `usePagination` | Generic pagination hook: page, pageSize, total, goToPage, nextPage, prevPage. |
| `useFilters` | Generic filter management: set/clear individual filters, clear all, serialize to query params. |
| `useDateRange` | Date range selection with presets: today, thisWeek, thisMonth, thisQuarter, custom. Returns startDate, endDate. |
| `useExportData` | Export data to CSV/Excel/PDF. Takes data array and column definitions. Handles file download. |
| `usePolling` | Reusable polling hook for alerts and real-time stock updates. |
| `useWindowSize` | Shared pattern: responsive breakpoints. |
| `useNavigationHandler` | Shared pattern: navigation with confirmation for unsaved changes. |
| `useStockAlert` | Fetch unread alert count (including HACCP overdue, cert expiring), poll periodically, expose unreadCount for badge. |
| `useOfflineSync` | Manage offline data storage via `idb` (IndexedDB). Save stock count entries and HACCP checklist completions offline, auto-sync when connection resumes. Provides `isOffline`, `pendingSync`, `syncNow` states. |
| `useUnitConversion` | Convert quantities between units using the restaurant's conversion table. Provides `convert(qty, fromUnit, toUnit)`, `getAlternateUnits(stockItemId)`, `convertToBaseUnit(qty, alternateUnitId)`. |
| `useDashboardLayout` | Manage customizable dashboard widget layout via `react-grid-layout`. Load/save layout from Redux, handle drag/resize events, reset to default layout. |
| `usePhotoCapture` | Manage camera state for photo capture via `react-webcam`. Open/close camera, capture photo, return as base64 or file. Used in goods receiving and HACCP checklists. |
| `useFoodCostCalculator` | Real-time food cost calculation as recipe ingredients change. Takes ingredients array, returns total cost, food cost %, contribution margin, and comparison to target. |

---

## 11. i18n Configuration

### Setup

```typescript
// src/i18n.ts
i18next
  .use(Backend)
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    fallbackLng: "en",
    interpolation: { escapeValue: false },
    backend: {
      loadPath: "/locales/{{lng}}/translation.json",
    },
  });
```

### Translation Keys Structure

```json
{
  "header": { "title", "search", "alerts", "account", "language" },
  "sidebar": { "dashboard", "stockItems", "recipes", "suppliers", "purchaseOrders", "stockCounts", "waste", "reports", "alerts", "settings" },
  "dashboard": { "title", "stockHealth", "totalItems", "lowStock", "outOfStock", "overstock", "recentAlerts", "stockValue", "pendingOrders", "topConsumed", "wasteOverview", "recentMovements", "quickActions" },
  "stockItems": { "title", "add", "edit", "delete", "import", "export", "name", "sku", "category", "quantity", "unit", "minThreshold", "maxCapacity", "costPrice", "supplier", "barcode", "storage", "perishable", "expirationDays", "notes", "status", "inStock", "lowStock", "outOfStock", "overstock", "inactive", "adjust", "movements", "search", "filters" },
  "categories": { "title", "add", "edit", "delete", "name", "parent" },
  "recipes": { "title", "product", "ingredients", "add", "remove", "quantity", "unit", "wastePercent", "recipeCost", "foodCostPercent", "margin", "noRecipe", "deductionPreview" },
  "suppliers": { "title", "add", "edit", "delete", "name", "contact", "email", "phone", "address", "paymentTerms", "leadTime", "minOrder", "stockItems", "performance" },
  "purchaseOrders": { "title", "create", "edit", "submit", "cancel", "receive", "orderNumber", "supplier", "status", "orderDate", "expectedDelivery", "items", "subtotal", "tax", "shipping", "total", "draft", "submitted", "confirmed", "partiallyReceived", "fullyReceived", "cancelled", "reorderSuggestions", "generatePO" },
  "goodsReceiving": { "title", "ordered", "previouslyReceived", "receivingNow", "accepted", "rejected", "actualPrice", "batchNumber", "expiryDate", "invoiceNumber" },
  "stockCounts": { "title", "start", "count", "complete", "approve", "cancel", "expected", "actual", "variance", "varianceValue", "inProgress", "completed", "approved", "progress", "scanBarcode" },
  "waste": { "title", "record", "quantity", "reason", "spoilage", "overproduction", "customerReturn", "preparation", "expired", "damaged", "other", "totalCost", "summary" },
  "reports": { "title", "stockLevels", "movements", "consumption", "valuation", "wasteAnalysis", "cogs", "variance", "dateRange", "export", "print", "groupBy", "daily", "weekly", "monthly" },
  "storageLocations": { "title", "add", "edit", "delete", "name", "type", "walkInCooler", "freezer", "dryStorage", "bar", "wineCellar", "prepStation", "displayCase", "tempRange", "transfer", "itemCount" },
  "menuEngineering": { "title", "star", "plowhorse", "puzzle", "dog", "contributionMargin", "popularity", "foodCostPercent", "recommendations", "promote", "optimize", "reposition", "remove" },
  "actualVsTheoretical": { "title", "theoreticalCost", "actualCost", "variance", "variancePercent", "target", "weptBreakdown", "drillDown" },
  "prepLists": { "title", "generate", "date", "forecast", "required", "onHand", "toPrepare", "assignedTo", "completed", "inProgress" },
  "foodSafety": { "title", "allergens", "nutrition", "haccp", "traceability", "contains", "mayContain", "freeFrom" },
  "haccp": { "title", "templates", "checklists", "dueToday", "overdue", "passed", "failed", "criticalItem", "complete", "frequency", "daily", "weekly", "monthly", "onReceiving" },
  "allergens": { "gluten", "crustaceans", "eggs", "fish", "peanuts", "soybeans", "milk", "nuts", "celery", "mustard", "sesame", "sulphites", "lupin", "molluscs" },
  "alerts": { "title", "lowStock", "outOfStock", "belowPar", "expiring", "expired", "overstock", "haccpOverdue", "certExpiring", "markRead", "markAllRead", "dismiss", "unread" },
  "settings": { "title", "general", "integrations", "costMethod", "fifo", "lifo", "weightedAverage", "autoDeduct", "alertEmail", "expirationDays" },
  "integrations": { "title", "connections", "add", "edit", "delete", "posSystem", "apiKey", "regenerateKey", "webhookSecret", "mappings", "lastSync", "active", "copyKey", "keyWarning" },
  "common": { "save", "cancel", "delete", "edit", "create", "search", "filter", "clearFilters", "export", "import", "print", "confirm", "back", "next", "loading", "noData", "error", "success", "actions", "status", "date", "notes", "total", "page", "of", "showing" },
  "validation": { "required", "minLength", "maxLength", "invalidEmail", "invalidPhone", "positiveNumber", "nonNegative", "uniqueSku", "selectItem", "selectSupplier", "quantityExceeded" },
  "errors": { "generic", "network", "unauthorized", "notFound", "serverError", "importFailed", "exportFailed" },
  "units": { "piece", "kilogram", "gram", "liter", "milliliter", "bottle", "box", "pack", "dozen", "portion" }
}
```

Translation files: `public/locales/en/translation.json`, `public/locales/fr/translation.json`, `public/locales/de/translation.json`

---

## 12. Entry Point & Wrappers

### main.tsx

```tsx
// Standalone entry point — NO @repo/ui, NO BonApp dependencies
import { Provider } from "react-redux";
import { PersistGate } from "redux-persist/integration/react";
import { I18nextProvider } from "react-i18next";
import { ConfigProvider } from "antd";
import i18n from "./i18n";
import { store, persistor } from "./redux/store";
import { antdTheme } from "./config/antdTheme";
import App from "./App";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <Provider store={store}>
    <PersistGate loading={null} persistor={persistor}>
      <I18nextProvider i18n={i18n}>
        <ConfigProvider theme={antdTheme}>
          <App />
        </ConfigProvider>
      </I18nextProvider>
    </PersistGate>
  </Provider>
);
```

### Vite Config

```typescript
// vite.config.ts — standalone config (NOT shared @repo/configs)
import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import svgr from "vite-plugin-svgr";

export default defineConfig({
  plugins: [react(), svgr()],
  server: { port: 3000 },
  build: { sourcemap: true },
  css: { preprocessorOptions: { scss: { api: "modern" } } },
});
```

---

## 13. Loaders (React Router Data Loaders)

| Loader | Route | Data Fetched |
|--------|-------|-------------|
| `dashboardLoader` | `/dashboard` | Low stock count, alert count, recent movements, stock value summary |
| `stockItemsLoader` | `/stock-items` | Paginated stock items, categories |
| `stockItemDetailLoader` | `/stock-items/:id` | Single stock item with movements |
| `categoriesLoader` | `/categories` | All categories |
| `recipesLoader` | `/recipes` | Products list with recipe status |
| `recipeDetailLoader` | `/recipes/:productId` | Recipe detail |
| `suppliersLoader` | `/suppliers` | Supplier list |
| `supplierDetailLoader` | `/suppliers/:id` | Supplier detail with stock items |
| `purchaseOrdersLoader` | `/purchase-orders` | PO list (paginated) |
| `purchaseOrderDetailLoader` | `/purchase-orders/:id` | PO detail with line items |
| `stockCountsLoader` | `/stock-counts` | Count session list |
| `stockCountDetailLoader` | `/stock-counts/:id` | Count session detail with items |
| `wasteLoader` | `/waste` | Waste records (paginated) |
| `reportsLoader` | `/reports/*` | Report-specific data |
| `alertsLoader` | `/alerts` | Alert list |
| `settingsLoader` | `/settings` | Current inventory settings |
| `connectionsLoader` | `/settings/integrations/connections` | External POS connections |

---

## 14. SCSS / Styling

### Approach
- Own design tokens (colors, fonts, spacing) — NOT from `@repo/ui`
- Tailwind CSS for utility classes (own config)
- SCSS modules for component-specific styles
- Ant Design component customization via `ConfigProvider` theme tokens (own `antdTheme.ts`)

### New SCSS Variables (inventory-specific)

```scss
// src/scss/abstract/_variables.scss

// Stock Level Colors
$stock-ok: #52c41a;          // Green — above PAR level
$stock-below-par: #fadb14;   // Yellow — below PAR but above minimum
$stock-low: #faad14;         // Orange — below minimum threshold
$stock-critical: #ff4d4f;    // Red — out of stock
$stock-over: #1890ff;        // Blue — overstock
$stock-inactive: #8c8c8c;    // Gray — inactive item

// Movement Type Colors
$movement-received: #52c41a;  // Green
$movement-sold: #1890ff;      // Blue
$movement-adjusted: #faad14;  // Orange
$movement-wasted: #ff4d4f;    // Red
$movement-returned: #722ed1;  // Purple
$movement-transferred: #13c2c2; // Teal

// WEPT Category Colors (Waste, Errors, Portioning, Theft)
$wept-waste: #8b4513;        // Brown — spoilage/expiration
$wept-error: #fa8c16;        // Orange — counting/receiving/recipe errors
$wept-portioning: #fadb14;   // Yellow — over-serving
$wept-theft: #cf1322;        // Dark Red — unauthorized removal

// Menu Engineering Colors
$menu-star: #faad14;         // Gold — Stars (high profit + high popularity)
$menu-plowhorse: #1890ff;    // Blue — Plowhorses (high popularity, low profit)
$menu-puzzle: #fa8c16;       // Orange — Puzzles (high profit, low popularity)
$menu-dog: #8c8c8c;          // Gray — Dogs (low profit + low popularity)

// Food Cost Indicator Colors
$food-cost-good: #52c41a;    // Green — below target
$food-cost-warning: #faad14; // Orange — near target
$food-cost-danger: #ff4d4f;  // Red — above target

// Actual vs Theoretical Variance Colors
$variance-ok: #52c41a;       // Green — < 2%
$variance-warning: #faad14;  // Orange — 2-3%
$variance-danger: #ff4d4f;   // Red — > 3%

// Purchase Order Status Colors
$po-draft: #8c8c8c;
$po-submitted: #1890ff;
$po-confirmed: #13c2c2;
$po-partial: #faad14;
$po-received: #52c41a;
$po-cancelled: #ff4d4f;

// HACCP & Food Safety Colors
$haccp-passed: #52c41a;      // Green
$haccp-pending: #1890ff;     // Blue
$haccp-overdue: #722ed1;     // Purple
$haccp-failed: #ff4d4f;      // Red
$haccp-critical: #cf1322;    // Dark Red — critical control point

// Allergen Colors
$allergen-contains: #ff4d4f;  // Red
$allergen-may-contain: #faad14; // Yellow
$allergen-free-from: #52c41a; // Green
```

---

## 15. File Structure

```
inventory-frontend/                    (SEPARATE GIT REPO — standalone project)
├── public/
│   ├── locales/
│   │   ├── en/translation.json
│   │   ├── fr/translation.json
│   │   └── de/translation.json
│   ├── site.webmanifest
│   └── favicon.ico
├── ssl/
│   ├── localhost-key.pem
│   └── localhost.pem
├── src/
│   ├── api/                              # Centralized API modules (15 files)
│   │   ├── stockItemApi.js
│   │   ├── stockCategoryApi.js
│   │   ├── storageLocationApi.js
│   │   ├── recipeApi.js
│   │   ├── supplierApi.js
│   │   ├── purchaseOrderApi.js
│   │   ├── stockCountApi.js
│   │   ├── wasteApi.js
│   │   ├── prepListApi.js
│   │   ├── foodSafetyApi.js
│   │   ├── haccpApi.js
│   │   ├── reportApi.js
│   │   ├── alertApi.js
│   │   ├── integrationApi.js
│   │   └── traceabilityApi.js
│   ├── components/                       # Shared/reusable components
│   │   ├── Layout/
│   │   │   ├── Layout.jsx
│   │   │   ├── Header.jsx
│   │   │   ├── Sidebar.jsx
│   │   │   └── MobileBottomNav.jsx
│   │   ├── StockLevelBar.jsx
│   │   ├── StockStatusBadge.jsx
│   │   ├── MenuEngineeringBadge.jsx
│   │   ├── FoodCostIndicator.jsx
│   │   ├── AllergenIconBar.jsx
│   │   ├── WEPTCategoryBadge.jsx
│   │   ├── MovementTypeIcon.jsx
│   │   ├── PurchaseOrderStatusBadge.jsx
│   │   ├── QuantityInput.jsx
│   │   ├── BarcodeScannerButton.jsx
│   │   ├── PhotoCaptureButton.jsx
│   │   ├── DateRangeFilter.jsx
│   │   ├── CurrencyDisplay.jsx
│   │   ├── ExportButton.jsx
│   │   ├── EmptyState.jsx
│   │   ├── StatCard.jsx
│   │   ├── DashboardWidgetWrapper.jsx
│   │   ├── SearchFilterBar.jsx
│   │   ├── BulkActionBar.jsx
│   │   ├── PrintButton.jsx
│   │   ├── ImportWizard.jsx
│   │   ├── VarianceThresholdBar.jsx
│   │   └── UnitConversionHelper.jsx
│   ├── config/
│   │   ├── axios/
│   │   │   └── axios-config.ts
│   │   └── antdTheme.ts               # Own Ant Design theme (NOT @repo/ui)
│   ├── constants/
│   │   ├── links.js                     # Sidebar/navigation links
│   │   ├── units.js                     # Unit of measurement options
│   │   ├── statuses.js                  # Status constants
│   │   └── charts.js                    # Chart.js config
│   ├── helpers/
│   │   ├── authStorage.js
│   │   ├── utils.js
│   │   ├── formatters.js               # Currency, date, number formatting
│   │   ├── csvParser.js                # CSV parsing helpers
│   │   ├── exportHelpers.js            # CSV/PDF export helpers
│   │   └── validateImage.js
│   ├── hooks/
│   │   ├── useAuthInitialization.js
│   │   ├── useClearSessionOnUnload.js
│   │   ├── useStockItemSearch.js
│   │   ├── useBarcodeScan.js
│   │   ├── usePagination.js
│   │   ├── useFilters.js
│   │   ├── useDateRange.js
│   │   ├── useExportData.js
│   │   ├── usePolling.js
│   │   ├── useWindowSize.js
│   │   ├── useNavigationHandler.js
│   │   ├── useStockAlert.js
│   │   ├── useOfflineSync.js
│   │   ├── useUnitConversion.js
│   │   ├── useDashboardLayout.js
│   │   ├── usePhotoCapture.js
│   │   └── useFoodCostCalculator.js
│   ├── pages/
│   │   ├── Auth/                            # Own auth pages (NOT BonApp Login app)
│   │   │   ├── LoginPage.tsx
│   │   │   ├── RegisterPage.tsx
│   │   │   ├── ForgotPasswordPage.tsx
│   │   │   ├── ResetPasswordPage.tsx
│   │   │   └── AcceptInvitePage.tsx
│   │   ├── Dashboard/
│   │   │   └── Dashboard.tsx               # Customizable drag-and-drop widget dashboard
│   │   ├── StockItems/
│   │   │   ├── StockItemList.jsx
│   │   │   ├── StockItemForm.jsx           # Multi-tab form (basic, levels, pricing, allergens, nutrition)
│   │   │   ├── StockItemDetail.jsx
│   │   │   ├── StockAdjustmentModal.jsx    # WEPT-classified adjustments
│   │   │   ├── StockMovementHistory.jsx
│   │   │   └── CsvImportModal.jsx
│   │   ├── Categories/
│   │   │   ├── StockCategoryList.jsx
│   │   │   └── StockCategoryForm.jsx
│   │   ├── StorageLocations/
│   │   │   ├── StorageLocationList.jsx
│   │   │   ├── StorageLocationForm.jsx
│   │   │   ├── StorageLocationDetail.jsx
│   │   │   └── StockTransferModal.jsx
│   │   ├── Recipes/
│   │   │   ├── RecipeList.jsx              # With food cost %, menu engineering badges
│   │   │   ├── RecipeDetail.jsx            # Auto-calculated allergens & nutrition
│   │   │   └── RecipeForm.jsx              # Live cost calculation
│   │   ├── Suppliers/
│   │   │   ├── SupplierList.jsx            # Performance scores, cert status
│   │   │   ├── SupplierForm.jsx
│   │   │   ├── SupplierDetail.jsx          # Performance dashboard, certifications
│   │   │   ├── SupplierStockItemModal.jsx
│   │   │   └── SupplierCertificationForm.jsx
│   │   ├── PurchaseOrders/
│   │   │   ├── PurchaseOrderList.jsx
│   │   │   ├── PurchaseOrderForm.jsx
│   │   │   ├── PurchaseOrderDetail.jsx
│   │   │   ├── GoodsReceivingForm.jsx      # Photo capture, temperature logging
│   │   │   └── ReorderSuggestions.jsx       # PAR-based suggestions
│   │   ├── StockCounts/
│   │   │   ├── StockCountList.jsx
│   │   │   ├── StockCountForm.jsx          # Select storage locations for shelf-to-sheet
│   │   │   ├── StockCountEntry.jsx         # Mobile-optimized, offline, multi-user, ALT count units
│   │   │   └── StockCountDetail.jsx        # WEPT variance analysis
│   │   ├── Waste/
│   │   │   ├── WasteRecordList.jsx         # WEPT categorization
│   │   │   └── WasteRecordForm.jsx         # WEPT-first entry flow
│   │   ├── PrepLists/
│   │   │   ├── PrepListOverview.jsx        # Calendar view
│   │   │   ├── PrepListGenerator.jsx       # Forecast-based generation
│   │   │   └── PrepListDetail.jsx          # Task assignment & completion tracking
│   │   ├── FoodSafety/
│   │   │   ├── FoodSafetyOverview.jsx      # Dashboard: allergens, HACCP, traceability
│   │   │   ├── AllergenDashboard.jsx       # Product × allergen matrix
│   │   │   ├── HaccpChecklistList.jsx
│   │   │   ├── HaccpTemplateManager.jsx
│   │   │   ├── HaccpTemplateForm.jsx
│   │   │   ├── HaccpChecklistEntry.jsx     # Mobile-optimized, offline support
│   │   │   ├── HaccpChecklistLogDetail.jsx
│   │   │   └── TraceabilitySearch.jsx      # Forward/backward ingredient tracing
│   │   ├── Reports/
│   │   │   ├── ReportsOverview.jsx         # 3-tier: Critical, Operational, Administrative
│   │   │   ├── ActualVsTheoreticalReport.jsx  # ★ #1 cost control report
│   │   │   ├── MenuEngineeringReport.jsx      # ★ 4-quadrant scatter plot
│   │   │   ├── StockLevelReport.jsx
│   │   │   ├── MovementHistoryReport.jsx
│   │   │   ├── ConsumptionReport.jsx
│   │   │   ├── ValuationReport.jsx
│   │   │   ├── WasteAnalysisReport.jsx     # WEPT breakdown
│   │   │   ├── COGSReport.jsx
│   │   │   ├── VarianceReport.jsx
│   │   │   └── SupplierPerformanceReport.jsx
│   │   ├── Alerts/
│   │   │   ├── AlertList.jsx
│   │   │   ├── AlertBadge.jsx
│   │   │   └── AlertDropdown.jsx
│   │   ├── Settings/
│   │   │   ├── InventorySettings.tsx
│   │   │   ├── GeneralSettings.tsx         # Food cost targets, variance thresholds
│   │   │   ├── OrganizationSettings.tsx    # Restaurant profile
│   │   │   ├── UserManagement.tsx          # Invite users, manage roles
│   │   │   ├── FoodSafetySettings.tsx      # HACCP/allergen/nutrition toggles
│   │   │   ├── IntegrationSettings.tsx
│   │   │   ├── PosConnectionList.tsx       # Any POS system
│   │   │   ├── PosConnectionForm.tsx
│   │   │   ├── ProductMappingManager.tsx
│   │   │   └── WebhookSubscriptionManager.tsx  # Outbound webhook management
│   │   └── Error/
│   │       └── Error.jsx
│   ├── redux/
│   │   ├── store.js
│   │   ├── slices/
│   │   │   ├── authSlice.js
│   │   │   ├── inventorySlice.js         # stock items, categories, storage locations
│   │   │   ├── recipesSlice.js           # recipes, menu engineering
│   │   │   ├── suppliersSlice.js         # suppliers, certifications
│   │   │   ├── purchaseOrdersSlice.js
│   │   │   ├── stockCountsSlice.js
│   │   │   ├── wasteSlice.js             # WEPT-classified waste
│   │   │   ├── prepListsSlice.js
│   │   │   ├── foodSafetySlice.js        # allergens, nutrition, HACCP
│   │   │   ├── reportsSlice.js           # + actual vs theoretical, menu engineering
│   │   │   ├── alertsSlice.js
│   │   │   ├── settingsSlice.js
│   │   │   ├── integrationsSlice.js      # + webhook subscriptions
│   │   │   └── uiSlice.js               # + dashboard widget layout
│   │   └── thunks/
│   │       ├── authThunk.js
│   │       ├── inventoryThunk.js
│   │       ├── categoryThunk.js
│   │       ├── storageLocationThunk.js
│   │       ├── recipeThunk.js
│   │       ├── supplierThunk.js
│   │       ├── purchaseOrderThunk.js
│   │       ├── stockCountThunk.js
│   │       ├── wasteThunk.js
│   │       ├── prepListThunk.js
│   │       ├── foodSafetyThunk.js
│   │       ├── haccpThunk.js
│   │       ├── reportThunk.js
│   │       ├── alertThunk.js
│   │       ├── integrationThunk.js
│   │       └── traceabilityThunk.js
│   ├── router/
│   │   ├── AuthRouter.tsx
│   │   ├── DashboardRouter.tsx
│   │   ├── StockItemRouter.jsx
│   │   ├── CategoryRouter.jsx
│   │   ├── StorageLocationRouter.jsx
│   │   ├── RecipeRouter.jsx
│   │   ├── SupplierRouter.jsx
│   │   ├── PurchaseOrderRouter.jsx
│   │   ├── StockCountRouter.jsx
│   │   ├── WasteRouter.jsx
│   │   ├── PrepListRouter.jsx
│   │   ├── FoodSafetyRouter.jsx
│   │   ├── ReportsRouter.jsx
│   │   ├── AlertRouter.jsx
│   │   └── SettingsRouter.jsx
│   ├── loaders/
│   │   ├── dashboardLoader.js
│   │   ├── stockItemsLoader.js
│   │   ├── categoriesLoader.js
│   │   ├── recipesLoader.js
│   │   ├── suppliersLoader.js
│   │   ├── purchaseOrdersLoader.js
│   │   ├── stockCountsLoader.js
│   │   ├── wasteLoader.js
│   │   ├── reportsLoader.js
│   │   ├── alertsLoader.js
│   │   ├── settingsLoader.js
│   │   └── connectionsLoader.js
│   ├── scss/
│   │   ├── abstract/
│   │   │   ├── _variables.scss
│   │   │   └── _mixins.scss
│   │   └── global.scss
│   ├── i18n.js
│   ├── App.jsx
│   └── main.jsx
├── index.html
├── vite.config.ts
├── tsconfig.json
├── tsconfig.node.json
├── tailwind.config.ts
├── postcss.config.js
├── package.json
├── Dockerfile
├── .dockerignore
├── .env.development
├── .env.production
├── .github/
│   └── workflows/
│       └── ci.yml                     # GitHub Actions CI/CD pipeline
└── README.md
```

---

## 16. Environment Variables

### .env.development

```env
# Points to OWN backend (NOT BonApp backend)
VITE_API_BASE_URL=http://localhost:5000

# Auth (own authentication — NOT BonApp's baseAccount)
VITE_AUTH_LOGIN=/api/auth/login
VITE_AUTH_REGISTER=/api/auth/register
VITE_AUTH_REFRESH=/api/auth/refresh
VITE_AUTH_FORGOT_PASSWORD=/api/auth/forgot-password
VITE_AUTH_RESET_PASSWORD=/api/auth/reset-password
VITE_AUTH_ME=/api/auth/me
VITE_AUTH_INVITE=/api/auth/invite
VITE_AUTH_ACCEPT_INVITE=/api/auth/accept-invite

# Stock Items
VITE_STOCK_ITEMS=/api/inventory/stock-items
VITE_CATEGORIES=/api/inventory/categories
VITE_STORAGE_LOCATIONS=/api/inventory/storage-locations

# Menu Items (own entity)
VITE_MENU_ITEMS=/api/inventory/menu-items

# Recipes
VITE_RECIPES=/api/inventory/recipes

# Suppliers
VITE_SUPPLIERS=/api/inventory/suppliers

# Purchase Orders
VITE_PURCHASE_ORDERS=/api/inventory/purchase-orders

# Stock Counts
VITE_STOCK_COUNTS=/api/inventory/stock-counts

# Waste
VITE_WASTE=/api/inventory/waste

# Reports
VITE_REPORTS=/api/inventory/reports

# Alerts
VITE_ALERTS=/api/inventory/alerts

# Food Safety
VITE_FOOD_SAFETY=/api/inventory/food-safety
VITE_HACCP=/api/inventory/haccp
VITE_PREP_LISTS=/api/inventory/prep-lists

# POS Connections (any POS — BonApp, Lightspeed, Toast, etc.)
VITE_POS_CONNECTIONS=/api/inventory/pos-connections
```

### .env.production

```env
VITE_API_BASE_URL=https://api.inventorypro.app
# ... same keys as development
```

---

## 17. Authentication Pages (Own — NOT BonApp's Login App)

Since this is a standalone application, it has its own complete authentication UI:

| Page | Description |
|------|-------------|
| `LoginPage.tsx` | Email + password login form. "Forgot password?" link. "Create account" link. Ant Design Card layout. Calls own backend `POST /api/auth/login`. |
| `RegisterPage.tsx` | Multi-step registration: 1) User info (name, email, password), 2) Organization info (restaurant name, address, currency, timezone). Creates user + organization. Calls `POST /api/auth/register`. |
| `ForgotPasswordPage.tsx` | Email input → sends password reset link. Calls `POST /api/auth/forgot-password`. |
| `ResetPasswordPage.tsx` | New password input (from email link with token). Calls `POST /api/auth/reset-password`. |
| `AcceptInvitePage.tsx` | When a user is invited to an organization, they click the email link and land here to set their password and join. Calls `POST /api/auth/accept-invite`. |
| `OrganizationSwitcher.tsx` | (In header) If user belongs to multiple organizations, dropdown to switch context. Updates `organizationId` in Redux. |

---

## 18. Development Phases

### Phase 1 — Project Setup, Auth & Foundation (Weeks 1-3)
- **Create standalone Vite + React + TypeScript project** (own repo, own configs)
- **Own authentication pages** — Login, Register, Forgot/Reset Password
- **Own Ant Design theme** (ConfigProvider with custom tokens)
- Layout components (Header with org switcher, Sidebar, Mobile Nav)
- Redux store setup with persist (auth, settings, ui slices)
- Axios config pointing to own Inventory Pro backend
- i18n setup (EN/DE/FR)
- Storage Location CRUD (list, form, detail, transfer modal)
- Stock Item CRUD (list, form with multi-tab, detail) — with PAR levels, alternate count units
- Stock Category CRUD
- Manual stock adjustment modal (with WEPT classification)
- Stock movement history view
- Unit conversion helper component
- Dashboard skeleton with drag-and-drop widget framework

**Files:** ~55 TSX + ~22 TS + ~12 SCSS

### Phase 2 — Menu Items, Recipes, Food Cost & Dashboard (Weeks 4-6)
- **Menu Item CRUD** (own entity — restaurants manage their menu items here OR sync from POS)
- Recipe/BOM CRUD (list, detail, form) with **live food cost calculation**
- Food cost indicator component
- Deduction preview
- Dashboard widgets (stock health, food cost gauge, PAR status, recent movements, quick actions)
- Chart.js integration for dashboard charts
- Stock level bar component (with PAR marker)

**Files:** ~22 TSX + ~8 TS

### Phase 3 — Suppliers & Purchase Orders (Weeks 7-9)
- Supplier CRUD (list, form, detail) with **certification management**
- Supplier-stock item linking with price history
- Purchase Order full lifecycle (list, form, detail)
- Goods Receiving form with **photo capture** and **temperature logging**
- **PAR-based** Reorder Suggestions page
- PO status badges and timeline

**Files:** ~28 TSX + ~10 TS

### Phase 4 — Stock Counts & Waste (Weeks 10-12)
- Stock Count session management with **shelf-to-sheet** location selection
- Stock Count Entry interface (mobile-optimized, **organized by storage location**, **alternate count units**, **offline support** via IndexedDB, **multi-user progress indicators**)
- Barcode scanner integration (item + case barcodes)
- **WEPT-classified** waste recording (list, form)
- Waste summary dashboard widget with WEPT breakdown

**Files:** ~18 TSX + ~8 TS

### Phase 5 — Actual vs Theoretical, Menu Engineering & Reports (Weeks 13-15)
- **Actual vs. Theoretical food cost report** — side-by-side, WEPT variance breakdown, trend chart
- **Menu Engineering Matrix report** — interactive 4-quadrant scatter plot with Recharts
- **Supplier Performance report** — scorecard with metrics
- All remaining report pages (stock levels, consumption, valuation, COGS, variance, movements)
- Date range filters, CSV/Excel/PDF export
- Report overview page (3-tier organization)
- Variance threshold bar component

**Files:** ~25 TSX + ~10 TS

### Phase 6 — Prep Lists & Food Safety (Weeks 16-19)
- **Prep List** overview (calendar), generator (forecast-based), detail (task assignment)
- **Food Safety overview** dashboard
- **Allergen Dashboard** — product × allergen matrix, auto-calculated from recipes
- **HACCP** checklist templates, mobile completion form (offline support), log viewer
- **Traceability Search** — forward/backward ingredient tracing
- Allergen icon bar, HACCP status indicators

**Files:** ~25 TSX + ~10 TS

### Phase 7 — Alerts, Settings, Integrations & Polish (Weeks 20-24)
- Alert system (list, badge, dropdown) — expanded types (HACCP overdue, cert expiring, below PAR)
- Alert polling
- **Organization settings** (name, address, currency, timezone)
- **User management** (invite users, manage roles)
- Inventory settings page with **food cost targets**, **variance thresholds**
- Food safety settings (HACCP/allergen/nutrition toggles)
- POS connection management (any POS system)
- Product mapping manager
- **Outbound webhook subscription manager**
- CSV import wizard for mappings
- i18n completion (all 3 languages — EN/DE/FR)
- End-to-end testing, performance optimization
- Docker build, CI/CD pipeline setup

**Files:** ~28 TSX + ~12 TS

---

## 19. Estimated Scope Summary

| Category | Count |
|----------|-------|
| **TSX Files** | ~155 (incl. auth pages, menu item pages, org settings) |
| **TS Files** | ~85 |
| **SCSS Files** | ~20 |
| **Translation Files** | 3 (EN/FR/DE) |
| **Redux Slices** | 15 (incl. own auth slice) |
| **Redux Thunk Files** | 18 (incl. authThunk, menuItemThunk) |
| **API Modules** | 17 (incl. authApi, menuItemApi) |
| **Router Files** | 16 (incl. AuthRouter) |
| **Loader Files** | 18 |
| **Custom Hooks** | 17 |
| **Helper Files** | 8 |
| **Reusable Components** | 22 |
| **Auth Pages** | 5 (Login, Register, Forgot/Reset Password, Accept Invite) |
| **Pages** | ~82 |
| **Total Files (est.)** | ~295 |
| **Comparable To** | A complete standalone SaaS application with its own auth, dashboard, and full feature set |

**Note:** This is a COMPLETE standalone frontend. Unlike a module added to a monorepo, this includes its own authentication pages, Ant Design theme configuration, Vite config, Docker build, and CI/CD pipeline.

---

## 20. Recommendations

### From Day 1

1. **Use TypeScript from Day 1** — Build the entire app in TypeScript (.tsx/.ts). Type safety is critical for the complex data structures (stock items, recipes, purchase orders, allergens, HACCP). Vite + React 18 supports TypeScript natively.

2. **Own Ant Design theme** — Create a custom `antdTheme.ts` using Ant Design's `ConfigProvider` to define the app's look and feel. Do NOT depend on any BonApp design tokens.

3. **Centralized API layer from the start** — Dedicated API modules (e.g., `stockItemApi.ts`, `authApi.ts`). Don't scatter API calls in thunks. All API calls go through `authorizedAxios` configured with JWT tokens.

4. **Add Vitest tests** — Set up proper component and hook tests from the beginning. Target 70%+ coverage. Especially critical for the food cost calculation logic and unit conversion system.

5. **Mobile-first CSS** — Many inventory tasks happen on mobile devices in kitchens and storage rooms. Design for mobile first, then enhance for desktop. Stock Count Entry, HACCP Checklist Entry, Waste Recording, and Prep List views are primarily mobile experiences.

6. **Offline support from Day 1** — Use IndexedDB (via `idb` library) to store stock count progress and HACCP checklist completions offline. Sync when connection resumes. This is critical because storage rooms and kitchens often have poor Wi-Fi.

7. **Consider React Query (TanStack Query)** — For the heavy server-state requirements (pagination, filtering, caching, background refetching). Can coexist with Redux Toolkit for client-side state. Particularly valuable for the reports section.

8. **Customizable Dashboard from Day 1** — Set up `react-grid-layout` early. Users expect to personalize their dashboard. Persist layout in Redux with `redux-persist`.

9. **Data Visualization Priority** — Invest in good chart components early (Recharts for scatter plots, Chart.js for standard charts). The Actual vs Theoretical report and Menu Engineering Matrix are the highest-value pages.

10. **Docker from Day 1** — Containerize the app with a multi-stage Dockerfile (build + nginx). This ensures consistent builds and easy deployment regardless of infrastructure.

### Future Enhancements

1. **SignalR for real-time alerts** — Push low stock alerts, HACCP reminders, and stock count progress to the browser instead of polling
2. **PWA support** — Full Progressive Web App for offline stock counting, HACCP checklists, and waste recording in storage rooms
3. **Multi-location dashboard** — Aggregate view across all restaurant locations with drill-down (for franchise operators)
4. **Vendor portal** — Allow suppliers to view their POs, confirm orders online, and update delivery schedules
5. **AI-powered features**:
   - Demand forecasting based on historical patterns, weather, events, day-of-week
   - Recipe optimization suggestions (reduce cost while maintaining quality)
   - Automatic waste reduction recommendations based on WEPT analysis
   - Menu engineering auto-suggestions (pricing, positioning)
6. **Integration marketplace** — Pre-built connectors for popular POS systems (BonApp, Lightspeed, Toast, Square, Orderbird) with guided setup wizards
7. **Customer-facing allergen display** — QR code on menu that links to real-time allergen information per dish (auto-generated from recipe data)
8. **Sustainability reporting** — Track food waste reduction over time, carbon footprint of supply chain, local sourcing percentage

---

## 21. Future BonApp Integration (Frontend)

When Inventory Pro connects to BonApp in the future, these are the frontend touch points:

1. **POS Connection page** — BonApp appears as one of the POS systems in the dropdown (alongside Lightspeed, Toast, Square, etc.). No special UI needed — BonApp is just another POS connection.

2. **Product Mapping page** — Map BonApp's Products to Inventory Pro's Menu Items. BonApp's product IDs are linked via the `ExternalProductId` field.

3. **Dashboard** — Stock deductions from BonApp orders appear in real-time as "Sold" movements in the Recent Movements widget, same as any other POS.

4. **Reports** — BonApp sales data flows into Actual vs. Theoretical, Menu Engineering, and Consumption reports automatically through the SaleRecord audit trail.

**No code changes needed in Inventory Pro frontend to support BonApp.** The UI already supports any POS system. Connecting to BonApp just means creating a new POS Connection and mapping products.

---

## 22. Industry Competitive Analysis (Frontend Feature Parity)

| Feature | MarketMan | Apicbase | CrunchTime | **Inventory Pro** |
|---------|-----------|----------|------------|---------------------|
| Customizable Dashboard | Basic | Yes | Yes | **Yes (drag-and-drop)** |
| Actual vs Theoretical Report | Yes | Yes | Yes | **Yes (with WEPT breakdown)** |
| Menu Engineering Matrix | No | Yes | Yes | **Yes (interactive scatter plot)** |
| PAR Level Visualization | Yes | No | Yes | **Yes (color-coded bars)** |
| Shelf-to-Sheet Counting | No | No | Yes | **Yes (by storage location)** |
| Alternate Count Units | No | No | Yes | **Yes (case/pack/bottle)** |
| Multi-user Counting | No | No | Yes | **Yes (real-time progress)** |
| Offline Counting | Yes (mobile app) | No | No | **Yes (IndexedDB)** |
| Photo Capture (Receiving) | No | No | No | **Yes (react-webcam)** |
| HACCP Digital Checklists | No | Yes | No | **Yes (mobile-optimized)** |
| Allergen Matrix | No | Yes | No | **Yes (product × allergen grid)** |
| Nutritional Calculator | No | Yes | No | **Yes (auto from recipes)** |
| Ingredient Traceability | No | Yes | No | **Yes (forward + backward)** |
| Prep List Generator | No | Yes | Yes | **Yes (forecast-based)** |
| Outbound Webhook UI | No | No | No | **Yes (subscription manager)** |
| Supplier Certifications | No | No | No | **Yes (compliance tracking)** |
| WEPT Waste Classification | No | No | Partial | **Yes (full WEPT framework)** |
| Multi-language (EN/DE/FR) | Limited | Limited | No | **Yes (full i18n)** |

---

*Report generated for Inventory Pro - Standalone Restaurant Inventory Management System - February 9, 2026 (Updated: standalone architecture)*
