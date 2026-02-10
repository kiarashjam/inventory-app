import React, { useEffect, useState } from 'react';
import { Table, Button, Space, Tag, Typography, Popconfirm, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useDispatch, useSelector } from 'react-redux';
import type { RootState, AppDispatch } from '../../redux/store';
import { fetchCategories } from '../../redux/slices/inventorySlice';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

function flattenCategories(cats: any[], out: any[] = []): any[] {
  if (!Array.isArray(cats)) return out;
  for (const c of cats) {
    out.push(c);
    if (Array.isArray(c.subCategories) && c.subCategories.length) flattenCategories(c.subCategories, out);
  }
  return out;
}

export const CategoryList: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { user } = useSelector((state: RootState) => state.auth);
  const { categories } = useSelector((state: RootState) => state.inventory);
  const orgId = user?.organizationId || 0;
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (orgId === 0) return;
    setLoading(true);
    dispatch(fetchCategories(orgId)).finally(() => setLoading(false));
  }, [dispatch, orgId]);

  const handleDelete = async (id: number) => {
    if (orgId === 0) return;
    try {
      await authorizedAxios.delete(`/api/inventory/categories/${orgId}/${id}`);
      message.success('Category deleted');
      dispatch(fetchCategories(orgId));
    } catch {
      message.error('Failed to delete');
    }
  };

  const flat = flattenCategories(categories);
  const getParentName = (parentId: number | null) => {
    if (parentId == null) return '—';
    const p = flat.find((c: any) => c.id === parentId);
    return p?.name ?? '—';
  };

  const columns = [
    { title: 'Name', dataIndex: 'name', key: 'name' },
    { title: 'Display Order', dataIndex: 'displayOrder', key: 'displayOrder', width: 120 },
    { title: 'Parent Category', key: 'parent', width: 160, render: (_: any, record: any) => getParentName(record.parentCategoryId) },
    { title: 'Active', dataIndex: 'isActive', key: 'active', width: 100, render: (v: boolean) => <Tag color={v ? 'green' : 'default'}>{v ? 'Yes' : 'No'}</Tag> },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_: any, record: any) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => navigate(`/categories/${record.id}/edit`)} />
          <Popconfirm title="Delete this category?" onConfirm={() => handleDelete(record.id)}>
            <Button size="small" danger icon={<DeleteOutlined />} />
          </Popconfirm>
        </Space>
      ),
    },
  ];

  if (orgId === 0) return null;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>Categories</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/categories/new')}>Add Category</Button>
      </div>
      <Table columns={columns} dataSource={flat} rowKey="id" loading={loading} pagination={{ pageSize: 20, showTotal: (t) => `Total ${t} categories` }} />
    </div>
  );
};
