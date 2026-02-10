import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { authorizedAxios } from '../../config/axios/axios-config';

interface StockItem {
  id: number;
  name: string;
  sku: string | null;
  description: string | null;
  categoryId: number;
  categoryName: string;
  baseUnitOfMeasurement: number;
  currentQuantity: number;
  minimumThreshold: number;
  parLevel: number | null;
  maximumCapacity: number | null;
  costPrice: number;
  averageCostPrice: number;
  primarySupplierId: number | null;
  primarySupplierName: string | null;
  barcode: string | null;
  primaryStorageLocationId: number | null;
  primaryStorageLocationName: string | null;
  isActive: boolean;
  isPerishable: boolean;
  notes: string | null;
  createdAt: string;
  stockStatus: string;
}

interface Category {
  id: number;
  name: string;
  displayOrder: number;
  parentCategoryId: number | null;
  isActive: boolean;
  subCategories: Category[];
}

interface StorageLocation {
  id: number;
  name: string;
  description: string | null;
  locationType: number;
  displayOrder: number;
  temperatureMin: number | null;
  temperatureMax: number | null;
  isActive: boolean;
}

interface InventoryState {
  stockItems: StockItem[];
  selectedStockItem: StockItem | null;
  categories: Category[];
  storageLocations: StorageLocation[];
  totalCount: number;
  page: number;
  pageSize: number;
  isLoading: boolean;
  error: string | null;
}

const initialState: InventoryState = {
  stockItems: [],
  selectedStockItem: null,
  categories: [],
  storageLocations: [],
  totalCount: 0,
  page: 1,
  pageSize: 20,
  isLoading: false,
  error: null,
};

export const fetchStockItems = createAsyncThunk('inventory/fetchStockItems',
  async ({ orgId, page, pageSize, search, categoryId }: { orgId: number; page?: number; pageSize?: number; search?: string; categoryId?: number }, { rejectWithValue }) => {
    try {
      const params = new URLSearchParams();
      if (page) params.append('page', page.toString());
      if (pageSize) params.append('pageSize', pageSize.toString());
      if (search) params.append('search', search);
      if (categoryId) params.append('categoryId', categoryId.toString());
      const response = await authorizedAxios.get(`/api/inventory/stock-items/${orgId}?${params}`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch stock items');
    }
  }
);

export const fetchCategories = createAsyncThunk('inventory/fetchCategories',
  async (orgId: number, { rejectWithValue }) => {
    try {
      const response = await authorizedAxios.get(`/api/inventory/categories/${orgId}`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch categories');
    }
  }
);

export const fetchStorageLocations = createAsyncThunk('inventory/fetchStorageLocations',
  async (orgId: number, { rejectWithValue }) => {
    try {
      const response = await authorizedAxios.get(`/api/inventory/storage-locations/${orgId}`);
      return response.data;
    } catch (error: any) {
      return rejectWithValue(error.response?.data?.message || 'Failed to fetch storage locations');
    }
  }
);

const inventorySlice = createSlice({
  name: 'inventory',
  initialState,
  reducers: {
    clearSelectedStockItem: (state) => { state.selectedStockItem = null; },
    setPage: (state, action) => { state.page = action.payload; },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchStockItems.pending, (state) => { state.isLoading = true; state.error = null; })
      .addCase(fetchStockItems.fulfilled, (state, action) => {
        state.isLoading = false;
        state.error = null;
        const data = action.payload?.data ?? action.payload;
        state.stockItems = Array.isArray(data?.items) ? data.items : (data?.items ?? []);
        state.totalCount = typeof data?.totalCount === 'number' ? data.totalCount : 0;
        state.page = typeof data?.page === 'number' ? data.page : state.page;
        state.pageSize = typeof data?.pageSize === 'number' ? data.pageSize : state.pageSize;
      })
      .addCase(fetchStockItems.rejected, (state, action) => {
        state.isLoading = false;
        state.error = (action.payload as string) || 'Failed to fetch stock items';
      })
      .addCase(fetchCategories.fulfilled, (state, action) => {
        const data = action.payload?.data ?? action.payload;
        state.categories = Array.isArray(data) ? data : (data ?? []);
      })
      .addCase(fetchStorageLocations.fulfilled, (state, action) => {
        const data = action.payload?.data ?? action.payload;
        state.storageLocations = Array.isArray(data) ? data : (data ?? []);
      });
  },
});

export const { clearSelectedStockItem, setPage } = inventorySlice.actions;
export default inventorySlice.reducer;
