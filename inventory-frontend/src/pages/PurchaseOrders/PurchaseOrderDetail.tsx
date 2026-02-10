import React, { useEffect, useState } from 'react';
import { Card, Table, Button, Space, Tag, Typography, Descriptions, Modal, InputNumber, message, Spin } from 'antd';
import { EditOutlined, DeleteOutlined, CheckOutlined, StopOutlined, InboxOutlined } from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';
import { Popconfirm } from 'antd';

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

export const PurchaseOrderDetail: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [po, setPo] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);
  const [receiveVisible, setReceiveVisible] = useState(false);
  const [receiveRows, setReceiveRows] = useState<{ id?: number; stockItemName?: string; orderedQuantity?: number; receivedQuantity?: number; acceptedQuantity: number; rejectedQuantity: number }[]>([]);

  const load = () => {
    if (orgId === 0 || !id) return;
    setLoading(true);
    authorizedAxios
      .get(`/api/inventory/purchase-orders/${orgId}/${id}`)
      .then((res) => {
        const data = res.data?.data ?? res.data;
        setPo(data);
      })
      .catch(() => message.error('Failed to load purchase order'))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    let mounted = true;
    if (orgId !== 0 && id) {
      setLoading(true);
      authorizedAxios
        .get(`/api/inventory/purchase-orders/${orgId}/${id}`)
        .then((res) => {
          if (mounted) setPo(res.data?.data ?? res.data);
        })
        .catch(() => { if (mounted) message.error('Failed to load purchase order'); })
        .finally(() => { if (mounted) setLoading(false); });
    }
    return () => { mounted = false; };
  }, [orgId, id]);

  const formatDate = (d: string | null) => (d ? new Date(d).toLocaleDateString() : '–');

  const handleSubmit = async () => {
    if (orgId === 0 || !id) return;
    setActionLoading(true);
    try {
      await authorizedAxios.post(`/api/inventory/purchase-orders/${orgId}/${id}/submit`);
      message.success('Order submitted');
      load();
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to submit');
    } finally {
      setActionLoading(false);
    }
  };

  const handleCancel = async () => {
    if (orgId === 0 || !id) return;
    setActionLoading(true);
    try {
      await authorizedAxios.post(`/api/inventory/purchase-orders/${orgId}/${id}/cancel`);
      message.success('Order cancelled');
      load();
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to cancel');
    } finally {
      setActionLoading(false);
    }
  };

  const handleDelete = async () => {
    if (orgId === 0 || !id) return;
    setActionLoading(true);
    try {
      await authorizedAxios.delete(`/api/inventory/purchase-orders/${orgId}/${id}`);
      message.success('Order deleted');
      navigate('/purchase-orders');
    } catch {
      message.error('Failed to delete');
    } finally {
      setActionLoading(false);
    }
  };

  const openReceive = () => {
    const items = po?.items ?? po?.lineItems ?? [];
    setReceiveRows(
      items.map((it: any) => ({
        id: it.id,
        stockItemName: it.stockItemName ?? it.stockItem?.name,
        orderedQuantity: it.orderedQuantity,
        receivedQuantity: it.receivedQuantity ?? 0,
        acceptedQuantity: it.orderedQuantity - (it.receivedQuantity ?? 0),
        rejectedQuantity: 0,
      }))
    );
    setReceiveVisible(true);
  };

  const handleReceive = async () => {
    if (orgId === 0 || !id) return;
    const payload = receiveRows.map((r) => ({
      lineItemId: r.id,
      acceptedQuantity: r.acceptedQuantity,
      rejectedQuantity: r.rejectedQuantity,
    }));
    setActionLoading(true);
    try {
      await authorizedAxios.post(`/api/inventory/purchase-orders/${orgId}/${id}/receive`, payload);
      message.success('Receipt recorded');
      setReceiveVisible(false);
      load();
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to receive');
    } finally {
      setActionLoading(false);
    }
  };

  const updateReceiveRow = (index: number, field: 'acceptedQuantity' | 'rejectedQuantity', value: number) => {
    setReceiveRows((prev) => {
      const next = [...prev];
      next[index] = { ...next[index], [field]: value };
      return next;
    });
  };

  const canEdit = po?.status === 'Draft';
  const canSubmit = po?.status === 'Draft';
  const canReceive = po?.status === 'Submitted' || po?.status === 'Confirmed' || po?.status === 'PartiallyReceived';
  const canCancel = po && !['Cancelled', 'Closed'].includes(po.status);

  if (orgId === 0) return null;

  if (loading || !po) {
    return <Spin />;
  }

  const items = po.items ?? po.lineItems ?? [];

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 16 }}>
        <Title level={3} style={{ margin: 0 }}>
          Purchase Order {po.orderNumber ?? po.id}
        </Title>
        <Space>
          {canEdit && (
            <>
              <Button icon={<EditOutlined />} onClick={() => navigate(`/purchase-orders/${id}/edit`)}>Edit</Button>
              <Button type="primary" icon={<CheckOutlined />} onClick={handleSubmit} loading={actionLoading}>Submit</Button>
              <Popconfirm title="Delete this purchase order?" onConfirm={handleDelete}>
                <Button danger icon={<DeleteOutlined />} loading={actionLoading}>Delete</Button>
              </Popconfirm>
            </>
          )}
          {canReceive && (
            <Button type="primary" icon={<InboxOutlined />} onClick={openReceive} loading={actionLoading}>Receive Goods</Button>
          )}
          {canCancel && !canEdit && (
            <Popconfirm title="Cancel this order?" onConfirm={handleCancel}>
              <Button danger icon={<StopOutlined />} loading={actionLoading}>Cancel</Button>
            </Popconfirm>
          )}
        </Space>
      </div>

      <Card style={{ marginBottom: 16 }}>
        <Descriptions column={2} size="small">
          <Descriptions.Item label="Order #">{po.orderNumber ?? po.id}</Descriptions.Item>
          <Descriptions.Item label="Supplier">{po.supplierName ?? po.supplier?.name ?? '–'}</Descriptions.Item>
          <Descriptions.Item label="Status">
            <Tag color={statusColors[po.status] ?? 'default'}>{po.status}</Tag>
          </Descriptions.Item>
          <Descriptions.Item label="Order Date">{formatDate(po.orderDate)}</Descriptions.Item>
          <Descriptions.Item label="Expected Delivery">{formatDate(po.expectedDeliveryDate)}</Descriptions.Item>
          <Descriptions.Item label="Actual Delivery">{formatDate(po.actualDeliveryDate)}</Descriptions.Item>
          <Descriptions.Item label="Total Amount">
            {po.totalAmount != null ? `CHF ${Number(po.totalAmount).toFixed(2)}` : '–'}
          </Descriptions.Item>
        </Descriptions>
      </Card>

      <Card title="Items">
        <Table
          dataSource={items}
          rowKey="id"
          size="small"
          pagination={false}
          columns={[
            { title: 'Stock Item', dataIndex: 'stockItemName', key: 'stockItemName', render: (t: string, r: any) => t ?? r.stockItem?.name ?? '–' },
            { title: 'Ordered Qty', dataIndex: 'orderedQuantity', key: 'orderedQuantity', width: 100 },
            { title: 'Received Qty', dataIndex: 'receivedQuantity', key: 'receivedQuantity', width: 100 },
            { title: 'Unit Price (CHF)', dataIndex: 'unitPrice', key: 'unitPrice', width: 120, render: (v: number) => (v != null ? Number(v).toFixed(2) : '–') },
            { title: 'Total (CHF)', key: 'total', width: 100, render: (_: any, r: any) => ((r.orderedQuantity ?? 0) * (r.unitPrice ?? 0)).toFixed(2) },
          ]}
        />
      </Card>

      <Modal
        title="Receive Goods"
        open={receiveVisible}
        onCancel={() => setReceiveVisible(false)}
        onOk={handleReceive}
        okText="Submit Receipt"
        width={640}
        confirmLoading={actionLoading}
      >
        <Table
          dataSource={receiveRows}
          rowKey={(r, i) => r.id?.toString() ?? String(i)}
          size="small"
          pagination={false}
          columns={[
            { title: 'Stock Item', dataIndex: 'stockItemName', key: 'stockItemName' },
            { title: 'Ordered', dataIndex: 'orderedQuantity', key: 'orderedQuantity', width: 80 },
            {
              title: 'Accepted Qty',
              key: 'acceptedQuantity',
              width: 120,
              render: (_, record, index) => (
                <InputNumber
                  min={0}
                  value={record.acceptedQuantity}
                  onChange={(v) => updateReceiveRow(index, 'acceptedQuantity', Number(v) ?? 0)}
                  style={{ width: '100%' }}
                />
              ),
            },
            {
              title: 'Rejected Qty',
              key: 'rejectedQuantity',
              width: 120,
              render: (_, record, index) => (
                <InputNumber
                  min={0}
                  value={record.rejectedQuantity}
                  onChange={(v) => updateReceiveRow(index, 'rejectedQuantity', Number(v) ?? 0)}
                  style={{ width: '100%' }}
                />
              ),
            },
          ]}
        />
      </Modal>
    </div>
  );
};
