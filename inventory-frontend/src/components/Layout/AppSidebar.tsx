import React from 'react';
import { Layout, Menu } from 'antd';
import { DashboardOutlined, InboxOutlined, AppstoreOutlined, EnvironmentOutlined, ShopOutlined, ShoppingCartOutlined, AuditOutlined, DeleteOutlined, BarChartOutlined, AlertOutlined, SettingOutlined, SafetyCertificateOutlined, MenuOutlined, ApiOutlined } from '@ant-design/icons';
import { useNavigate, useLocation } from 'react-router';

const { Sider } = Layout;

interface AppSidebarProps {
  collapsed: boolean;
}

export const AppSidebar: React.FC<AppSidebarProps> = ({ collapsed }) => {
  const navigate = useNavigate();
  const location = useLocation();

  const menuItems = [
    { key: '/dashboard', icon: <DashboardOutlined />, label: 'Dashboard' },
    { key: '/stock-items', icon: <InboxOutlined />, label: 'Stock Items' },
    { key: '/categories', icon: <AppstoreOutlined />, label: 'Categories' },
    { key: '/storage-locations', icon: <EnvironmentOutlined />, label: 'Storage Locations' },
    { key: '/suppliers', icon: <ShopOutlined />, label: 'Suppliers' },
    { key: '/menu-items', icon: <MenuOutlined />, label: 'Menu Items' },
    { key: '/purchase-orders', icon: <ShoppingCartOutlined />, label: 'Purchase Orders' },
    { key: '/stock-counts', icon: <AuditOutlined />, label: 'Stock Counts' },
    { key: '/waste', icon: <DeleteOutlined />, label: 'Waste' },
    { key: '/food-safety', icon: <SafetyCertificateOutlined />, label: 'Food Safety' },
    { key: '/reports', icon: <BarChartOutlined />, label: 'Reports' },
    { key: '/alerts', icon: <AlertOutlined />, label: 'Alerts' },
    { key: '/integrations', icon: <ApiOutlined />, label: 'Integrations' },
    { key: '/settings', icon: <SettingOutlined />, label: 'Settings' },
  ];

  const selectedKey = '/' + location.pathname.split('/')[1];

  return (
    <Sider trigger={null} collapsible collapsed={collapsed} breakpoint="lg" style={{ overflow: 'auto', height: '100vh', position: 'sticky', top: 0, left: 0 }}>
      <div style={{ height: 64, display: 'flex', alignItems: 'center', justifyContent: 'center', color: '#fff', fontSize: collapsed ? 16 : 20, fontWeight: 700 }}>
        {collapsed ? 'IP' : 'Inventory Pro'}
      </div>
      <Menu theme="dark" mode="inline" selectedKeys={[selectedKey]} items={menuItems} onClick={({ key }) => navigate(key)} />
    </Sider>
  );
};
