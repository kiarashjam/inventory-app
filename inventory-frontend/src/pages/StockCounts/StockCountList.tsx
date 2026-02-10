import React, { useEffect, useState } from 'react';
import { Table, Button, Space, Tag, Typography, message } from 'antd';
import { PlusOutlined, EyeOutlined, StopOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';
import { Popconfirm } from 'antd';

const { Title } = Typography;

const statusColors: Record<string, string> = {
  InProgress: 'blue',
  Completed: 'orange',
  Approved: 'green',
  Cancelled: 'red',
};

export const StockCountList: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [data, setData] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const load = (p: number = page, ps: number = pageSize) => {
    if (orgId === 0) return;
    setLoading(true);
    authorizedAxios
      .get(`/api/inventory/stock-counts/${orgId}`, { params: { page: p, pageSize: ps } })
      .then((res) => {
        const body = res.data?.data ?? res.data;
        const items = Array.isArray(body?.items) ? body.items : (Array.isArray(body) ? body : []);
        setData(items);
        setTotal(body?.totalCount ?? body?.total ?? items.length);
      })
      .catch(() => message.error('Failed to load stock counts'))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    let mounted = true;
    if (orgId !== 0) {
      setLoading(true);
      authorizedAxios
        .get(`/api/inventory/stock-counts/${orgId}`, { params: { page, pageSize } })
        .then((res) => {
          if (mounted) {
            const body = res.data?.data ?? res.data;
            const items = Array.isArray(body?.items) ? body.items : (Array.isArray(body) ? body : []);
            setData(items);
            setTotal(body?.totalCount ?? body?.total ?? items.length);
          }
        })
        .catch(() => { if (mounted) message.error('Failed to load stock counts'); })
        .finally(() => { if (mounted) setLoading(false); });
    }
    return () => { mounted = false; };
  }, [orgId, page, pageSize]);

  const handleCancel = async (id: number) => {
    if (orgId === 0) return;
    try {
      await authorizedAxios.post(`/api/inventory/stock-counts/${orgId}/${id}/cancel`);
      message.success('Count cancelled');
      load(page, pageSize);
    } catch {
      message.error('Failed to cancel');
    }
  };

  const formatDate = (d: string | null) => (d ? new Date(d).toLocaleDateString() : '–');

  const columns = [
    { title: 'Count Date', dataIndex: 'countDate', key: 'countDate', width: 120, render: formatDate },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      render: (s: string) => <Tag color={statusColors[s] ?? 'default'}>{s ?? '–'}</Tag>,
    },
    { title: 'Item Count', dataIndex: 'itemCount', key: 'itemCount', width: 100 },
    {
      title: 'Total Variance Value (CHF)',
      dataIndex: 'totalVarianceValue',
      key: 'totalVarianceValue',
      width: 180,
      render: (v: number) => (v != null ? `CHF ${Number(v).toFixed(2)}` : '–'),
    },
    { title: 'Created By', dataIndex: 'createdByName', key: 'createdBy', width: 120 },
    {
      title: 'Actions',
      key: 'actions',
      width: 160,
      render: (_: any, record: any) => (
        <Space>
          <Button size="small" icon={<EyeOutlined />} onClick={() => navigate(`/stock-counts/${record.id}`)}>View</Button>
          {record.status === 'InProgress' && (
            <Popconfirm title="Cancel this count?" onConfirm={() => handleCancel(record.id)}>
              <Button size="small" danger icon={<StopOutlined />}>Cancel</Button>
            </Popconfirm>
          )}
        </Space>
      ),
    },
  ];

  if (orgId === 0) return null;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>Stock Counts</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/stock-counts/new')}>
          Start New Count
        </Button>
      </div>
      <Table
        columns={columns}
        dataSource={data}
        rowKey="id"
        loading={loading}
        pagination={{
          current: page,
          pageSize,
          total,
          showSizeChanger: true,
          showTotal: (t) => `Total ${t} counts`,
          onChange: (p, ps) => {
            setPage(p);
            setPageSize(ps ?? pageSize);
          },
        }}
      />
    </div>
  );
};
