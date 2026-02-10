import React, { useEffect, useState } from 'react';
import { Form, Input, InputNumber, Select, Button, Card, Typography, Space, Table, message, Spin, DatePicker } from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';
import dayjs from 'dayjs';

const { Title } = Typography;
const { TextArea } = Input;

interface LineItem {
  key: string;
  stockItemId?: number;
  stockItemName?: string;
  orderedQuantity: number;
  unitPrice: number;
  totalPrice: number;
}

export const PurchaseOrderForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = !!id;
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [suppliers, setSuppliers] = useState<any[]>([]);
  const [stockItems, setStockItems] = useState<any[]>([]);
  const [lineItems, setLineItems] = useState<LineItem[]>([]);
  const [initialLoading, setInitialLoading] = useState(isEdit);

  const loadOptions = () => {
    if (orgId === 0) return;
    authorizedAxios.get(`/api/inventory/suppliers/${orgId}`).then((r) => {
      const data = r.data?.data ?? r.data;
      setSuppliers(Array.isArray(data) ? data : []);
    }).catch(() => {});
    authorizedAxios.get(`/api/inventory/stock-items/${orgId}`, { params: { pageSize: 500 } }).then((r) => {
      const body = r.data?.data ?? r.data;
      const items = Array.isArray(body?.items) ? body.items : (Array.isArray(body) ? body : []);
      setStockItems(items);
    }).catch(() => {});
  };

  useEffect(() => {
    let mounted = true;
    if (orgId === 0) return;
    loadOptions();
    if (isEdit && id) {
      setInitialLoading(true);
      authorizedAxios
        .get(`/api/inventory/purchase-orders/${orgId}/${id}`)
        .then((r) => {
          if (mounted) {
            const data = r.data?.data ?? r.data;
            if (data) {
              form.setFieldsValue({
                supplierId: data.supplierId,
                expectedDeliveryDate: data.expectedDeliveryDate ? dayjs(data.expectedDeliveryDate) : undefined,
                notes: data.notes,
              });
              const items = (data.items ?? data.lineItems ?? []).map((it: any, idx: number) => ({
                key: it.id?.toString() ?? `row-${idx}`,
                stockItemId: it.stockItemId ?? it.stockItem?.id,
                stockItemName: it.stockItemName ?? it.stockItem?.name,
                orderedQuantity: it.orderedQuantity ?? 0,
                unitPrice: it.unitPrice ?? 0,
                totalPrice: (it.orderedQuantity ?? 0) * (it.unitPrice ?? 0),
              }));
              setLineItems(items);
            }
          }
        })
        .catch(() => { if (mounted) message.error('Failed to load purchase order'); })
        .finally(() => { if (mounted) setInitialLoading(false); });
    }
    return () => { mounted = false; };
  }, [orgId, id, isEdit, form]);

  const addRow = () => {
    setLineItems((prev) => [...prev, { key: `new-${Date.now()}`, orderedQuantity: 0, unitPrice: 0, totalPrice: 0 }]);
  };

  const removeRow = (key: string) => {
    setLineItems((prev) => prev.filter((r) => r.key !== key));
  };

  const updateRow = (key: string, field: keyof LineItem, value: number) => {
    setLineItems((prev) =>
      prev.map((r) => {
        if (r.key !== key) return r;
        const next = { ...r, [field]: value };
        if (field === 'orderedQuantity' || field === 'unitPrice') {
          next.totalPrice = (field === 'orderedQuantity' ? value : r.orderedQuantity) * (field === 'unitPrice' ? value : r.unitPrice);
        }
        return next;
      })
    );
  };

  const totalAmount = lineItems.reduce((sum, r) => sum + (r.totalPrice || 0), 0);

  const onFinish = async (values: any) => {
    if (orgId === 0) return;
    const payload = {
      supplierId: values.supplierId,
      expectedDeliveryDate: values.expectedDeliveryDate ? dayjs(values.expectedDeliveryDate).format('YYYY-MM-DD') : undefined,
      notes: values.notes,
      items: lineItems
        .filter((r) => r.stockItemId && (r.orderedQuantity ?? 0) > 0)
        .map((r) => ({ stockItemId: r.stockItemId, orderedQuantity: r.orderedQuantity, unitPrice: r.unitPrice })),
    };
    if (payload.items.length === 0) {
      message.warning('Add at least one line item with quantity > 0');
      return;
    }
    setLoading(true);
    try {
      if (isEdit) {
        await authorizedAxios.put(`/api/inventory/purchase-orders/${orgId}/${id}`, payload);
        message.success('Purchase order updated');
      } else {
        await authorizedAxios.post(`/api/inventory/purchase-orders/${orgId}`, payload);
        message.success('Purchase order created');
      }
      navigate('/purchase-orders');
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to save');
    } finally {
      setLoading(false);
    }
  };

  if (orgId === 0) return null;

  return (
    <div>
      <Title level={3}>{isEdit ? 'Edit Purchase Order' : 'New Purchase Order'}</Title>
      <Card>
        {initialLoading ? (
          <Spin />
        ) : (
          <Form form={form} layout="vertical" onFinish={onFinish}>
            <Space style={{ width: '100%' }} size="middle" wrap>
              <Form.Item name="supplierId" label="Supplier" rules={[{ required: true, message: 'Supplier is required' }]} style={{ minWidth: 260 }}>
                <Select
                  placeholder="Select supplier"
                  options={suppliers.map((s: any) => ({ value: s.id, label: s.name }))}
                  showSearch
                  optionFilterProp="label"
                />
              </Form.Item>
              <Form.Item name="expectedDeliveryDate" label="Expected Delivery Date" style={{ minWidth: 200 }}>
                <DatePicker style={{ width: '100%' }} />
              </Form.Item>
            </Space>
            <Form.Item name="notes" label="Notes">
              <TextArea rows={2} placeholder="Optional notes" />
            </Form.Item>

            <Typography.Text strong style={{ display: 'block', marginBottom: 8 }}>Line Items</Typography.Text>
            <Table
              dataSource={lineItems}
              pagination={false}
              size="small"
              columns={[
                {
                  title: 'Stock Item',
                  key: 'stockItem',
                  width: 260,
                  render: (_, record) => (
                    <Select
                      placeholder="Select item"
                      style={{ width: '100%' }}
                      options={stockItems.map((s: any) => ({ value: s.id, label: s.name }))}
                      value={record.stockItemId}
                      onChange={(v) => {
                        const item = stockItems.find((s: any) => s.id === v);
                        updateRow(record.key, 'stockItemId', v);
                        if (item) setLineItems((prev) => prev.map((r) => (r.key === record.key ? { ...r, stockItemId: v, stockItemName: item.name } : r)));
                      }}
                      showSearch
                      optionFilterProp="label"
                    />
                  ),
                },
                {
                  title: 'Ordered Qty',
                  key: 'orderedQuantity',
                  width: 120,
                  render: (_, record) => (
                    <InputNumber
                      min={0}
                      step={1}
                      value={record.orderedQuantity}
                      onChange={(v) => updateRow(record.key, 'orderedQuantity', Number(v) ?? 0)}
                      style={{ width: '100%' }}
                    />
                  ),
                },
                {
                  title: 'Unit Price (CHF)',
                  key: 'unitPrice',
                  width: 130,
                  render: (_, record) => (
                    <InputNumber
                      min={0}
                      step={0.01}
                      value={record.unitPrice}
                      onChange={(v) => updateRow(record.key, 'unitPrice', Number(v) ?? 0)}
                      style={{ width: '100%' }}
                    />
                  ),
                },
                { title: 'Total (CHF)', key: 'total', width: 100, render: (_, r) => r.totalPrice.toFixed(2) },
                {
                  title: '',
                  key: 'remove',
                  width: 56,
                  render: (_, record) => (
                    <Button type="text" danger icon={<DeleteOutlined />} onClick={() => removeRow(record.key)} />
                  ),
                },
              ]}
              rowKey="key"
            />
            <Button type="dashed" onClick={addRow} icon={<PlusOutlined />} style={{ marginTop: 8 }}>
              Add Item
            </Button>

            <div style={{ marginTop: 16, marginBottom: 16 }}>
              <Typography.Text strong>Running total: CHF {totalAmount.toFixed(2)}</Typography.Text>
            </div>

            <Form.Item>
              <Space>
                <Button type="primary" htmlType="submit" loading={loading}>Save as Draft</Button>
                <Button onClick={() => navigate('/purchase-orders')}>Cancel</Button>
              </Space>
            </Form.Item>
          </Form>
        )}
      </Card>
    </div>
  );
};
