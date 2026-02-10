import React, { useEffect, useState } from 'react';
import { Card, Table, Button, Tag, Typography, InputNumber, message, Spin } from 'antd';
import { CheckOutlined } from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

const statusColors: Record<string, string> = {
  InProgress: 'blue',
  Completed: 'orange',
  Approved: 'green',
  Cancelled: 'red',
};

export const StockCountDetail: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [count, setCount] = useState<any>(null);
  const [items, setItems] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);

  const load = () => {
    if (orgId === 0 || !id) return;
    setLoading(true);
    authorizedAxios
      .get(`/api/inventory/stock-counts/${orgId}/${id}`)
      .then((res) => {
        const data = res.data?.data ?? res.data;
        setCount(data);
        setItems(data?.items ?? []);
      })
      .catch(() => message.error('Failed to load stock count'))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    let mounted = true;
    if (orgId !== 0 && id) {
      setLoading(true);
      authorizedAxios
        .get(`/api/inventory/stock-counts/${orgId}/${id}`)
        .then((res) => {
          if (mounted) {
            const data = res.data?.data ?? res.data;
            setCount(data);
            setItems(data?.items ?? []);
          }
        })
        .catch(() => { if (mounted) message.error('Failed to load stock count'); })
        .finally(() => { if (mounted) setLoading(false); });
    }
    return () => { mounted = false; };
  }, [orgId, id]);

  const updateActualQuantity = (itemId: number, value: number) => {
    setItems((prev) => prev.map((it) => (it.id === itemId ? { ...it, actualQuantity: value } : it)));
  };

  const handleSubmitCounts = async () => {
    if (orgId === 0 || !id) return;
    const payload = items.map((it) => ({ id: it.id, stockItemId: it.stockItemId, actualQuantity: it.actualQuantity ?? it.expectedQuantity ?? 0 }));
    setActionLoading(true);
    try {
      await authorizedAxios.put(`/api/inventory/stock-counts/${orgId}/${id}/items`, payload);
      message.success('Counts submitted');
      load();
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to submit');
    } finally {
      setActionLoading(false);
    }
  };

  const handleComplete = async () => {
    if (orgId === 0 || !id) return;
    setActionLoading(true);
    try {
      await authorizedAxios.post(`/api/inventory/stock-counts/${orgId}/${id}/complete`);
      message.success('Count completed');
      load();
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to complete');
    } finally {
      setActionLoading(false);
    }
  };

  const handleApprove = async () => {
    if (orgId === 0 || !id) return;
    setActionLoading(true);
    try {
      await authorizedAxios.post(`/api/inventory/stock-counts/${orgId}/${id}/approve`);
      message.success('Count approved');
      load();
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to approve');
    } finally {
      setActionLoading(false);
    }
  };

  const formatDate = (d: string | null) => (d ? new Date(d).toLocaleDateString() : '–');
  const isInProgress = count?.status === 'InProgress';
  const isCompleted = count?.status === 'Completed';

  const varianceColor = (v: number) => {
    if (v === 0) return 'green';
    if (v < 0) return 'red';
    return 'orange';
  };

  if (orgId === 0) return null;

  if (loading && !count) {
    return <Spin />;
  }

  const itemColumns = [
    { title: 'Stock Item Name', dataIndex: 'stockItemName', key: 'stockItemName', render: (t: string, r: any) => t ?? r.stockItem?.name ?? '–' },
    { title: 'Expected Qty', dataIndex: 'expectedQuantity', key: 'expectedQuantity', width: 110 },
    ...(isInProgress
      ? [
          {
            title: 'Actual Qty',
            key: 'actualQuantity',
            width: 120,
            render: (_: any, record: any) => (
              <InputNumber
                min={0}
                value={record.actualQuantity ?? record.expectedQuantity ?? 0}
                onChange={(v) => updateActualQuantity(record.id, Number(v) ?? 0)}
                style={{ width: '100%' }}
              />
            ),
          },
        ]
      : [{ title: 'Actual Qty', dataIndex: 'actualQuantity', key: 'actualQuantity', width: 100 }]),
    {
      title: 'Variance',
      key: 'variance',
      width: 100,
      render: (_: any, r: any) => {
        const exp = r.expectedQuantity ?? 0;
        const act = r.actualQuantity ?? 0;
        const v = act - exp;
        return <Tag color={varianceColor(v)}>{v >= 0 ? `+${v}` : v}</Tag>;
      },
    },
    {
      title: 'Variance Value (CHF)',
      key: 'varianceValue',
      width: 140,
      render: (_: any, r: any) => {
        const exp = r.expectedQuantity ?? 0;
        const act = r.actualQuantity ?? 0;
        const cost = r.costPrice ?? 0;
        const val = (act - exp) * cost;
        return val != null ? `CHF ${val.toFixed(2)}` : '–';
      },
    },
  ];

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>
          Stock Count – {formatDate(count?.countDate)}
        </Title>
        <div>
          {isInProgress && (
            <>
              <Button type="primary" onClick={handleSubmitCounts} loading={actionLoading} style={{ marginRight: 8 }}>
                Submit Counts
              </Button>
              <Button onClick={handleComplete} loading={actionLoading} icon={<CheckOutlined />}>
                Complete
              </Button>
            </>
          )}
          {isCompleted && (
            <Button type="primary" onClick={handleApprove} loading={actionLoading} icon={<CheckOutlined />}>
              Approve
            </Button>
          )}
        </div>
      </div>

      <Card style={{ marginBottom: 16 }}>
        <Typography.Paragraph><strong>Status:</strong> <Tag color={statusColors[count?.status] ?? 'default'}>{count?.status}</Tag></Typography.Paragraph>
        {count?.notes && <Typography.Paragraph><strong>Notes:</strong> {count.notes}</Typography.Paragraph>}
      </Card>

      <Card title="Items">
        <Table
          dataSource={items}
          rowKey="id"
          size="small"
          pagination={false}
          columns={itemColumns}
        />
      </Card>
    </div>
  );
};
