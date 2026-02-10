import React, { useEffect, useState } from 'react';
import { Form, Input, InputNumber, Select, Switch, Button, Card, Typography, Space, message, Spin } from 'antd';
import { useNavigate, useParams } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;
const { TextArea } = Input;

const unitOptions = [
  { value: 0, label: 'Piece' }, { value: 1, label: 'Kilogram' }, { value: 2, label: 'Gram' },
  { value: 3, label: 'Liter' }, { value: 4, label: 'Milliliter' }, { value: 5, label: 'Bottle' },
  { value: 6, label: 'Box' }, { value: 7, label: 'Pack' }, { value: 8, label: 'Dozen' }, { value: 9, label: 'Portion' },
];

export const StockItemForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = !!id;
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [categories, setCategories] = useState<any[]>([]);
  const [suppliers, setSuppliers] = useState<any[]>([]);
  const [locations, setLocations] = useState<any[]>([]);

  useEffect(() => {
    if (!orgId) return;
    let mounted = true;
    const loadOptions = async () => {
      try {
        const [catRes, supRes, locRes] = await Promise.all([
          authorizedAxios.get(`/api/inventory/categories/${orgId}`),
          authorizedAxios.get(`/api/inventory/suppliers/${orgId}`),
          authorizedAxios.get(`/api/inventory/storage-locations/${orgId}`),
        ]);
        if (mounted) {
          setCategories(Array.isArray(catRes.data?.data) ? catRes.data.data : (Array.isArray(catRes.data) ? catRes.data : []));
          setSuppliers(Array.isArray(supRes.data?.data) ? supRes.data.data : (Array.isArray(supRes.data) ? supRes.data : []));
          setLocations(Array.isArray(locRes.data?.data) ? locRes.data.data : (Array.isArray(locRes.data) ? locRes.data : []));
        }
      } catch { /* ignore */ }
    };
    loadOptions();
    if (isEdit && id) {
      authorizedAxios.get(`/api/inventory/stock-items/${orgId}/${id}`).then(r => {
        if (mounted) {
          const data = r.data?.data ?? r.data;
          if (data) form.setFieldsValue(data);
        }
      }).catch(() => { if (mounted) message.error('Failed to load item'); });
    }
    return () => { mounted = false; };
  }, [orgId, id, isEdit, form]);

  const onFinish = async (values: any) => {
    setLoading(true);
    try {
      if (isEdit) {
        await authorizedAxios.put(`/api/inventory/stock-items/${orgId}/${id}`, values);
        message.success('Stock item updated');
      } else {
        await authorizedAxios.post(`/api/inventory/stock-items/${orgId}`, values);
        message.success('Stock item created');
      }
      navigate('/stock-items');
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to save');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <Title level={3}>{isEdit ? 'Edit Stock Item' : 'New Stock Item'}</Title>
      <Card>
        <Form form={form} layout="vertical" onFinish={onFinish} initialValues={{ baseUnitOfMeasurement: 0, currentQuantity: 0, minimumThreshold: 0, costPrice: 0, isActive: true, isPerishable: false }}>
          <Form.Item name="name" label="Name" rules={[{ required: true, message: 'Name is required' }]}>
            <Input placeholder="e.g., Flour, Tomato Sauce" />
          </Form.Item>
          <Space style={{ width: '100%' }} size="middle">
            <Form.Item name="sku" label="SKU" style={{ flex: 1 }}><Input placeholder="e.g., FL-001" /></Form.Item>
            <Form.Item name="barcode" label="Barcode" style={{ flex: 1 }}><Input placeholder="Scan or enter" /></Form.Item>
          </Space>
          <Space style={{ width: '100%' }} size="middle">
            <Form.Item name="categoryId" label="Category" rules={[{ required: true }]} style={{ flex: 1 }}>
              <Select placeholder="Select category" options={categories.map((c: any) => ({ value: c.id, label: c.name }))} />
            </Form.Item>
            <Form.Item name="baseUnitOfMeasurement" label="Unit" rules={[{ required: true }]} style={{ flex: 1 }}>
              <Select options={unitOptions} />
            </Form.Item>
          </Space>
          <Space style={{ width: '100%' }} size="middle">
            {!isEdit && <Form.Item name="currentQuantity" label="Initial Quantity" style={{ flex: 1 }}><InputNumber style={{ width: '100%' }} min={0} step={0.01} /></Form.Item>}
            <Form.Item name="minimumThreshold" label="Min Threshold" style={{ flex: 1 }}><InputNumber style={{ width: '100%' }} min={0} step={0.01} /></Form.Item>
            <Form.Item name="parLevel" label="PAR Level" style={{ flex: 1 }}><InputNumber style={{ width: '100%' }} min={0} step={0.01} /></Form.Item>
          </Space>
          <Space style={{ width: '100%' }} size="middle">
            <Form.Item name="costPrice" label="Cost Price (CHF)" style={{ flex: 1 }}><InputNumber style={{ width: '100%' }} min={0} step={0.01} /></Form.Item>
            <Form.Item name="maximumCapacity" label="Max Capacity" style={{ flex: 1 }}><InputNumber style={{ width: '100%' }} min={0} step={0.01} /></Form.Item>
          </Space>
          <Space style={{ width: '100%' }} size="middle">
            <Form.Item name="primarySupplierId" label="Primary Supplier" style={{ flex: 1 }}>
              <Select allowClear placeholder="Select supplier" options={suppliers.map((s: any) => ({ value: s.id, label: s.name }))} />
            </Form.Item>
            <Form.Item name="primaryStorageLocationId" label="Storage Location" style={{ flex: 1 }}>
              <Select allowClear placeholder="Select location" options={locations.map((l: any) => ({ value: l.id, label: l.name }))} />
            </Form.Item>
          </Space>
          <Form.Item name="description" label="Description"><TextArea rows={3} placeholder="Optional description" /></Form.Item>
          <Space>
            <Form.Item name="isPerishable" label="Perishable" valuePropName="checked"><Switch /></Form.Item>
            {isEdit && <Form.Item name="isActive" label="Active" valuePropName="checked"><Switch /></Form.Item>}
          </Space>
          <Form.Item name="notes" label="Notes"><TextArea rows={2} placeholder="Optional notes" /></Form.Item>
          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" loading={loading}>Save</Button>
              <Button onClick={() => navigate('/stock-items')}>Cancel</Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};
