import React, { useEffect, useState } from 'react';
import { Table, Button, Space, Tag, Typography, message } from 'antd';
import { CheckOutlined, CheckCircleOutlined } from '@ant-design/icons';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

const alertTypeColors: Record<string, string> = {
  LowStock: 'orange',
  OutOfStock: 'red',
  Expiring: 'gold',
  Expired: 'red',
  Overstock: 'blue',
  Default: 'default',
};

export const AlertList: React.FC = () => {
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [alerts, setAlerts] = useState<any[]>([]);
  const [loading, setLoading] = useState(false);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  const loadAlerts = (p: number, ps: number, isMounted?: () => boolean) => {
    if (orgId === 0) return;
    setLoading(true);
    authorizedAxios
      .get(`/api/inventory/alerts/${orgId}`, { params: { page: p, pageSize: ps } })
      .then((res) => {
        if (isMounted && !isMounted()) return;
        const data = res.data?.data ?? res.data;
        const items = Array.isArray(data?.items) ? data.items : (Array.isArray(data) ? data : []);
        setAlerts(items);
        setTotal(typeof data?.totalCount === 'number' ? data.totalCount : items.length);
      })
      .catch(() => {
        if (!isMounted || isMounted()) message.error('Failed to load alerts');
      })
      .finally(() => {
        if (!isMounted || isMounted()) setLoading(false);
      });
  };

  useEffect(() => {
    if (orgId === 0) return;
    let mounted = true;
    loadAlerts(page, pageSize, () => mounted);
    return () => {
      mounted = false;
    };
  }, [orgId, page, pageSize]);

  const handleMarkRead = async (alertId: number) => {
    if (orgId === 0) return;
    try {
      await authorizedAxios.post(`/api/inventory/alerts/${orgId}/${alertId}/read`);
      message.success('Marked as read');
      loadAlerts(page, pageSize);
    } catch {
      message.error('Failed to mark as read');
    }
  };

  const handleMarkAllRead = async () => {
    if (orgId === 0) return;
    try {
      const unread = alerts.filter((a: any) => !a.isRead);
      await Promise.all(unread.map((a: any) => authorizedAxios.post(`/api/inventory/alerts/${orgId}/${a.id}/read`)));
      message.success('All marked as read');
      loadAlerts(page, pageSize);
    } catch {
      message.error('Failed to mark all as read');
    }
  };

  const columns = [
    { title: 'Type', dataIndex: 'alertType', key: 'type', width: 120, render: (t: string) => <Tag color={alertTypeColors[t] || alertTypeColors.Default}>{t || 'Alert'}</Tag> },
    { title: 'Message', dataIndex: 'message', key: 'message' },
    { title: 'Stock Item', dataIndex: 'stockItemName', key: 'stockItemName', width: 160, render: (v: string) => v || '—' },
    { title: 'Created At', dataIndex: 'createdAt', key: 'createdAt', width: 180, render: (v: string) => (v ? new Date(v).toLocaleString() : '—') },
    { title: 'Read', dataIndex: 'isRead', key: 'read', width: 90, render: (v: boolean) => <Tag color={v ? 'green' : 'default'}>{v ? 'Yes' : 'No'}</Tag> },
    {
      title: 'Actions',
      key: 'actions',
      width: 180,
      render: (_: any, record: any) => (
        <Space>
          {!record.isRead && (
            <Button size="small" icon={<CheckOutlined />} onClick={() => handleMarkRead(record.id)}>Mark Read</Button>
          )}
          <Button size="small" icon={<CheckCircleOutlined />} onClick={() => handleMarkRead(record.id)}>Dismiss</Button>
        </Space>
      ),
    },
  ];

  if (orgId === 0) return null;

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>Alerts</Title>
        <Button icon={<CheckCircleOutlined />} onClick={handleMarkAllRead}>Mark All Read</Button>
      </div>
      <Table
        columns={columns}
        dataSource={alerts}
        rowKey="id"
        loading={loading}
        pagination={{
          current: page,
          pageSize,
          total,
          showSizeChanger: true,
          showTotal: (t) => `Total ${t} alerts`,
          onChange: (p, ps) => {
            setPage(p);
            setPageSize(ps ?? 20);
          },
        }}
      />
    </div>
  );
};
