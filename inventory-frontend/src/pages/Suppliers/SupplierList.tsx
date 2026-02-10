import React, { useEffect, useState } from 'react';
import { Table, Button, Input, Space, Tag, Typography, Popconfirm, message } from 'antd';
import { PlusOutlined, SearchOutlined, EyeOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

export const SupplierList: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [suppliers, setSuppliers] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [search, setSearch] = useState('');

  const loadSuppliers = (searchTerm?: string) => {
    if (orgId === 0) return;
    setLoading(true);
    const params = searchTerm ? `?search=${encodeURIComponent(searchTerm)}` : '';
    authorizedAxios
      .get(`/api/inventory/suppliers/${orgId}${params}`)
      .then((res) => {
        const data = res.data?.data ?? res.data;
        setSuppliers(Array.isArray(data) ? data : []);
      })
      .catch(() => message.error('Failed to load suppliers'))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    if (orgId === 0) return;
    let mounted = true;
    setLoading(true);
    authorizedAxios
      .get(`/api/inventory/suppliers/${orgId}`)
      .then((res) => {
        if (mounted) {
          const data = res.data?.data ?? res.data;
          setSuppliers(Array.isArray(data) ? data : []);
        }
      })
      .catch(() => {
        if (mounted) message.error('Failed to load suppliers');
      })
      .finally(() => {
        if (mounted) setLoading(false);
      });
    return () => {
      mounted = false;
    };
  }, [orgId]);

  const handleSearch = () => {
    loadSuppliers(search);
  };

  const handleDelete = async (id: number) => {
    if (orgId === 0) return;
    try {
      await authorizedAxios.delete(`/api/inventory/suppliers/${orgId}/${id}`);
      message.success('Supplier deleted');
      loadSuppliers(search);
    } catch {
      message.error('Failed to delete');
    }
  };

  const columns = [
    { title: 'Name', dataIndex: 'name', key: 'name', render: (text: string, record: any) => <a onClick={() => navigate(`/suppliers/${record.id}/edit`)}>{text}</a> },
    { title: 'Contact Person', dataIndex: 'contactPerson', key: 'contactPerson', width: 140 },
    { title: 'Email', dataIndex: 'email', key: 'email', width: 180 },
    { title: 'Phone', dataIndex: 'phone', key: 'phone', width: 120 },
    { title: 'Lead Time (days)', dataIndex: 'leadTimeDays', key: 'leadTimeDays', width: 120 },
    { title: 'Active', dataIndex: 'isActive', key: 'active', width: 100, render: (v: boolean) => <Tag color={v ? 'green' : 'default'}>{v ? 'Yes' : 'No'}</Tag> },
    {
      title: 'Actions',
      key: 'actions',
      width: 140,
      render: (_: any, record: any) => (
        <Space>
          <Button size="small" icon={<EyeOutlined />} onClick={() => navigate(`/suppliers/${record.id}/edit`)} />
          <Button size="small" icon={<EditOutlined />} onClick={() => navigate(`/suppliers/${record.id}/edit`)} />
          <Popconfirm title="Delete this supplier?" onConfirm={() => handleDelete(record.id)}>
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
        <Title level={3} style={{ margin: 0 }}>Suppliers</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/suppliers/new')}>Add Supplier</Button>
      </div>
      <Space style={{ marginBottom: 16 }}>
        <Input placeholder="Search by name..." prefix={<SearchOutlined />} value={search} onChange={(e) => setSearch(e.target.value)} onPressEnter={handleSearch} style={{ width: 300 }} />
        <Button onClick={handleSearch}>Search</Button>
      </Space>
      <Table columns={columns} dataSource={suppliers} rowKey="id" loading={loading} pagination={{ pageSize: 20, showTotal: (t) => `Total ${t} suppliers` }} />
    </div>
  );
};
