import React, { useEffect, useState } from 'react';
import { Table, Button, Input, Space, Tag, Typography, Popconfirm, message } from 'antd';
import { PlusOutlined, SearchOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useDispatch, useSelector } from 'react-redux';
import type { RootState, AppDispatch } from '../../redux/store';
import { fetchStockItems } from '../../redux/slices/inventorySlice';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

const statusColors: Record<string, string> = {
  InStock: 'green', BelowPar: 'gold', Low: 'orange', OutOfStock: 'red', Overstock: 'blue',
};

const unitLabels: Record<number, string> = {
  0: 'pcs', 1: 'kg', 2: 'g', 3: 'L', 4: 'mL', 5: 'btl', 6: 'box', 7: 'pack', 8: 'doz', 9: 'portion',
};

export const StockItemList: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { user } = useSelector((state: RootState) => state.auth);
  const { stockItems, totalCount, page, pageSize, isLoading } = useSelector((state: RootState) => state.inventory);
  const [search, setSearch] = useState('');

  const orgId = user?.organizationId || 0;

  useEffect(() => {
    if (orgId !== 0) dispatch(fetchStockItems({ orgId, page, pageSize }));
  }, [dispatch, orgId, page, pageSize]);

  const handleSearch = () => {
    if (orgId !== 0) dispatch(fetchStockItems({ orgId, page: 1, pageSize, search }));
  };

  const handleDelete = async (id: number) => {
    if (orgId === 0) return;
    try {
      await authorizedAxios.delete(`/api/inventory/stock-items/${orgId}/${id}`);
      message.success('Stock item deleted');
      dispatch(fetchStockItems({ orgId, page, pageSize }));
    } catch { message.error('Failed to delete'); }
  };

  const columns = [
    { title: 'Name', dataIndex: 'name', key: 'name', render: (text: string, record: any) => <a onClick={() => navigate(`/stock-items/${record.id}`)}>{text}</a> },
    { title: 'SKU', dataIndex: 'sku', key: 'sku', width: 100 },
    { title: 'Category', dataIndex: 'categoryName', key: 'categoryName', width: 120 },
    { title: 'Quantity', dataIndex: 'currentQuantity', key: 'qty', width: 100, render: (qty: number, record: any) => <span style={{ color: record.stockStatus === 'OutOfStock' ? '#ff4d4f' : record.stockStatus === 'Low' ? '#faad14' : '#52c41a' }}>{qty.toFixed(2)}</span> },
    { title: 'Unit', dataIndex: 'baseUnitOfMeasurement', key: 'unit', width: 80, render: (u: number) => unitLabels[u] || 'pcs' },
    { title: 'Cost', dataIndex: 'costPrice', key: 'cost', width: 100, render: (v: number) => `CHF ${v.toFixed(2)}` },
    { title: 'Status', dataIndex: 'stockStatus', key: 'status', width: 100, render: (s: string) => <Tag color={statusColors[s] || 'default'}>{s}</Tag> },
    { title: 'Actions', key: 'actions', width: 120, render: (_: any, record: any) => (
      <Space>
        <Button size="small" icon={<EditOutlined />} onClick={() => navigate(`/stock-items/${record.id}/edit`)} />
        <Popconfirm title="Delete this item?" onConfirm={() => handleDelete(record.id)}><Button size="small" danger icon={<DeleteOutlined />} /></Popconfirm>
      </Space>
    )},
  ];

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>Stock Items</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/stock-items/new')}>Add Item</Button>
      </div>
      <Space style={{ marginBottom: 16 }}>
        <Input placeholder="Search items..." prefix={<SearchOutlined />} value={search} onChange={e => setSearch(e.target.value)} onPressEnter={handleSearch} style={{ width: 300 }} />
        <Button onClick={handleSearch}>Search</Button>
      </Space>
      <Table columns={columns} dataSource={stockItems} rowKey="id" loading={isLoading}
        pagination={{ current: page, pageSize, total: totalCount, showSizeChanger: true, showTotal: (t) => `Total ${t} items`,
          onChange: (p, ps) => { if (orgId !== 0) dispatch(fetchStockItems({ orgId, page: p, pageSize: ps, search })); } }} />
    </div>
  );
};
