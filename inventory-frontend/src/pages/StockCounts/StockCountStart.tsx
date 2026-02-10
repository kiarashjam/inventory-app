import React, { useEffect, useState } from 'react';
import { Form, Input, Select, Button, Card, Typography, Space, message } from 'antd';
import { useNavigate } from 'react-router';
import { useSelector } from 'react-redux';
import type { RootState } from '../../redux/store';
import { authorizedAxios } from '../../config/axios/axios-config';

const { Title } = Typography;
const { TextArea } = Input;

export const StockCountStart: React.FC = () => {
  const navigate = useNavigate();
  const { user } = useSelector((state: RootState) => state.auth);
  const orgId = user?.organizationId || 0;
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [categories, setCategories] = useState<any[]>([]);
  const [locations, setLocations] = useState<any[]>([]);

  useEffect(() => {
    let mounted = true;
    if (orgId === 0) return;
    Promise.all([
      authorizedAxios.get(`/api/inventory/categories/${orgId}`),
      authorizedAxios.get(`/api/inventory/storage-locations/${orgId}`),
    ])
      .then(([catRes, locRes]) => {
        if (mounted) {
          const catData = catRes.data?.data ?? catRes.data;
          const locData = locRes.data?.data ?? locRes.data;
          setCategories(Array.isArray(catData) ? catData : []);
          setLocations(Array.isArray(locData) ? locData : []);
        }
      })
      .catch(() => {});
    return () => { mounted = false; };
  }, [orgId]);

  const onFinish = async (values: any) => {
    if (orgId === 0) return;
    setLoading(true);
    try {
      const res = await authorizedAxios.post(`/api/inventory/stock-counts/${orgId}`, {
        notes: values.notes,
        categoryId: values.categoryId || undefined,
        storageLocationId: values.storageLocationId || undefined,
      });
      const created = res.data?.data ?? res.data;
      const countId = created?.id ?? created?.stockCountId;
      if (countId) {
        message.success('Stock count started');
        navigate(`/stock-counts/${countId}`);
      } else {
        message.success('Stock count started');
        navigate('/stock-counts');
      }
    } catch (e: any) {
      message.error(e.response?.data?.message || 'Failed to start count');
    } finally {
      setLoading(false);
    }
  };

  if (orgId === 0) return null;

  return (
    <div>
      <Title level={3}>Start New Stock Count</Title>
      <Card>
        <Form form={form} layout="vertical" onFinish={onFinish}>
          <Form.Item name="notes" label="Notes">
            <TextArea rows={3} placeholder="Optional notes for this count" />
          </Form.Item>
          <Form.Item name="categoryId" label="Category filter (optional)">
            <Select
              allowClear
              placeholder="Filter by category"
              options={categories.map((c: any) => ({ value: c.id, label: c.name }))}
            />
          </Form.Item>
          <Form.Item name="storageLocationId" label="Storage Location filter (optional)">
            <Select
              allowClear
              placeholder="Filter by storage location"
              options={locations.map((l: any) => ({ value: l.id, label: l.name }))}
            />
          </Form.Item>
          <Form.Item>
            <Space>
              <Button type="primary" htmlType="submit" loading={loading}>Start Count</Button>
              <Button onClick={() => navigate('/stock-counts')}>Cancel</Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};
