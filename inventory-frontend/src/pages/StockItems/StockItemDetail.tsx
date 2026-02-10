import React, { useEffect, useState } from 'react';
import { Tabs, Card, Descriptions, Tag, Button, Space, Typography, Table, Form, Radio, Input, InputNumber, Select, message, Spin, Popconfirm, Empty } from 'antd';
import { EditOutlined, DeleteOutlined } from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;
const { TextArea } = Input;

const statusColors: Record<string, string> = {
  InStock: 'green', BelowPar: 'gold', Low: 'orange', OutOfStock: 'red', Overstock: 'blue',
};

const unitLabels: Record<number, string> = {
  0: 'pcs', 1: 'kg', 2: 'g', 3: 'L', 4: 'mL', 5: 'btl', 6: 'box', 7: 'pack', 8: 'doz', 9: 'portion',
};

const movementTypeLabels: Record<number, string> = {
  0: 'Received', 1: 'Sold', 2: 'Adjusted', 3: 'Wasted', 4: 'Transferred', 5: 'Returned', 6: 'Count Correction', 7: 'Expired',
};

const adjustmentReasons = [
  { value: 'Received', label: 'Received' },
  { value: 'Damaged', label: 'Damaged' },
  { value: 'Expired', label: 'Expired' },
  { value: 'Theft', label: 'Theft' },
  { value: 'Correction', label: 'Correction' },
  { value: 'Transfer', label: 'Transfer' },
  { value: 'Returned', label: 'Returned' },
  { value: 'Over-portioning', label: 'Over-portioning' },
];

export const StockItemDetail: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [item, setItem] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [movements, setMovements] = useState<any[]>([]);
  const [movementsTotal, setMovementsTotal] = useState(0);
  const [movementsPage, setMovementsPage] = useState(1);
  const [movementsPageSize, setMovementsPageSize] = useState(10);
  const [movementsLoading, setMovementsLoading] = useState(false);
  const [adjustLoading, setAdjustLoading] = useState(false);
  const [adjustForm] = Form.useForm();

  const loadItem = () => {
    if (orgId === 0 || !id) return;
    setLoading(true);
    authorizedAxios
      .get(`/api/inventory/stock-items/${orgId}/${id}`)
      .then((res) => setItem(res.data?.data ?? res.data))
      .catch(() => message.error('Failed to load stock item'))
      .finally(() => setLoading(false));
  };

  useEffect(() => {
    let mounted = true;
    if (orgId !== 0 && id) {
      setLoading(true);
      authorizedAxios
        .get(`/api/inventory/stock-items/${orgId}/${id}`)
        .then((res) => { if (mounted) setItem(res.data?.data ?? res.data); })
        .catch(() => { if (mounted) message.error('Failed to load stock item'); })
        .finally(() => { if (mounted) setLoading(false); });
    }
    return () => { mounted = false; };
  }, [orgId, id]);

  const loadMovements = () => {
    if (orgId === 0 || !id) return;
    setMovementsLoading(true);
    authorizedAxios
      .get(`/api/inventory/stock-items/${orgId}/${id}/movements`, {
        params: { page: movementsPage, pageSize: movementsPageSize },
      })
      .then((res) => {
        const data = res.data?.data ?? res.data;
        const list = Array.isArray(data?.items) ? data.items : (Array.isArray(data) ? data : []);
        const total = data?.totalCount ?? data?.total ?? list.length;
        setMovements(list);
        setMovementsTotal(total);
      })
      .catch(() => message.error('Failed to load movements'))
      .finally(() => setMovementsLoading(false));
  };

  useEffect(() => {
    let mounted = true;
    if (orgId !== 0 && id) {
      setMovementsLoading(true);
      authorizedAxios
        .get(`/api/inventory/stock-items/${orgId}/${id}/movements`, {
          params: { page: movementsPage, pageSize: movementsPageSize },
        })
        .then((res) => {
          if (!mounted) return;
          const data = res.data?.data ?? res.data;
          const list = Array.isArray(data?.items) ? data.items : (Array.isArray(data) ? data : []);
          const total = data?.totalCount ?? data?.total ?? list.length;
          setMovements(list);
          setMovementsTotal(total);
        })
        .catch(() => { if (mounted) message.error('Failed to load movements'); })
        .finally(() => { if (mounted) setMovementsLoading(false); });
    }
    return () => { mounted = false; };
  }, [orgId, id, movementsPage, movementsPageSize]);

  const handleDelete = async () => {
    if (orgId === 0 || !id) return;
    try {
      await authorizedAxios.delete(`/api/inventory/stock-items/${orgId}/${id}`);
      message.success('Stock item deleted');
      navigate('/stock-items');
    } catch {
      message.error('Failed to delete');
    }
  };

  const handleAdjust = async (values: { direction: 'increase' | 'decrease'; quantity: number; reason: string; notes?: string }) => {
    if (orgId === 0 || !id) return;
    const qty = values.direction === 'decrease' ? -Math.abs(values.quantity) : Math.abs(values.quantity);
    setAdjustLoading(true);
    try {
      await authorizedAxios.post(`/api/inventory/stock-items/${orgId}/${id}/adjust`, {
        quantity: qty,
        reason: values.reason,
        notes: values.notes,
      });
      message.success('Stock adjusted');
      adjustForm.resetFields();
      loadItem();
      loadMovements();
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to adjust');
    } finally {
      setAdjustLoading(false);
    }
  };

  if (loading && !item) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', padding: 48 }}>
        <Spin size="large" />
      </div>
    );
  }

  if (!item) {
    return (
      <div>
        <Title level={3}>Stock Item</Title>
        <Empty description="Item not found" />
      </div>
    );
  }

  const qty = item.currentQuantity ?? 0;
  const qtyColor = item.stockStatus === 'OutOfStock' ? '#ff4d4f' : item.stockStatus === 'Low' || item.stockStatus === 'BelowPar' ? '#faad14' : '#52c41a';

  const movementColumns = [
    { title: 'Date', dataIndex: 'createdAt', key: 'date', width: 120, render: (d: string) => (d ? new Date(d).toLocaleString() : '–') },
    { title: 'Type', dataIndex: 'movementType', key: 'type', width: 120, render: (t: number) => <Tag color="blue">{movementTypeLabels[t] ?? 'Unknown'}</Tag> },
    { title: 'Quantity', dataIndex: 'quantityChange', key: 'qty', width: 100, render: (v: number) => <span style={{ color: v >= 0 ? '#52c41a' : '#ff4d4f' }}>{v >= 0 ? '+' : ''}{v}</span> },
    { title: 'Previous Qty', dataIndex: 'previousQuantity', key: 'prev', width: 100, render: (v: number) => v?.toFixed(2) ?? '–' },
    { title: 'New Qty', dataIndex: 'newQuantity', key: 'new', width: 100, render: (v: number) => v?.toFixed(2) ?? '–' },
    { title: 'Reason', dataIndex: 'reason', key: 'reason' },
    { title: 'Created By', dataIndex: 'createdBy', key: 'createdBy', width: 120 },
  ];

  const tabItems = [
    {
      key: 'overview',
      label: 'Overview',
      children: (
        <Card>
          <Space style={{ marginBottom: 16 }}>
            <Button type="primary" icon={<EditOutlined />} onClick={() => navigate(`/stock-items/${id}/edit`)}>Edit</Button>
            <Popconfirm title="Delete this stock item?" onConfirm={handleDelete}>
              <Button danger icon={<DeleteOutlined />}>Delete</Button>
            </Popconfirm>
          </Space>
          <Descriptions column={1} bordered size="small">
            <Descriptions.Item label="Name">{item.name}</Descriptions.Item>
            <Descriptions.Item label="SKU">{item.sku ?? '–'}</Descriptions.Item>
            <Descriptions.Item label="Category">{item.categoryName ?? '–'}</Descriptions.Item>
            <Descriptions.Item label="Unit">{unitLabels[item.baseUnitOfMeasurement] ?? 'pcs'}</Descriptions.Item>
            <Descriptions.Item label="Status"><Tag color={statusColors[item.stockStatus] || 'default'}>{item.stockStatus ?? '–'}</Tag></Descriptions.Item>
            <Descriptions.Item label="Cost Price">CHF {(item.costPrice ?? 0).toFixed(2)}</Descriptions.Item>
            <Descriptions.Item label="Avg Cost Price">CHF {(item.averageCostPrice ?? item.costPrice ?? 0).toFixed(2)}</Descriptions.Item>
            <Descriptions.Item label="Quantity"><span style={{ color: qtyColor, fontWeight: 600 }}>{(qty).toFixed(2)}</span></Descriptions.Item>
            {item.parLevel != null && <Descriptions.Item label="PAR Level">{item.parLevel}</Descriptions.Item>}
            {item.minimumThreshold != null && <Descriptions.Item label="Min Threshold">{item.minimumThreshold}</Descriptions.Item>}
            {item.maximumCapacity != null && <Descriptions.Item label="Max Capacity">{item.maximumCapacity}</Descriptions.Item>}
            <Descriptions.Item label="Primary Supplier">{item.primarySupplierName ?? '–'}</Descriptions.Item>
            <Descriptions.Item label="Primary Storage Location">{item.primaryStorageLocationName ?? '–'}</Descriptions.Item>
            <Descriptions.Item label="">
              <Space>
                {item.isPerishable && <Tag>Perishable</Tag>}
                {item.isActive !== false && <Tag color="green">Active</Tag>}
                {item.isActive === false && <Tag color="default">Inactive</Tag>}
              </Space>
            </Descriptions.Item>
            <Descriptions.Item label="Description">{item.description || '–'}</Descriptions.Item>
            <Descriptions.Item label="Notes">{item.notes || '–'}</Descriptions.Item>
          </Descriptions>
        </Card>
      ),
    },
    {
      key: 'movements',
      label: 'Stock Movements',
      children: (
        <Table
          columns={movementColumns}
          dataSource={movements}
          rowKey="id"
          loading={movementsLoading}
          pagination={{
            current: movementsPage,
            pageSize: movementsPageSize,
            total: movementsTotal,
            showSizeChanger: true,
            showTotal: (t) => `Total ${t} movements`,
            onChange: (p, ps) => { setMovementsPage(p); setMovementsPageSize(ps ?? 10); },
          }}
        />
      ),
    },
    {
      key: 'adjust',
      label: 'Adjust Stock',
      children: (
        <Card style={{ maxWidth: 560 }}>
          <Form form={adjustForm} layout="vertical" onFinish={handleAdjust} initialValues={{ direction: 'increase', quantity: 1, reason: 'Correction' }}>
            <Form.Item name="direction" label="Adjustment Type" rules={[{ required: true }]}>
              <Radio.Group options={[{ value: 'increase', label: 'Increase' }, { value: 'decrease', label: 'Decrease' }]} />
            </Form.Item>
            <Form.Item name="quantity" label="Quantity" rules={[{ required: true, message: 'Required' }]}>
              <InputNumber min={0.01} step={0.01} style={{ width: '100%' }} />
            </Form.Item>
            <Form.Item name="reason" label="Reason" rules={[{ required: true }]}>
              <Select options={adjustmentReasons} placeholder="Select reason" />
            </Form.Item>
            <Form.Item name="notes" label="Notes">
              <TextArea rows={2} placeholder="Optional notes" />
            </Form.Item>
            <Form.Item shouldUpdate={(prev, curr) => prev.direction !== curr.direction || prev.quantity !== curr.quantity}>
              {({ getFieldValue }) => {
                const dir = getFieldValue('direction');
                const q = getFieldValue('quantity') ?? 0;
                const newQty = dir === 'increase' ? qty + q : Math.max(0, qty - q);
                return <Typography.Text type="secondary">Current: {qty.toFixed(2)} → New: {newQty.toFixed(2)}</Typography.Text>;
              }}
            </Form.Item>
            <Form.Item>
              <Button type="primary" htmlType="submit" loading={adjustLoading}>Submit</Button>
            </Form.Item>
          </Form>
        </Card>
      ),
    },
  ];

  return (
    <div>
      <Title level={3}>{item.name}</Title>
      <Tabs items={tabItems} />
    </div>
  );
};
