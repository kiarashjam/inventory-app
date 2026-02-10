import React, { useEffect, useState } from 'react';
import { Form, Input, InputNumber, Switch, Button, Card, Typography, Space, message } from 'antd';
import { useNavigate, useParams } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;
const { TextArea } = Input;

export const SupplierForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = !!id;
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (orgId === 0 || !isEdit || !id) return;
    let mounted = true;
    authorizedAxios
      .get(`/api/inventory/suppliers/${orgId}/${id}`)
      .then((r) => {
        if (mounted) {
          const data = r.data?.data ?? r.data;
          if (data) form.setFieldsValue(data);
        }
      })
      .catch(() => {
        if (mounted) message.error('Failed to load supplier');
      });
    return () => {
      mounted = false;
    };
  }, [orgId, id, isEdit, form]);

  const onFinish = async (values: any) => {
    if (orgId === 0) return;
    setLoading(true);
    try {
      if (isEdit) {
        await authorizedAxios.put(`/api/inventory/suppliers/${orgId}/${id}`, values);
        message.success('Supplier updated');
      } else {
        await authorizedAxios.post(`/api/inventory/suppliers/${orgId}`, values);
        message.success('Supplier created');
      }
      navigate('/suppliers');
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to save');
    } finally {
      setLoading(false);
    }
  };

  if (orgId === 0) return null;

  return (
    <div>
      <Title level={3}>{isEdit ? 'Edit Supplier' : 'New Supplier'}</Title>
      <Card>
        <Form form={form} layout="vertical" onFinish={onFinish} initialValues={{ leadTimeDays: 0, minimumOrderAmount: 0, isActive: true }}>
          <Form.Item name="name" label="Name" rules={[{ required: true, message: 'Name is required' }]}>
            <Input placeholder="Supplier name" />
          </Form.Item>
          <Space style={{ width: '100%' }} size="middle">
            <Form.Item name="contactPerson" label="Contact Person" style={{ flex: 1 }}><Input placeholder="Contact name" /></Form.Item>
            <Form.Item name="email" label="Email" style={{ flex: 1 }}><Input type="email" placeholder="email@example.com" /></Form.Item>
            <Form.Item name="phone" label="Phone" style={{ flex: 1 }}><Input placeholder="Phone" /></Form.Item>
          </Space>
          <Form.Item name="address" label="Address"><Input placeholder="Street address" /></Form.Item>
          <Space style={{ width: '100%' }} size="middle">
            <Form.Item name="city" label="City" style={{ flex: 1 }}><Input placeholder="City" /></Form.Item>
            <Form.Item name="postalCode" label="Postal Code" style={{ flex: 1 }}><Input placeholder="Postal code" /></Form.Item>
            <Form.Item name="country" label="Country" style={{ flex: 1 }}><Input placeholder="Country" /></Form.Item>
          </Space>
          <Space style={{ width: '100%' }} size="middle">
            <Form.Item name="paymentTerms" label="Payment Terms" style={{ flex: 1 }}><Input placeholder="e.g. Net 30" /></Form.Item>
            <Form.Item name="leadTimeDays" label="Lead Time (days)" style={{ flex: 1 }}><InputNumber style={{ width: '100%' }} min={0} /></Form.Item>
            <Form.Item name="minimumOrderAmount" label="Minimum Order (CHF)" style={{ flex: 1 }}><InputNumber style={{ width: '100%' }} min={0} step={0.01} /></Form.Item>
          </Space>
          <Form.Item name="notes" label="Notes"><TextArea rows={3} placeholder="Optional notes" /></Form.Item>
          {isEdit && (
            <Form.Item name="isActive" label="Active" valuePropName="checked">
              <Switch />
            </Form.Item>
          )}
          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" loading={loading}>Save</Button>
              <Button onClick={() => navigate('/suppliers')}>Cancel</Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};
