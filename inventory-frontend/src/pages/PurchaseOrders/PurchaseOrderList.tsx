import React, { useEffect, useState } from 'react';
import { Table, Button, Space, Tag, Typography, Popconfirm, message } from 'antd';
import { PlusOutlined, EyeOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

const statusColors: Record<string, string> = {
  Draft: 'default',
  Submitted: 'blue',
  Confirmed: 'cyan',
  PartiallyReceived: 'orange',
  FullyReceived: 'green',
  Cancelled: 'red',
  Closed: 'purple',
};

export const PurchaseOrderList: React.FC = () => {
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
      .get(`/api/inventory/purchase-orders/${orgId}`, { params: { page: p, pageSize: ps } })
      .then((res) => {
        const body = res.data?.data ?? res.data;
        const items = Array.isArray(body?.items) ? body.items : (Array.isArray(body) ? body : []);
        setData(items);
        setTotal(body?.totalCount ?? body?.total ?? items.length);
      })
      .catch(() => message.error('Failed to load purchase orders'))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    let mounted = true;
    if (orgId !== 0) {
      setLoading(true);
      authorizedAxios
        .get(`/api/inventory/purchase-orders/${orgId}`, { params: { page, pageSize } })
        .then((res) => {
          if (mounted) {
            const body = res.data?.data ?? res.data;
            const items = Array.isArray(body?.items) ? body.items : (Array.isArray(body) ? body : []);
            setData(items);
            setTotal(body?.totalCount ?? body?.total ?? items.length);
          }
        })
        .catch(() => { if (mounted) message.error('Failed to load purchase orders'); })
        .finally(() => { if (mounted) setLoading(false); });
    }
    return () => { mounted = false; };
  }, [orgId, page, pageSize]);

  const handleDelete = async (id: number) => {
    if (orgId === 0) return;
    try {
      await authorizedAxios.delete(`/api/inventory/purchase-orders/${orgId}/${id}`);
      message.success('Purchase order deleted');
      load(page, pageSize);
    } catch {
      message.error('Failed to delete');
    }
  };

  const formatDate = (d: string | null) => (d ? new Date(d).toLocaleDateString() : '–');

  const columns = [
    {
      title: 'Order #',
      dataIndex: 'orderNumber',
      key: 'orderNumber',
      width: 120,
      render: (text: string, record: any) => (
        <a onClick={() => navigate(`/purchase-orders/${record.id}`)}>{text ?? record.id}</a>
      ),
    },
    { title: 'Supplier Name', dataIndex: 'supplierName', key: 'supplierName', width: 180 },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      width: 130,
      render: (s: string) => <Tag color={statusColors[s] ?? 'default'}>{s ?? '–'}</Tag>,
    },
    { title: 'Order Date', dataIndex: 'orderDate', key: 'orderDate', width: 110, render: formatDate },
    { title: 'Expected Delivery', dataIndex: 'expectedDeliveryDate', key: 'expectedDelivery', width: 130, render: formatDate },
    { title: 'Items Count', dataIndex: 'itemsCount', key: 'itemsCount', width: 100 },
    {
      title: 'Total Amount',
      dataIndex: 'totalAmount',
      key: 'totalAmount',
      width: 120,
      render: (v: number) => (v != null ? `CHF ${Number(v).toFixed(2)}` : '–'),
    },
    {
      title: 'Actions',
      key: 'actions',
      width: 160,
      render: (_: any, record: any) => (
        <Space>
          <Button size="small" icon={<EyeOutlined />} onClick={() => navigate(`/purchase-orders/${record.id}`)}>View</Button>
          {record.status === 'Draft' && (
            <>
              <Button size="small" icon={<EditOutlined />} onClick={() => navigate(`/purchase-orders/${record.id}/edit`)}>Edit</Button>
              <Popconfirm title="Delete this purchase order?" onConfirm={() => handleDelete(record.id)}>
                <Button size="small" danger icon={<DeleteOutlined />}>Delete</Button>
              </Popconfirm>
            </>
          )}
        </Space>
      ),
    },
  ];

  if (orgId === 0) return null;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>Purchase Orders</Title>
        <Button type="primary" icon={<PlusOutlined />} onClick={() => navigate('/purchase-orders/new')}>
          New Purchase Order
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
          showTotal: (t) => `Total ${t} orders`,
          onChange: (p, ps) => {
            setPage(p);
            setPageSize(ps ?? pageSize);
          },
        }}
      />
    </div>
  );
};
