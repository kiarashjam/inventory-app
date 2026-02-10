import React, { useEffect, useState } from 'react';
import { Table, Button, Space, Tag, Typography, Popconfirm, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined, BookOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

const foodCostColor = (pct: number | null | undefined): string => {
  if (pct == null) return undefined as any;
  if (pct < 30) return 'green';
  if (pct <= 35) return 'gold';
  return 'red';
};

export const MenuItemList: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);

  const load = () => {
    if (orgId === 0) return;
    setLoading(true);
    authorizedAxios
      .get(`/api/inventory/menu-items/${orgId}`)
      .then((res) => {
        const data = res.data?.data ?? res.data;
        const list = Array.isArray(data?.items) ? data.items : (Array.isArray(data) ? data : []);
        setItems(list);
      })
      .catch(() => message.error('Failed to load menu items'))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    let mounted = true;
    if (orgId !== 0) {
      setLoading(true);
      authorizedAxios
        .get(`/api/inventory/menu-items/${orgId}`)
        .then((res) => {
          if (mounted) {
            const data = res.data?.data ?? res.data;
            const list = Array.isArray(data?.items) ? data.items : (Array.isArray(data) ? data : []);
            setItems(list);
          }
        })
        .catch(() => { if (mounted) message.error('Failed to load menu items'); })
        .finally(() => { if (mounted) setLoading(false); });
    }
    return () => { mounted = false; };
  }, [orgId]);

  const handleDelete = async (id: number) => {
    if (orgId === 0) return;
    try {
      await authorizedAxios.delete(`/api/inventory/menu-items/${orgId}/${id}`);
      message.success('Menu item deleted');
      load();
    } catch {
      message.error('Failed to delete');
    }
  };

  const columns = [
    { title: 'Name', dataIndex: 'name', key: 'name' },
    { title: 'Category', dataIndex: 'category', key: 'category', width: 140 },
    { title: 'Selling Price (CHF)', dataIndex: 'sellingPrice', key: 'sellingPrice', width: 140, render: (v: number) => v != null ? `CHF ${Number(v).toFixed(2)}` : '–' },
    { title: 'Food Cost %', dataIndex: 'foodCostPercent', key: 'foodCostPercent', width: 120, render: (v: number) => v != null ? <Tag color={foodCostColor(v)}>{Number(v).toFixed(1)}%</Tag> : '–' },
    { title: 'Active', dataIndex: 'isActive', key: 'isActive', width: 90, render: (v: boolean) => <Tag color={v !== false ? 'green' : 'default'}>{v !== false ? 'Yes' : 'No'}</Tag> },
    {
      title: 'Actions',
      key: 'actions',
      width: 200,
      render: (_: any, record: any) => (
        <Space>
          <Button size="small" icon={<BookOutlined />} onClick={() => navigate(`/recipes/${record.id}`)}>View Recipe</Button>
          <Button size="small" icon={<EditOutlined />} onClick={() => navigate(`/menu-items/${record.id}/edit`)} />
          <Popconfirm title="Delete this menu item?" onConfirm={() => handleDelete(record.id)}>
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>Menu Items</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/menu-items/new')}>New Menu Item</Button>
      </div>
      <Table columns={columns} dataSource={items} rowKey="id" loading={loading} />
    </div>
  );
};
