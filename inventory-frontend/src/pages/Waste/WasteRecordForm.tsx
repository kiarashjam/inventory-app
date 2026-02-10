import React, { useEffect, useState } from 'react';
import { Form, Input, InputNumber, Select, Button, Card, Typography, Space, message } from 'antd';
import { useNavigate } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;
const { TextArea } = Input;

const wasteReasonOptions = [
  { value: 0, label: 'Spoilage' },
  { value: 1, label: 'Overproduction' },
  { value: 2, label: 'Customer Return' },
  { value: 3, label: 'Preparation' },
  { value: 4, label: 'Expired' },
  { value: 5, label: 'Damaged' },
  { value: 6, label: 'Other' },
];

export const WasteRecordForm: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [stockItems, setStockItems] = useState<any[]>([]);
  const [costPreview, setCostPreview] = useState<number | null>(null);

  useEffect(() => {
    let mounted = true;
    if (orgId === 0) return;
    authorizedAxios
      .get(`/api/inventory/stock-items/${orgId}`, { params: { pageSize: 500 } })
      .then((r) => {
        if (mounted) {
          const body = r.data?.data ?? r.data;
          const items = Array.isArray(body?.items) ? body.items : (Array.isArray(body) ? body : []);
          setStockItems(items);
        }
      })
      .catch(() => {});
    return () => { mounted = false; };
  }, [orgId]);

  const quantity = Form.useWatch('quantity', form);
  const stockItemId = Form.useWatch('stockItemId', form);

  useEffect(() => {
    if (stockItemId != null && quantity != null && quantity >= 0) {
      const item = stockItems.find((s: any) => s.id === stockItemId);
      const cost = item?.costPrice != null ? Number(quantity) * Number(item.costPrice) : null;
      setCostPreview(cost);
    } else {
      setCostPreview(null);
    }
  }, [stockItemId, quantity, stockItems]);

  const onFinish = async (values: any) => {
    if (orgId === 0) return;
    setLoading(true);
    try {
      await authorizedAxios.post(`/api/inventory/waste/${orgId}`, {
        stockItemId: values.stockItemId,
        quantity: values.quantity,
        wasteReason: values.wasteReason,
        notes: values.notes,
      });
      message.success('Waste recorded');
      navigate('/waste');
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to save');
    } finally {
      setLoading(false);
    }
  };

  if (orgId === 0) return null;

  return (
    <div>
      <Title level={3}>Record Waste</Title>
      <Card>
        <Form form={form} layout="vertical" onFinish={onFinish}>
          <Form.Item name="stockItemId" label="Stock Item" rules={[{ required: true, message: 'Stock item is required' }]}>
            <Select
              placeholder="Select stock item"
              options={stockItems.map((s: any) => ({ value: s.id, label: s.name }))}
              showSearch
              optionFilterProp="label"
            />
          </Form.Item>
          <Form.Item
            name="quantity"
            label="Quantity"
            rules={[{ required: true, message: 'Quantity is required' }, { type: 'number', min: 0.01, message: 'Min 0.01' }]}
          >
            <InputNumber style={{ width: '100%' }} min={0.01} step={0.01} placeholder="Quantity" />
          </Form.Item>
          {costPreview != null && (
            <Typography.Text type="secondary">Estimated cost: CHF {costPreview.toFixed(2)}</Typography.Text>
          )}
          <Form.Item name="wasteReason" label="Waste Reason" rules={[{ required: true, message: 'Waste reason is required' }]}>
            <Select placeholder="Select reason" options={wasteReasonOptions} />
          </Form.Item>
          <Form.Item name="notes" label="Notes">
            <TextArea rows={3} placeholder="Optional notes" />
          </Form.Item>
          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" loading={loading}>Save</Button>
              <Button onClick={() => navigate('/waste')}>Cancel</Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};
