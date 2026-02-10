import { createSlice, PayloadAction } from '@reduxjs/toolkit';

interface UiState {
  lang: string;
  sidebarCollapsed: boolean;
  mobileMenuOpen: boolean;
}

const initialState: UiState = {
  lang: 'en',
  sidebarCollapsed: false,
  mobileMenuOpen: false,
};

const uiSlice = createSlice({
  name: 'ui',
  initialState,
  reducers: {
    setLanguage: (state, action: PayloadAction<string>) => { state.lang = action.payload; },
    toggleSidebar: (state) => { state.sidebarCollapsed = !state.sidebarCollapsed; },
    setSidebarCollapsed: (state, action: PayloadAction<boolean>) => { state.sidebarCollapsed = action.payload; },
    toggleMobileMenu: (state) => { state.mobileMenuOpen = !state.mobileMenuOpen; },
    closeMobileMenu: (state) => { state.mobileMenuOpen = false; },
  },
});

export const { setLanguage, toggleSidebar, setSidebarCollapsed, toggleMobileMenu, closeMobileMenu } = uiSlice.actions;
export default uiSlice.reducer;
