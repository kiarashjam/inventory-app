import React, { useEffect, useState } from 'react';
import { Form, Input, InputNumber, Switch, Button, Card, Typography, Space, message, Spin } from 'antd';
import { useNavigate, useParams } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;

export const MenuItemForm: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEdit = !!id;
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    let mounted = true;
    if (isEdit && id && orgId !== 0) {
      authorizedAxios
        .get(`/api/inventory/menu-items/${orgId}/${id}`)
        .then((r) => {
          if (mounted) {
            const data = r.data?.data ?? r.data;
            if (data) form.setFieldsValue(data);
          }
        })
        .catch(() => { if (mounted) message.error('Failed to load menu item'); });
    }
    return () => { mounted = false; };
  }, [orgId, id, isEdit, form]);

  const onFinish = async (values: any) => {
    if (orgId === 0) return;
    setLoading(true);
    try {
      if (isEdit) {
        await authorizedAxios.put(`/api/inventory/menu-items/${orgId}/${id}`, values);
        message.success('Menu item updated');
      } else {
        await authorizedAxios.post(`/api/inventory/menu-items/${orgId}`, values);
        message.success('Menu item created');
      }
      navigate('/menu-items');
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to save');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <Title level={3}>{isEdit ? 'Edit Menu Item' : 'New Menu Item'}</Title>
      <Card>
        <Form form={form} layout="vertical" onFinish={onFinish} initialValues={{ sellingPrice: 0, isActive: true }}>
          <Form.Item name="name" label="Name" rules={[{ required: true, message: 'Name is required' }]}>
            <Input placeholder="e.g., Caesar Salad" />
          </Form.Item>
          <Form.Item name="category" label="Category">
            <Input placeholder="e.g., Salads" />
          </Form.Item>
          <Form.Item name="sellingPrice" label="Selling Price (CHF)" rules={[{ required: true, message: 'Required' }, { type: 'number', min: 0, message: 'Must be ≥ 0' }]}>
            <InputNumber style={{ width: '100%' }} min={0} step={0.01} placeholder="0.00" />
          </Form.Item>
          <Form.Item name="externalId" label="External ID">
            <Input placeholder="Optional external reference" />
          </Form.Item>
          {isEdit && (
            <Form.Item name="isActive" label="Active" valuePropName="checked">
              <Switch />
            </Form.Item>
          )}
          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" loading={loading}>Save</Button>
              <Button onClick={() => navigate('/menu-items')}>Cancel</Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};
