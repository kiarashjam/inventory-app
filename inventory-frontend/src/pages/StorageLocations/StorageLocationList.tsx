import React, { useEffect, useState } from 'react';
import { Table, Button, Space, Tag, Typography, Popconfirm, message } from 'antd';
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useDispatch, useSelector } from 'react-redux';
import type { RootState, AppDispatch } from '../../redux/store';
import { fetchStorageLocations } from '../../redux/slices/inventorySlice';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

const locationTypeLabels: Record<number, string> = {
  0: 'Walk-in Cooler',
  1: 'Freezer',
  2: 'Dry Storage',
  3: 'Bar',
  4: 'Wine Cellar',
  5: 'Prep Station',
  6: 'Display Case',
  7: 'Other',
};

export const StorageLocationList: React.FC = () => {
  const dispatch = useDispatch<AppDispatch>();
  const navigate = useNavigate();
  const { user } = useSelector((state: RootState) => state.auth);
  const { storageLocations } = useSelector((state: RootState) => state.inventory);
  const orgId = user?.organizationId || 0;
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (orgId === 0) return;
    setLoading(true);
    dispatch(fetchStorageLocations(orgId)).finally(() => setLoading(false));
  }, [dispatch, orgId]);

  const handleDelete = async (id: number) => {
    if (orgId === 0) return;
    try {
      await authorizedAxios.delete(`/api/inventory/storage-locations/${orgId}/${id}`);
      message.success('Storage location deleted');
      dispatch(fetchStorageLocations(orgId));
    } catch {
      message.error('Failed to delete');
    }
  };

  const tempRange = (min: number | null, max: number | null) => {
    if (min == null && max == null) return '—';
    if (min != null && max != null) return `${min}°C – ${max}°C`;
    if (min != null) return `≥ ${min}°C`;
    return `≤ ${max}°C`;
  };

  const columns = [
    { title: 'Name', dataIndex: 'name', key: 'name' },
    { title: 'Type', dataIndex: 'locationType', key: 'type', width: 140, render: (t: number) => <Tag>{locationTypeLabels[t] ?? 'Other'}</Tag> },
    { title: 'Temperature Range', key: 'temp', width: 160, render: (_: any, r: any) => tempRange(r.temperatureMin, r.temperatureMax) },
    { title: 'Display Order', dataIndex: 'displayOrder', key: 'displayOrder', width: 120 },
    { title: 'Active', dataIndex: 'isActive', key: 'active', width: 100, render: (v: boolean) => <Tag color={v ? 'green' : 'default'}>{v ? 'Yes' : 'No'}</Tag> },
    {
      title: 'Actions',
      key: 'actions',
      width: 120,
      render: (_: any, record: any) => (
        <Space>
          <Button size="small" icon={<EditOutlined />} onClick={() => navigate(`/storage-locations/${record.id}/edit`)} />
          <Popconfirm title="Delete this location?" onConfirm={() => handleDelete(record.id)}>
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
        <Title level={3} style={{ margin: 0 }}>Storage Locations</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/storage-locations/new')}>Add Location</Button>
      </div>
      <Table columns={columns} dataSource={storageLocations} rowKey="id" loading={loading} pagination={{ pageSize: 20, showTotal: (t) => `Total ${t} locations` }} />
    </div>
  );
};
