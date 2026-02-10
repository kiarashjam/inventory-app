import React, { useEffect, useState } from 'react';
import { Form, Input, InputNumber, Select, Switch, Button, Card, Typography, Space, message } from 'antd';
import { useNavigate, useParams } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;
const { TextArea } = Input;

const locationTypeOptions = [
  { value: 0, label: 'Walk-in Cooler' },
  { value: 1, label: 'Freezer' },
  { value: 2, label: 'Dry Storage' },
  { value: 3, label: 'Bar' },
  { value: 4, label: 'Wine Cellar' },
  { value: 5, label: 'Prep Station' },
  { value: 6, label: 'Display Case' },
  { value: 7, label: 'Other' },
];

export const StorageLocationForm: React.FC = () => {
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
      .get(`/api/inventory/storage-locations/${orgId}/${id}`)
      .then((r) => {
        if (mounted) {
          const data = r.data?.data ?? r.data;
          if (data) form.setFieldsValue(data);
        }
      })
      .catch(() => {
        if (mounted) message.error('Failed to load location');
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
        await authorizedAxios.put(`/api/inventory/storage-locations/${orgId}/${id}`, values);
        message.success('Storage location updated');
      } else {
        await authorizedAxios.post(`/api/inventory/storage-locations/${orgId}`, values);
        message.success('Storage location created');
      }
      navigate('/storage-locations');
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to save');
    } finally {
      setLoading(false);
    }
  };

  if (orgId === 0) return null;

  return (
    <div>
      <Title level={3}>{isEdit ? 'Edit Storage Location' : 'New Storage Location'}</Title>
      <Card>
        <Form form={form} layout="vertical" onFinish={onFinish} initialValues={{ locationType: 0, displayOrder: 0, isActive: true }}>
          <Form.Item name="name" label="Name" rules={[{ required: true, message: 'Name is required' }]}>
            <Input placeholder="Location name" />
          </Form.Item>
          <Form.Item name="description" label="Description">
            <TextArea rows={2} placeholder="Optional description" />
          </Form.Item>
          <Form.Item name="locationType" label="Location Type" rules={[{ required: true }]}>
            <Select options={locationTypeOptions} placeholder="Select type" />
          </Form.Item>
          <Form.Item name="displayOrder" label="Display Order">
            <InputNumber style={{ width: '100%' }} min={0} />
          </Form.Item>
          <Space style={{ width: '100%' }} size="middle">
            <Form.Item name="temperatureMin" label="Temperature Min (°C)" style={{ flex: 1 }}>
              <InputNumber style={{ width: '100%' }} placeholder="Min" />
            </Form.Item>
            <Form.Item name="temperatureMax" label="Temperature Max (°C)" style={{ flex: 1 }}>
              <InputNumber style={{ width: '100%' }} placeholder="Max" />
            </Form.Item>
          </Space>
          <Form.Item name="isActive" label="Active" valuePropName="checked">
            <Switch />
          </Form.Item>
          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" loading={loading}>Save</Button>
              <Button onClick={() => navigate('/storage-locations')}>Cancel</Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};
