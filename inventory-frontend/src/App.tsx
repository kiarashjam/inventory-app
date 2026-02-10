import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from './redux/store';
import { AppLayout } from './components/Layout/AppLayout';
import { LoginPage } from './pages/Auth/LoginPage';
import { RegisterPage } from './pages/Auth/RegisterPage';
import { Dashboard } from './pages/Dashboard/Dashboard';
import { StockItemList } from './pages/StockItems/StockItemList';
import { StockItemForm } from './pages/StockItems/StockItemForm';
import { StockItemDetail } from './pages/StockItems/StockItemDetail';
import { MenuItemList } from './pages/MenuItems/MenuItemList';
import { MenuItemForm } from './pages/MenuItems/MenuItemForm';
import { RecipeDetail } from './pages/Recipes/RecipeDetail';
import { RecipeForm } from './pages/Recipes/RecipeForm';
import { SettingsPage } from './pages/Settings/SettingsPage';
import { CategoryList } from './pages/Categories/CategoryList';
import { CategoryForm } from './pages/Categories/CategoryForm';
import { StorageLocationList } from './pages/StorageLocations/StorageLocationList';
import { StorageLocationForm } from './pages/StorageLocations/StorageLocationForm';
import { SupplierList } from './pages/Suppliers/SupplierList';
import { SupplierForm } from './pages/Suppliers/SupplierForm';
import { AlertList } from './pages/Alerts/AlertList';
import { PurchaseOrderList } from './pages/PurchaseOrders/PurchaseOrderList';
import { PurchaseOrderForm } from './pages/PurchaseOrders/PurchaseOrderForm';
import { PurchaseOrderDetail } from './pages/PurchaseOrders/PurchaseOrderDetail';
import { WasteRecordList } from './pages/Waste/WasteRecordList';
import { WasteRecordForm } from './pages/Waste/WasteRecordForm';
import { StockCountList } from './pages/StockCounts/StockCountList';
import { StockCountStart } from './pages/StockCounts/StockCountStart';
import { StockCountDetail } from './pages/StockCounts/StockCountDetail';
import { ReportsPage } from './pages/Reports/ReportsPage';
import { PosConnectionsPage } from './pages/Integrations/PosConnectionsPage';

const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated } = useSelector((state: RootState) => state.auth);
  const hasToken = !!localStorage.getItem('accessToken');
  return (isAuthenticated && hasToken) ? <>{children}</> : <Navigate to="/login" replace />;
};

const App: React.FC = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/" element={<ProtectedRoute><AppLayout /></ProtectedRoute>}>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard" element={<Dashboard />} />
          <Route path="stock-items" element={<StockItemList />} />
          <Route path="stock-items/new" element={<StockItemForm />} />
          <Route path="stock-items/:id/edit" element={<StockItemForm />} />
          <Route path="stock-items/:id" element={<StockItemDetail />} />
          <Route path="categories" element={<CategoryList />} />
          <Route path="categories/new" element={<CategoryForm />} />
          <Route path="categories/:id/edit" element={<CategoryForm />} />
          <Route path="storage-locations" element={<StorageLocationList />} />
          <Route path="storage-locations/new" element={<StorageLocationForm />} />
          <Route path="storage-locations/:id/edit" element={<StorageLocationForm />} />
          <Route path="suppliers" element={<SupplierList />} />
          <Route path="suppliers/new" element={<SupplierForm />} />
          <Route path="suppliers/:id/edit" element={<SupplierForm />} />
          <Route path="alerts" element={<AlertList />} />
          <Route path="purchase-orders" element={<PurchaseOrderList />} />
          <Route path="purchase-orders/new" element={<PurchaseOrderForm />} />
          <Route path="purchase-orders/:id" element={<PurchaseOrderDetail />} />
          <Route path="purchase-orders/:id/edit" element={<PurchaseOrderForm />} />
          <Route path="stock-counts" element={<StockCountList />} />
          <Route path="stock-counts/new" element={<StockCountStart />} />
          <Route path="stock-counts/:id" element={<StockCountDetail />} />
          <Route path="waste" element={<WasteRecordList />} />
          <Route path="waste/new" element={<WasteRecordForm />} />
          <Route path="menu-items" element={<MenuItemList />} />
          <Route path="menu-items/new" element={<MenuItemForm />} />
          <Route path="menu-items/:id/edit" element={<MenuItemForm />} />
          <Route path="recipes/:menuItemId" element={<RecipeDetail />} />
          <Route path="recipes/:menuItemId/edit" element={<RecipeForm />} />
          <Route path="integrations" element={<PosConnectionsPage />} />
          <Route path="food-safety" element={<div>Food Safety Page - Coming Soon</div>} />
          <Route path="reports" element={<ReportsPage />} />
          <Route path="settings" element={<SettingsPage />} />
          <Route path="*" element={<Navigate to="/dashboard" replace />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
};

export default App;
